using Microsoft.EntityFrameworkCore;
using PensionMS.Infrastructure.Reporting.Exporters;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Reports;
using PensionMS.Core.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PensionMS.Infrastructure.Services.Reporting;

public partial class ReportService
{
    // ═══════════════════════════════════════
    // DOC-01: Missing Documents
    // ═══════════════════════════════════════
    public async Task<ApiResponse<PagedResult<FileMissingDocsRow>>> GetMissingDocsReportAsync(ReportFilter filter)
    {
        var queries = await _access.ForPermissionAsync("Report.Documents.View");
        // ── الخطوة 1: جلب الملفات التي لها مستندات ناقصة مسجلة يدوياً ──
        var manualMissingFiles = await queries.Query(_unitOfWork.MissingDocuments, filter.BranchId)
            .AsNoTracking()
            .Include(m => m.DocumentType)
            .Include(m => m.Agent)
            .Include(m => m.Guardian)
            .Include(m => m.EntitledPerson)
            .Where(m => m.Status != MissingDocumentStatusConstants.Fulfilled)
            .Where(m =>
                (m.AgentID == null && m.GuardianID == null && m.EntitledID == null)
                || (m.AgentID != null && m.Agent != null && m.Agent.IsActive && !m.Agent.IsDeleted)
                || (m.GuardianID != null && m.Guardian != null && m.Guardian.IsActive && !m.Guardian.IsDeleted)
                || (m.EntitledID != null && m.EntitledPerson != null && m.EntitledPerson.IsActive && !m.EntitledPerson.IsDeceased)
            )
            .ToListAsync();

        // تجميع حسب FileID
        var manualDocsByFile = manualMissingFiles
            .GroupBy(m => m.FileID)
            .ToDictionary(g => g.Key, g => g.ToList());

        var manualFileIds = manualDocsByFile.Keys.ToList();

        // ── الخطوة 2: جلب كل الملفات النشطة (للفحص التلقائي) ──
        var allActiveFileIds = await queries.Query(_unitOfWork.RetirementFiles, filter.BranchId)
            .AsNoTracking()
            .Where(f => !f.IsDeleted)
            .Select(f => f.FileID)
            .ToListAsync();

        // دمج: الملفات التي لها نواقص يدوية + كل الملفات النشطة
        var candidateFileIds = manualFileIds.Union(allActiveFileIds).Distinct().ToList();

        // ── الخطوة 3: فلتر البحث ──
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var searchFileIds = await queries.Query(_unitOfWork.RetirementFiles, filter.BranchId)
                .AsNoTracking()
                .Where(f => !f.IsDeleted && (
                    (f.FileNumber != null && f.FileNumber.Contains(filter.Search)) ||
                    (f.Retiree != null && f.Retiree.FullName != null && f.Retiree.FullName.Contains(filter.Search))
                ))
                .Select(f => f.FileID)
                .ToListAsync();

            candidateFileIds = candidateFileIds.Intersect(searchFileIds).ToList();
        }

        // ── الخطوة 4: جلب بيانات الملفات ──
        var fileData = await queries.Query(_unitOfWork.RetirementFiles, filter.BranchId)
            .AsNoTracking()
            .Include(f => f.Retiree)
            .Include(f => f.Agents.Where(a => a.IsActive && !a.IsDeleted))
            .Where(f => candidateFileIds.Contains(f.FileID))
            .Select(f => new
            {
                f.FileID,
                f.FileNumber,
                f.FileOpenDate,
                RetireeName = f.Retiree != null ? f.Retiree.FullName : "",
                MilitaryNumber = f.Retiree != null ? f.Retiree.MilitaryNumber : null,
                Phone = f.Retiree != null ? f.Retiree.Phone : null,
                AgentPhone = string.Join("، ", f.Agents.Where(a => a.IsActive && !a.IsDeleted).Select(a => a.Phone).Where(p => p != null))
            })
            .ToListAsync();

        // ── الخطوة 5: تجميع النتائج الأولية (بدون فحص تلقائي) ──
        var results = new List<FileMissingDocsRow>();

        foreach (var file in fileData)
        {
            var missingDocs = new List<string>();
            int maxDays = 0;

            // المستندات المسجلة يدوياً
            if (manualDocsByFile.TryGetValue(file.FileID, out var manualDocs))
            {
                foreach (var m in manualDocs)
                {
                    string entityInfo = "";
                    if (m.AgentID != null && m.Agent != null)
                        entityInfo = $"وكيل: {m.Agent.FullName}";
                    else if (m.GuardianID != null && m.Guardian != null)
                        entityInfo = $"وصي: {m.Guardian.FullName}";
                    else if (m.EntitledID != null && m.EntitledPerson != null)
                        entityInfo = $"مستحق: {m.EntitledPerson.FullName}";
                    else
                        entityInfo = $"المتقاعد: {file.RetireeName}";

                    missingDocs.Add($"{m.DocumentType?.TypeName ?? "مستند"} ({entityInfo})");
                    int days = (int)(DateTime.UtcNow - m.CreatedAt).TotalDays;
                    if (days > maxDays) maxDays = days;
                }
            }

            if (missingDocs.Count > 0)
            {
                results.Add(new FileMissingDocsRow
                {
                    FileID = file.FileID,
                    FileNumber = file.FileNumber,
                    RetireeName = file.RetireeName,
                    MilitaryNumber = file.MilitaryNumber,
                    Phone = file.Phone,
                    AgentPhone = file.AgentPhone,
                    MissingCount = missingDocs.Count,
                    MissingDocsList = string.Join("، ", missingDocs.Distinct()),
                    MaxDaysMissing = maxDays
                });
            }
        }

        // ── الخطوة 6: الترتيب ──
        results = results.OrderByDescending(r => r.MaxDaysMissing).ToList();

        // ── الخطوة 7: الصفحة الحالية ──
        var totalCount = results.Count;
        var pagedData = results
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToList();

        // ── الخطوة 8: الفحص التلقائي فقط للصفحة الحالية ──
        var pageFileIds = pagedData.Select(r => r.FileID).ToList();
        var autoDetectResults = new Dictionary<int, List<string>>();

        foreach (var fileId in pageFileIds)
        {
            var autoRes = await _missingDocumentService.GetAutoDetectedByFileAsync(fileId);
            if (autoRes.IsSuccess && autoRes.Data != null && autoRes.Data.Count > 0)
            {
                autoDetectResults[fileId] = autoRes.Data.Select(ad => ad.DocumentTypeName).ToList();
            }
        }

        // دمج النتائج
        foreach (var row in pagedData)
        {
            if (autoDetectResults.TryGetValue(row.FileID, out var autoDocs) && autoDocs.Count > 0)
            {
                var existingDocs = row.MissingDocsList.Split('،').Select(s => s.Trim()).ToList();
                var newDocs = autoDocs.Where(d => !existingDocs.Any(e => e.Contains(d))).ToList();
                if (newDocs.Count > 0)
                {
                    row.MissingDocsList = string.Join("، ", existingDocs.Concat(newDocs));
                    row.MissingCount = existingDocs.Count + newDocs.Count;
                }
            }
        }

        return ApiResponse<PagedResult<FileMissingDocsRow>>.Success(new PagedResult<FileMissingDocsRow>
        {
            Items = pagedData,
            TotalCount = totalCount,
            Page = filter.PageNumber,
            PageSize = filter.PageSize
        });
    }

    // ═══════════════════════════════════════
    // DOC-01: Missing Documents by Person (beneficiaries/heirs)
    // ═══════════════════════════════════════
    public async Task<ApiResponse<PagedResult<MissingDocsByPersonRow>>> GetMissingDocsByPersonReportAsync(ReportFilter filter)
    {
        var queries = await _access.ForPermissionAsync("Report.Documents.View");
        // ── الخطوة 1: جلب المستندات اليدوية الناقصة ──
        var manualMissingDocs = await queries.Query(_unitOfWork.MissingDocuments, filter.BranchId)
            .AsNoTracking()
            .Include(m => m.DocumentType)
            .Include(m => m.Agent)
            .Include(m => m.Guardian)
            .Include(m => m.EntitledPerson)
            .Where(m => m.Status != MissingDocumentStatusConstants.Fulfilled)
            .Where(m =>
                (m.AgentID == null && m.GuardianID == null && m.EntitledID == null)
                || (m.AgentID != null && m.Agent != null && m.Agent.IsActive && !m.Agent.IsDeleted)
                || (m.GuardianID != null && m.Guardian != null && m.Guardian.IsActive && !m.Guardian.IsDeleted)
                || (m.EntitledID != null && m.EntitledPerson != null && m.EntitledPerson.IsActive && !m.EntitledPerson.IsDeceased)
            )
            .ToListAsync();

        var fileIds = manualMissingDocs.Select(m => m.FileID).Distinct().ToList();

        // ── الخطوة 2: جلب بيانات الملفات ──
        var filesData = await queries.Query(_unitOfWork.RetirementFiles, filter.BranchId)
            .AsNoTracking()
            .Include(f => f.Retiree)
            .Include(f => f.EntitledPersons.Where(ep => !ep.IsDeleted))
            .Include(f => f.Guardians.Where(g => !g.IsDeleted && g.IsActive))
            .Include(f => f.Agents.Where(a => !a.IsDeleted && a.IsActive))
            .Where(f => fileIds.Contains(f.FileID))
            .ToListAsync();

        // ── الخطوة 3: فلتر البحث ──
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            filesData = filesData.Where(f =>
                (f.FileNumber != null && f.FileNumber.Contains(filter.Search)) ||
                (f.Retiree != null && f.Retiree.FullName != null && f.Retiree.FullName.Contains(filter.Search)) ||
                f.EntitledPersons.Any(ep => ep.FullName.Contains(filter.Search)) ||
                f.Guardians.Any(g => g.FullName.Contains(filter.Search)) ||
                f.Agents.Any(a => a.FullName.Contains(filter.Search))
            ).ToList();
        }

        // ── الخطوة 4: تجميع النتائج حسب_person ──
        var results = new List<MissingDocsByPersonRow>();

        foreach (var file in filesData)
        {
            var fileManualDocs = manualMissingDocs.Where(m => m.FileID == file.FileID).ToList();

            // المستندات الناقصة للمتقاعد نفسه
            var retireeDocs = fileManualDocs.Where(m => m.AgentID == null && m.GuardianID == null && m.EntitledID == null).ToList();
            if (retireeDocs.Count > 0)
            {
                results.Add(new MissingDocsByPersonRow
                {
                    FileID = file.FileID,
                    FileNumber = file.FileNumber,
                    RetireeName = file.Retiree?.FullName ?? "",
                    MilitaryNumber = file.Retiree?.MilitaryNumber,
                    PersonType = "متقاعد",
                    PersonName = file.Retiree?.FullName ?? "",
                    Relationship = null,
                    PersonPhone = file.Retiree?.Phone,
                    MissingCount = retireeDocs.Count,
                    MissingDocsList = string.Join("، ", retireeDocs.Select(d => d.DocumentType?.TypeName ?? "مستند")),
                    MaxDaysMissing = retireeDocs.Max(d => (int)(DateTime.UtcNow - d.CreatedAt).TotalDays)
                });
            }

            // المستندات الناقصة للمستحقين (الورثة/المستفيدين)
            foreach (var ep in file.EntitledPersons.Where(ep => ep.IsActive && !ep.IsDeceased))
            {
                var epDocs = fileManualDocs.Where(m => m.EntitledID == ep.EntitledID).ToList();
                if (epDocs.Count > 0)
                {
                    results.Add(new MissingDocsByPersonRow
                    {
                        FileID = file.FileID,
                        FileNumber = file.FileNumber,
                        RetireeName = file.Retiree?.FullName ?? "",
                        MilitaryNumber = file.Retiree?.MilitaryNumber,
                        PersonType = "مستحق",
                        PersonName = ep.FullName,
                        Relationship = ep.Relationship?.RelationshipName,
                        PersonPhone = ep.Phone,
                        MissingCount = epDocs.Count,
                        MissingDocsList = string.Join("، ", epDocs.Select(d => d.DocumentType?.TypeName ?? "مستند")),
                        MaxDaysMissing = epDocs.Max(d => (int)(DateTime.UtcNow - d.CreatedAt).TotalDays)
                    });
                }
            }

            // المستندات الناقصة للأوصياء
            foreach (var g in file.Guardians.Where(g => g.IsActive))
            {
                var gDocs = fileManualDocs.Where(m => m.GuardianID == g.GuardianID).ToList();
                if (gDocs.Count > 0)
                {
                    results.Add(new MissingDocsByPersonRow
                    {
                        FileID = file.FileID,
                        FileNumber = file.FileNumber,
                        RetireeName = file.Retiree?.FullName ?? "",
                        MilitaryNumber = file.Retiree?.MilitaryNumber,
                        PersonType = "وصي",
                        PersonName = g.FullName,
                        Relationship = null,
                        PersonPhone = g.Phone,
                        MissingCount = gDocs.Count,
                        MissingDocsList = string.Join("، ", gDocs.Select(d => d.DocumentType?.TypeName ?? "مستند")),
                        MaxDaysMissing = gDocs.Max(d => (int)(DateTime.UtcNow - d.CreatedAt).TotalDays)
                    });
                }
            }

            // المستندات الناقصة للوكلاء
            foreach (var a in file.Agents.Where(a => a.IsActive))
            {
                var aDocs = fileManualDocs.Where(m => m.AgentID == a.AgentID).ToList();
                if (aDocs.Count > 0)
                {
                    results.Add(new MissingDocsByPersonRow
                    {
                        FileID = file.FileID,
                        FileNumber = file.FileNumber,
                        RetireeName = file.Retiree?.FullName ?? "",
                        MilitaryNumber = file.Retiree?.MilitaryNumber,
                        PersonType = "وكيل",
                        PersonName = a.FullName,
                        Relationship = null,
                        PersonPhone = a.Phone,
                        MissingCount = aDocs.Count,
                        MissingDocsList = string.Join("، ", aDocs.Select(d => d.DocumentType?.TypeName ?? "مستند")),
                        MaxDaysMissing = aDocs.Max(d => (int)(DateTime.UtcNow - d.CreatedAt).TotalDays)
                    });
                }
            }
        }

        // ── الخطوة 5: الترتيب والصفحة ──
        results = results.OrderByDescending(r => r.MaxDaysMissing).ToList();

        var totalCount = results.Count;
        var pagedData = results
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToList();

        return ApiResponse<PagedResult<MissingDocsByPersonRow>>.Success(new PagedResult<MissingDocsByPersonRow>
        {
            Items = pagedData,
            TotalCount = totalCount,
            Page = filter.PageNumber,
            PageSize = filter.PageSize
        });
    }

    // ═══════════════════════════════════════
    // DOC-02: Expired Documents
    // ═══════════════════════════════════════
    public async Task<ApiResponse<PagedResult<ExpiredDocRow>>> GetExpiredDocsReportAsync(ReportFilter filter)
    {
        var queries = await _access.ForPermissionAsync("Report.Documents.View");
        var query = queries.Query(_unitOfWork.Documents, filter.BranchId)
            .AsNoTracking()
            .Include(d => d.DocumentType)
            .Where(d => !d.IsDeleted && d.ExpiryDate.HasValue && d.ExpiryDate.Value < PensionMS.Core.Time.YemenTime.Today)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.Where(d => d.DocumentType != null && d.DocumentType.TypeName.Contains(filter.Search));

        var totalCount = await query.CountAsync();
        var expiredDocs = await query
            .OrderByDescending(d => d.ExpiryDate).ThenBy(d => d.DocumentID)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var data = expiredDocs.Select(d => new ExpiredDocRow
        {
            DocumentID = d.DocumentID,
            DocTypeName = d.DocumentType?.TypeName ?? "",
            OwnerName = "Unknown",
            OwnerType = "Various",
            ExpiryDate = d.ExpiryDate,
            DaysExpired = PensionMS.Core.Time.YemenTime.Today.DayNumber - d.ExpiryDate!.Value.DayNumber,
            IsVerified = d.Verified
        }).ToList();

        return ApiResponse<PagedResult<ExpiredDocRow>>.Success(new PagedResult<ExpiredDocRow>
        {
            Items = data,
            TotalCount = totalCount,
            Page = filter.PageNumber,
            PageSize = filter.PageSize
        });
    }
}
