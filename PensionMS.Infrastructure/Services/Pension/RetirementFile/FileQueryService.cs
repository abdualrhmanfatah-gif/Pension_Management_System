using Microsoft.EntityFrameworkCore;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Documents;
using PensionMS.Core.DTOs.Discrepancy;
using PensionMS.Core.DTOs.RetirementFiles;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Helpers;
using PensionMS.Infrastructure.Helpers;

using System.Text.Json;

namespace PensionMS.Infrastructure.Services.Pension.RetirementFile;

public class FileQueryService : IFileQueryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMissingDocumentService _missingDocumentService;
    private readonly IAccessiblePensionRepository _access;
    private readonly IPensionSuspensionService _pensionSuspensionService;
    private readonly IDocumentService _documentService;
    private readonly RetirementFileCompletenessEvaluator _completeness;

    public FileQueryService(IUnitOfWork unitOfWork,
        IMissingDocumentService missingDocumentService, IAccessiblePensionRepository access,
        IPensionSuspensionService pensionSuspensionService, IDocumentService documentService)
    {
        _unitOfWork = unitOfWork;
        _missingDocumentService = missingDocumentService;
        _access = access;
        _pensionSuspensionService = pensionSuspensionService;
        _documentService = documentService;
        _completeness = new RetirementFileCompletenessEvaluator(unitOfWork, missingDocumentService);
    }

    public async Task<ApiResponse<RetirementFileDto>> GetByIdAsync(int id)
    {
        var queries = await _access.ForPermissionAsync("File.View.Sensitive");
        var file = await queries.Files()
            .AsNoTracking()
            .Include(f => f.Retiree)
            .Include(f => f.Rank)
            .Include(f => f.PensionBranch)
            .Include(f => f.Status)
            .Include(f => f.AssignedEmployee)
            .FirstOrDefaultAsync(f => f.FileID == id);

        if (file == null)
            return ApiResponse<RetirementFileDto>.Fail("الملف غير موجود");
        if (file.Retiree == null)
            return ApiResponse<RetirementFileDto>.Fail("بيانات المتقاعد غير مرتبطة بهذا الملف");
        var salaryStatus = await _pensionSuspensionService.GetSalaryStatusAsync(file.FileID);
        if (!salaryStatus.IsSuccess) return ApiResponse<RetirementFileDto>.Fail(salaryStatus.Message!);
        var identity = await _documentService.GetRetireeIdentityPathAsync(file.FileID);
        if (!identity.IsSuccess) return ApiResponse<RetirementFileDto>.Fail(identity.Message!);

        // Get active payment book
        var activeBook = await _unitOfWork.PaymentBooks.Query()
            .AsNoTracking()
            .Where(pb => pb.FileID == id && pb.Status == PaymentBookStatus.Active)
            .OrderByDescending(pb => pb.CreatedAt)
            .FirstOrDefaultAsync();

        // Get counts for statistics
        var entitledCount = await _unitOfWork.EntitledPersons.Query()
            .AsNoTracking()
            .CountAsync(e => e.FileID == id && !e.IsDeleted);
        
        var agentsCount = await _unitOfWork.Agents.Query()
            .AsNoTracking()
            .CountAsync(a => a.FileID == id && !a.IsDeleted);
        
        var guardiansCount = await _unitOfWork.Guardians.Query()
            .AsNoTracking()
            .CountAsync(g => g.FileID == id && !g.IsDeleted);

        var compliance = await _missingDocumentService.GetComplianceSummaryAsync(id);
        var missingDocsList = await _completeness.GetMissingDocsListAsync(id);
        var missingData = await _completeness.CalculateMissingDataAsync(id);

        var hasMinors = await _unitOfWork.EntitledPersons.Query()
            .AsNoTracking()
            .AnyAsync(ep => ep.FileID == id && !ep.IsDeleted && ep.IsActive
                           && ep.BirthDate.AddYears(18) > PensionMS.Core.Time.YemenTime.Today);

        var hasPledges = await _unitOfWork.DocumentPledges.Query()
            .AsNoTracking()
            .AnyAsync(p => p.FileID == id && !p.IsDeleted);

        var hasExemptions = await _unitOfWork.DocumentExemptions.Query()
            .AsNoTracking()
            .AnyAsync(x => x.FileID == id);

        var retireeMilNum = StringHelpers.NormalizeDigits(file.Retiree?.MilitaryNumber);
        var retireeApprovalId = file.Retiree?.ApprovalItemID ?? 0;

        // Check settlement by RetireeID or ApprovalItemID (translated to SQL)
        var hasSettlements = await _unitOfWork.SettlementItems.Query()
            .AsNoTracking()
            .AnyAsync(s => !s.IsDeleted && (
                s.RetireeID == file.RetireeID ||
                (s.ApprovalItemID != null && retireeApprovalId > 0 && s.ApprovalItemID == retireeApprovalId)
            ));

        // Check by MilitaryNumber using NormalizeDigits (must be evaluated in memory)
        if (!hasSettlements && !string.IsNullOrEmpty(retireeMilNum))
        {
            var milCandidates = await _unitOfWork.SettlementItems.Query()
                .AsNoTracking()
                .Where(s => !s.IsDeleted && s.MilitaryNumber != null)
                .ToListAsync();
            hasSettlements = milCandidates.Any(s => StringHelpers.NormalizeDigits(s.MilitaryNumber) == retireeMilNum);
        }

        var totalMissingCount = compliance.MissingCount;
        var hasPledgesOrExemptions = totalMissingCount > 0 || hasPledges || hasExemptions;

        return ApiResponse<RetirementFileDto>.Success(RetirementFileDtoMapper.MapToDto(file, file.Retiree, new RetirementFileDto
        {
            FileStatusName = FileStatusConstants.Name(file.StatusID ?? 0),
            SalaryStatus = salaryStatus.Data!, IdDocumentPath = identity.Data,
            EntitledPersonsCount = entitledCount, AgentsCount = agentsCount, GuardiansCount = guardiansCount,
            MissingDocumentsCount = missingDocsList.Count, MissingDocumentsList = missingDocsList,
            MissingDataCount = missingData.Count, MissingDataList = missingData.Items,
            ActivePaymentBookNumber = activeBook?.BookNumber, ActivePaymentBookExpiry = activeBook?.ExpiryDate,
            ActivePaymentBookStatus = activeBook?.Status,
            HasMinors = hasMinors, HasPledgesOrExemptions = hasPledgesOrExemptions, HasSettlements = hasSettlements
        }));
    }

    public async Task<ApiResponse<PagedResult<RetirementFileListDto>>> GetAllAsync(int page, int pageSize, string? search = null, int? statusId = null, int? userId = null, string? statusIds = null, int? uploadedBy = null, int? cursor = null, string? period = null, DateTime? fromDate = null, DateTime? toDate = null, int? categoryId = null, int? rankId = null, string? governorate = null, byte? gender = null, string? unitName = null, bool? hasSettlements = null)
    {
        var queries = await _access.ForPermissionAsync("File.View.Sensitive");
        var query = queries.Files()
            .AsNoTracking()
            .AsQueryable();

        // ── فلترة حسب نطاق فرع المستخدم ──
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(f => f.FileNumber.Contains(search) ||
                                     f.Retiree.FullName.Contains(search) ||
                                     f.Retiree.NationalID.Contains(search) ||
                                     f.Retiree.MilitaryNumber.Contains(search));

        if (!string.IsNullOrWhiteSpace(statusIds))
        {
            var idList = statusIds.Split(',')
                .Select(s => int.TryParse(s.Trim(), out var v) ? v : 0)
                .Where(v => v > 0)
                .ToList();

            if (idList.Count > 0)
            {
                query = query.Where(f => f.StatusID.HasValue && idList.Contains(f.StatusID.Value));
            }
        }
        else if (statusId.HasValue)
        {
            query = query.Where(f => f.StatusID == statusId);
        }

        if (uploadedBy.HasValue)
        {
            query = query.Where(f => f.Documents.Any(d => d.UploadedByUserID == uploadedBy.Value));
        }

        // ── فلترة حسب الفئة/الرتبة/المحافظة/الجنس ──
        if (categoryId.HasValue)
            query = query.Where(f => f.Retiree.ApprovalItem.CategoryID == categoryId.Value);
        if (rankId.HasValue)
            query = query.Where(f => f.RankID == rankId.Value);
        if (!string.IsNullOrWhiteSpace(governorate))
            query = query.Where(f => f.Retiree.Governorate == governorate);
        if (gender.HasValue)
            query = query.Where(f => f.Retiree.Gender == gender.Value);
        if (!string.IsNullOrWhiteSpace(unitName))
            query = query.Where(f => f.Retiree.ApprovalItem != null && f.Retiree.ApprovalItem.UnitName == unitName);
        if (hasSettlements.HasValue)
        {
            if (hasSettlements.Value)
                query = query.Where(f => f.Retiree.SettlementItems.Any());
            else
                query = query.Where(f => !f.Retiree.SettlementItems.Any());
        }

        // ── فلترة حسب الفترة ──
        if (!string.IsNullOrWhiteSpace(period))
        {
            var (periodFrom, periodTo) = PensionMS.Core.Time.YemenTime.PeriodUtc(period);
            query = query.Where(f => f.Documents.Any(d => d.CreatedAt >= periodFrom && d.CreatedAt < periodTo));
        }
        else if (fromDate.HasValue || toDate.HasValue)
        {
            var from = fromDate.HasValue
                ? PensionMS.Core.Time.YemenTime.StartUtc(DateOnly.FromDateTime(fromDate.Value))
                : DateTime.MinValue;
            var to = toDate.HasValue
                ? PensionMS.Core.Time.YemenTime.StartUtc(DateOnly.FromDateTime(toDate.Value).AddDays(1))
                : DateTime.MaxValue;
            query = query.Where(f => f.Documents.Any(d => d.CreatedAt >= from && d.CreatedAt < to));
        }

        if (cursor.HasValue)
        {
            var items = await query
                .Where(f => f.FileID < cursor.Value)
                .OrderByDescending(f => f.FileID)
                .Take(pageSize + 1)
                .Select(f => new RetirementFileListDto
                {
                    FileID = f.FileID,
                    FileNumber = f.FileNumber,
                    RetireeFullName = f.Retiree != null ? f.Retiree.FullName : "غير محدد",
                    NationalID = f.Retiree != null ? f.Retiree.NationalID : "غير محدد",
                    MilitaryNumber = f.Retiree != null ? f.Retiree.MilitaryNumber : null,
                    RankName = f.Rank != null ? f.Rank.RankName : null,
                    StatusID = f.StatusID,
                    FileStatusName = f.Status != null ? f.Status.StatusName : null,
                    FileOpenDate = f.FileOpenDate,
                    CreatedAt = f.CreatedAt,
                    ActivePaymentBookNumber = f.PaymentBooks
                        .Where(pb => pb.Status == PaymentBookStatus.Active && !pb.IsDeleted)
                        .OrderByDescending(pb => pb.CreatedAt)
                        .Select(pb => pb.BookNumber)
                        .FirstOrDefault(),
                    MissingDocumentsCount = f.MissingDocuments
                        .Count(m => (m.Status == PensionMS.Core.Constants.MissingDocumentStatusConstants.Pledged)),
                    LastUploadedByUserID = f.Documents
                        .OrderByDescending(d => d.CreatedAt)
                        .Select(d => d.UploadedByUserID)
                        .FirstOrDefault(),
                    LastUploadedByUserName = f.Documents
                        .OrderByDescending(d => d.CreatedAt)
                        .Select(d => d.UploadedByUser.FullName)
                        .FirstOrDefault(),
                    LastUploadDate = f.Documents
                        .OrderByDescending(d => d.CreatedAt)
                        .Select(d => d.CreatedAt)
                        .FirstOrDefault()
                })
                .ToListAsync();

            bool hasMore = items.Count > pageSize;
            if (hasMore) items = items.Take(pageSize).ToList();

            return ApiResponse<PagedResult<RetirementFileListDto>>.Success(new PagedResult<RetirementFileListDto>
            {
                Items = items,
                TotalCount = 0,
                Page = page,
                PageSize = pageSize,
                NextCursor = items.Count > 0 ? items.Last().FileID : null,
                HasMore = hasMore
            });
        }

        var total = await query.CountAsync();
        var itemsQuery = await query
            .OrderByDescending(f => f.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(f => new RetirementFileListDto
            {
                FileID = f.FileID,
                FileNumber = f.FileNumber,
                RetireeFullName = f.Retiree != null ? f.Retiree.FullName : "غير محدد",
                NationalID = f.Retiree != null ? f.Retiree.NationalID : "غير محدد",
                MilitaryNumber = f.Retiree != null ? f.Retiree.MilitaryNumber : null,
                RankName = f.Rank != null ? f.Rank.RankName : null,
                StatusID = f.StatusID,
                FileStatusName = f.Status != null ? f.Status.StatusName : null,
                FileOpenDate = f.FileOpenDate,
                CreatedAt = f.CreatedAt,
                ActivePaymentBookNumber = f.PaymentBooks
                    .Where(pb => pb.Status == PaymentBookStatus.Active && !pb.IsDeleted)
                    .OrderByDescending(pb => pb.CreatedAt)
                    .Select(pb => pb.BookNumber)
                    .FirstOrDefault(),
                // Fix N+1: Project counts directly
                MissingDocumentsCount = f.MissingDocuments
                    .Count(m => (m.Status == PensionMS.Core.Constants.MissingDocumentStatusConstants.Pending || 
                                 m.Status == PensionMS.Core.Constants.MissingDocumentStatusConstants.Pledged)),
                HasDocuments = f.Documents.Any(),
                LastUploadedByUserID = f.Documents
                    .OrderByDescending(d => d.CreatedAt)
                    .Select(d => d.UploadedByUserID)
                    .FirstOrDefault(),
                LastUploadedByUserName = f.Documents
                    .OrderByDescending(d => d.CreatedAt)
                    .Select(d => d.UploadedByUser.FullName)
                    .FirstOrDefault(),
                LastUploadDate = f.Documents
                    .OrderByDescending(d => d.CreatedAt)
                    .Select(d => d.CreatedAt)
                    .FirstOrDefault()
            })
            .ToListAsync();

        return ApiResponse<PagedResult<RetirementFileListDto>>.Success(new PagedResult<RetirementFileListDto>
        {
            Items = itemsQuery, TotalCount = total, Page = page, PageSize = pageSize
        });
    }

    public async Task<ApiResponse<RetirementFileDto>> GetByFileNumberAsync(string fileNumber)
    {
        var queries = await _access.ForPermissionAsync("File.View.Sensitive");
        var file = await queries.Files()
            .AsNoTracking()
            .Include(f => f.Retiree)
            .Include(f => f.Rank)
            .Include(f => f.PensionBranch)
            .Include(f => f.Status)
            .FirstOrDefaultAsync(f => f.FileNumber == fileNumber);

        if (file == null)
            return ApiResponse<RetirementFileDto>.Fail("الملف غير موجود");
        if (file.Retiree == null)
            return ApiResponse<RetirementFileDto>.Fail("بيانات المتقاعد غير مرتبطة بهذا الملف");
        var salaryStatus = await _pensionSuspensionService.GetSalaryStatusAsync(file.FileID);
        if (!salaryStatus.IsSuccess) return ApiResponse<RetirementFileDto>.Fail(salaryStatus.Message!);
        var identity = await _documentService.GetRetireeIdentityPathAsync(file.FileID);
        if (!identity.IsSuccess) return ApiResponse<RetirementFileDto>.Fail(identity.Message!);

        var activeBook = await _unitOfWork.PaymentBooks.Query()
            .AsNoTracking()
            .Where(pb => pb.FileID == file.FileID && pb.Status == PaymentBookStatus.Active)
            .OrderByDescending(pb => pb.CreatedAt)
            .FirstOrDefaultAsync();

        // حساب البيانات الناقصة
        var missingData = await _completeness.CalculateMissingDataAsync(file.FileID);
        var missingDocsList = await _completeness.GetMissingDocsListAsync(file.FileID);

        return ApiResponse<RetirementFileDto>.Success(RetirementFileDtoMapper.MapToDto(file, file.Retiree, new RetirementFileDto
        {
            FileStatusName = FileStatusConstants.Name(file.StatusID ?? 0),
            SalaryStatus = salaryStatus.Data!, IdDocumentPath = identity.Data,
            MissingDocumentsCount = missingDocsList.Count, MissingDocumentsList = missingDocsList,
            MissingDataCount = missingData.Count, MissingDataList = missingData.Items,
            ActivePaymentBookNumber = activeBook?.BookNumber, ActivePaymentBookExpiry = activeBook?.ExpiryDate,
            ActivePaymentBookStatus = activeBook?.Status
        }));
    }

    public async Task<ApiResponse<List<RetirementFileListDto>>> GetByRetireeIdAsync(int retireeId)
    {
        var queries = await _access.ForPermissionAsync("File.View.Sensitive");
        var files = await queries.Files()
            .AsNoTracking()
            .Where(f => f.RetireeID == retireeId)
            .Select(f => new RetirementFileListDto
            {
                FileID = f.FileID,
                FileNumber = f.FileNumber,
                RetireeFullName = f.Retiree != null ? f.Retiree.FullName : "غير محدد",
                NationalID = f.Retiree != null ? f.Retiree.NationalID : "غير محدد",
                MilitaryNumber = f.Retiree != null ? f.Retiree.MilitaryNumber : null,
                RankName = f.Rank != null ? f.Rank.RankName : null,
                StatusID = f.StatusID,
                FileStatusName = f.Status != null ? f.Status.StatusName : null,
                FileOpenDate = f.FileOpenDate,
                CreatedAt = f.CreatedAt,
                ActivePaymentBookNumber = f.PaymentBooks
                    .Where(pb => pb.Status == PaymentBookStatus.Active && !pb.IsDeleted)
                    .OrderByDescending(pb => pb.CreatedAt)
                    .Select(pb => pb.BookNumber)
                    .FirstOrDefault(),
                // Fix N+1: Project counts directly
                MissingDocumentsCount = f.MissingDocuments
                    .Count(m => (m.Status == PensionMS.Core.Constants.MissingDocumentStatusConstants.Pending || 
                                 m.Status == PensionMS.Core.Constants.MissingDocumentStatusConstants.Pledged))
            })
            .ToListAsync();

        return ApiResponse<List<RetirementFileListDto>>.Success(files);
    }

    public async Task<ApiResponse<List<StatusTransitionDto>>> GetStatusTransitionsAsync(int fileId)
    {
        var queries = await _access.ForPermissionAsync("File.View.Sensitive");
        if (!await queries.Files().AnyAsync(f => f.FileID == fileId))
            throw new PensionMS.Core.Exceptions.EntityNotFoundException("الملف غير موجود");
        // Get all status transitions for this file
        var transitions = await _unitOfWork.StatusTransitionLogs.Query()
            .AsNoTracking()
            .Where(log => log.TableID == SystemTableConstants.RetirementFiles && log.RecordID == fileId)
            .OrderByDescending(log => log.TransitionDate)
            .ToListAsync();

        if (transitions.Count == 0)
            return ApiResponse<List<StatusTransitionDto>>.Success(new List<StatusTransitionDto>());

        // Fetch ALL file statuses (small lookup table) to avoid OPENJSON/Contains SQL issues
        var allStatuses = await _unitOfWork.FileStatuses.Query()
            .AsNoTracking()
            .ToListAsync();
        var statusNames = allStatuses.ToDictionary(s => s.StatusID, s => s.StatusName);

        // Fetch only relevant users for ChangedByUserID lookup
        var relevantUserIds = transitions
            .Where(t => t.ChangedByUserID.HasValue)
            .Select(t => t.ChangedByUserID!.Value)
            .Distinct()
            .ToList();

        var userNames = new Dictionary<int, string>();
        if (relevantUserIds.Count > 0)
        {
            await AddUserNamesAsync(relevantUserIds, userNames);
        }

        var result = transitions.Select(log => new StatusTransitionDto
        {
            TransitionID = log.TransitionID,
            RecordID = log.RecordID,
            OldStatusID = log.OldStatusID,
            OldStatusName = log.OldStatusID.HasValue && statusNames.ContainsKey(log.OldStatusID.Value)
                ? statusNames[log.OldStatusID.Value]
                : null,
            NewStatusID = log.NewStatusID,
            NewStatusName = statusNames.ContainsKey(log.NewStatusID)
                ? statusNames[log.NewStatusID]
                : "غير معروف",
            Reason = log.Reason,
            TransitionDate = log.TransitionDate,
            ChangedByUserID = log.ChangedByUserID,
            ChangedByUserName = log.ChangedByUserID.HasValue && userNames.ContainsKey(log.ChangedByUserID.Value)
                ? userNames[log.ChangedByUserID.Value]
                : null
        }).ToList();

        return ApiResponse<List<StatusTransitionDto>>.Success(result);
    }

    public async Task<ApiResponse<List<FileChangeLogDto>>> GetFileChangeLogAsync(int fileId, int page = 1, int pageSize = 200)
    {
        var queries = await _access.ForPermissionAsync("File.View.Sensitive");
        if (!await queries.Files().AnyAsync(f => f.FileID == fileId))
            throw new PensionMS.Core.Exceptions.EntityNotFoundException("الملف غير موجود");
        var changes = new List<FileChangeLogDto>();

        // ── 1. Status Transitions ──────────────────────────────
        var transitions = await _unitOfWork.StatusTransitionLogs.Query()
            .AsNoTracking()
            .Where(log => log.TableID == 2 && log.RecordID == fileId)
            .OrderByDescending(log => log.TransitionDate)
            .ToListAsync();

        // Lookup tables
        var allStatuses = await _unitOfWork.FileStatuses.Query()
            .AsNoTracking().ToListAsync();
        var statusNames = allStatuses.ToDictionary(s => s.StatusID, s => s.StatusName);

        // Phase 4.1: تحسين — فلترة المستخدمين المطلوبين فقط
        var relevantUserIds = transitions
            .Where(t => t.ChangedByUserID.HasValue)
            .Select(t => t.ChangedByUserID!.Value)
            .Distinct()
            .ToList();

        var userNames = new Dictionary<int, string>();
        if (relevantUserIds.Count > 0)
        {
            // Avoid OPENJSON/IN queries for better compatibility with older SQL Server versions
            await AddUserNamesAsync(relevantUserIds, userNames);
        }

        foreach (var t in transitions)
        {
            var oldName = t.OldStatusID.HasValue && statusNames.ContainsKey(t.OldStatusID.Value)
                ? statusNames[t.OldStatusID.Value] : null;
            var newName = statusNames.ContainsKey(t.NewStatusID)
                ? statusNames[t.NewStatusID] : "غير معروف";

            changes.Add(new FileChangeLogDto
            {
                ChangeType = "StatusChange",
                Description = oldName != null
                    ? $"تغيير حالة الملفات من \"{oldName}\" إلى \"{newName}\""
                    : $"تعيين حالة الملفات إلى \"{newName}\"",
                EntityName = "حالة الملفات",
                OldStatusName = oldName,
                NewStatusName = newName,
                Reason = t.Reason,
                ChangeDate = t.TransitionDate,
                ChangedByUserName = t.ChangedByUserID.HasValue && userNames.ContainsKey(t.ChangedByUserID.Value)
                    ? userNames[t.ChangedByUserID.Value] : null
            });
        }

        // ── 2. Audit Logs (all related tables) ─────────────────
        // TableIDs related to a file
        var relatedTableIds = new[] { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 18, 19 };
        // 2=RetirementFiles, 3=EntitledPersons, 4=Agents, 5=Guardians
        // 6=Documents, 7=DocumentPledges, 8=MissingDocuments
        // 9=DocumentExemptions, 10=FileNotes, 11=PensionSuspensions, 12=SalaryRecipients
        // 18=Settlements, 19=PaymentBooks

        var tableNameMap = new Dictionary<int, string>
        {
            { 2, "الملف" },
            { 3, "المستحقون" },
            { 4, "الوكلاء" },
            { 5, "الأوصياء" },
            { 6, "الوثائق" },
            { 7, "التعهدات" },
            { 8, "الوثائق الناقصة" },
            { 9, "إعفاءات الوثائق" },
            { 10, "ملاحظات الملف" },
            { 11, "إيقاف المعاش" },
            { 12, "مستلمو المعاشات" },
            { 18, "التسويات" },
            { 19, "دفاتر الصرف" }
        };

        var actionMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Create", "إنشاء" },
            { "Update", "تعديل" },
            { "Delete", "حذف" }
        };

        // For the file itself (TableID=2, RecordID=fileId)
        var fileAudits = await _unitOfWork.AuditLogs.Query()
            .AsNoTracking()
            .Where(a => a.TableID.HasValue && a.TableID.Value == 2 && a.RecordID.HasValue && a.RecordID.Value == fileId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        foreach (var a in fileAudits)
        {
            var actionAr = actionMap.GetValueOrDefault(a.Action, a.Action);
            var safeDisplayName = a.EntityDisplayName;
            if (!string.IsNullOrWhiteSpace(safeDisplayName) && safeDisplayName.Contains("(") && safeDisplayName.EndsWith(")"))
                safeDisplayName = null;

            var displayName = string.IsNullOrWhiteSpace(safeDisplayName)
                ? "الملف"
                : safeDisplayName;

            var description = $"{actionAr} {displayName}";

            changes.Add(new FileChangeLogDto
            {
                ChangeType = a.Action,
                Description = description,
                EntityName = "الملف",
                EntityKey = a.EntityKey,
                EntityDisplayName = a.EntityDisplayName,
                FieldChanges = BuildFieldChanges(a.OldValues, a.NewValues),
                CorrelationId = a.CorrelationId,
                ChangeDate = a.CreatedAt,
                ChangedByUserName = a.UserID.HasValue && userNames.ContainsKey(a.UserID.Value)
                    ? userNames[a.UserID.Value] : null,
                IPAddress = a.IPAddress,
                UserAgent = a.UserAgent
            });
        }

        // For related entities - get IDs of all entities belonging to this file
        var entitledIds = await _unitOfWork.EntitledPersons.Query().AsNoTracking()
            .Where(e => e.FileID == fileId).Select(e => e.EntitledID).ToListAsync();
        var agentIds = await _unitOfWork.Agents.Query().AsNoTracking()
            .Where(a => a.FileID == fileId).Select(a => a.AgentID).ToListAsync();
        var guardianIds = await _unitOfWork.Guardians.Query().AsNoTracking()
            .Where(g => g.FileID == fileId).Select(g => g.GuardianID).ToListAsync();
        var documentIds = await queries.Query(_unitOfWork.Documents).AsNoTracking()
            .Where(d => d.FileID == fileId).Select(d => d.DocumentID).ToListAsync();
        var pledgeIds = await _unitOfWork.DocumentPledges.Query().AsNoTracking()
            .Where(p => p.FileID == fileId).Select(p => p.PledgeID).ToListAsync();
        var missingDocIds = await _unitOfWork.MissingDocuments.Query().AsNoTracking()
            .Where(m => m.FileID == fileId).Select(m => m.MissingDocumentID).ToListAsync();
        var exemptionIds = await _unitOfWork.DocumentExemptions.Query().AsNoTracking()
            .Where(x => x.FileID == fileId).Select(x => x.ExemptionID).ToListAsync();
        var noteIds = await _unitOfWork.FileNotes.Query().AsNoTracking()
            .Where(n => n.FileID == fileId).Select(n => n.NoteID).ToListAsync();
        var suspensionIds = await _unitOfWork.PensionSuspensions.Query().AsNoTracking()
            .Where(s => s.FileID == fileId).Select(s => s.SuspensionID).ToListAsync();
        var recipientIds = await _unitOfWork.SalaryRecipients.Query().AsNoTracking()
            .Where(r => r.FileID == fileId).Select(r => r.RecipientID).ToListAsync();
        var paymentBookIds = await _unitOfWork.PaymentBooks.Query().AsNoTracking()
            .Where(pb => pb.FileID == fileId).Select(pb => pb.PaymentBookID).ToListAsync();

        // Settlements - fetch by RetireeID linked to this file
        var retireeId = await queries.Files().AsNoTracking()
            .Where(f => f.FileID == fileId).Select(f => f.RetireeID).FirstOrDefaultAsync();
        var settlementItemIds = retireeId > 0
            ? await _unitOfWork.SettlementItems.Query().AsNoTracking()
                .Where(s => s.RetireeID == retireeId).Select(s => s.SettlementItemID).ToListAsync()
            : new List<int>();

        // Build a map: TableID → list of record IDs belonging to this file
        var tableRecordIds = new Dictionary<int, List<int>>
        {
            { 3, entitledIds },
            { 4, agentIds },
            { 5, guardianIds },
            { 6, documentIds },
            { 7, pledgeIds },
            { 8, missingDocIds },
            { 9, exemptionIds },
            { 10, noteIds },
            { 11, suspensionIds },
            { 12, recipientIds },
            { 18, settlementItemIds },
            { 19, paymentBookIds }
        };

        // Phase 4.1: استعلام واحد بدل N+1 — جمع كل الـ recordIds في predicate واحد
        // بناء قائمة موحدة لكل الأزواج (tableId, recordIds)
        var allRecordIds = tableRecordIds
            .Where(kvp => kvp.Value.Count > 0)
            .SelectMany(kvp => kvp.Value)
            .Distinct()
            .ToList();

        var allTableIds = tableRecordIds
            .Where(kvp => kvp.Value.Count > 0)
            .Select(kvp => kvp.Key)
            .ToList();

        // استعلام واحد لكل الـ audit logs المطلوبة
        // Query audit logs per table to avoid EF Core SQL generation issues
        var relatedAudits = new List<AuditLog>();
        if (allRecordIds.Count > 0 && allTableIds.Count > 0)
        {
            // Query audit logs for each table separately
            foreach (var tableId in allTableIds)
            {
                var recordIdsForTable = tableRecordIds[tableId];
                if (recordIdsForTable.Count == 0) continue;

                var auditsForTable = await _unitOfWork.AuditLogs.Query()
                    .AsNoTracking()
                    .Where(a => a.TableID == tableId && a.RecordID.HasValue && recordIdsForTable.Contains(a.RecordID.Value))
                    .ToListAsync();
                
                relatedAudits.AddRange(auditsForTable);
            }
            
            // Remove duplicates and sort
            relatedAudits = relatedAudits
                .OrderByDescending(a => a.CreatedAt)
                .ToList();
        }

        // تحديث userNames بالمستخدمين الإضافيين من audit logs
        var auditUserIds = relatedAudits
            .Where(a => a.UserID.HasValue && !userNames.ContainsKey(a.UserID.Value))
            .Select(a => a.UserID!.Value)
            .Distinct()
            .ToList();
        if (auditUserIds.Count > 0)
        {
            // Reuse the same safe loader for any additional users referenced in audit logs
            await AddUserNamesAsync(auditUserIds, userNames);
        }

        // تصفية وإضافة لكل جدول
        foreach (var (tableId, recordIds) in tableRecordIds)
        {
            if (recordIds.Count == 0) continue;

            var filtered = relatedAudits.Where(a => 
                a.TableID.HasValue && a.TableID.Value == tableId && a.RecordID.HasValue && recordIds.Contains(a.RecordID.Value));

            var tableName = tableNameMap.GetValueOrDefault(tableId, $"جدول {tableId}");
            foreach (var a in filtered)
            {
                var actionAr = actionMap.GetValueOrDefault(a.Action, a.Action);
                var safeDisplayName = a.EntityDisplayName;
                if (!string.IsNullOrWhiteSpace(safeDisplayName) && safeDisplayName.Contains("(") && safeDisplayName.EndsWith(")"))
                    safeDisplayName = null;

                var displayName = string.IsNullOrWhiteSpace(safeDisplayName)
                    ? tableName
                    : safeDisplayName;

                var description = $"{actionAr} {displayName}";

                changes.Add(new FileChangeLogDto
                {
                    ChangeType = a.Action,
                    Description = description,
                    EntityName = tableName,
                    EntityKey = a.EntityKey,
                    EntityDisplayName = a.EntityDisplayName,
                    FieldChanges = BuildFieldChanges(a.OldValues, a.NewValues),
                    CorrelationId = a.CorrelationId,
                    ChangeDate = a.CreatedAt,
                    ChangedByUserName = a.UserID.HasValue && userNames.ContainsKey(a.UserID.Value)
                        ? userNames[a.UserID.Value] : null,
                    IPAddress = a.IPAddress,
                    UserAgent = a.UserAgent
                });
            }
        }

        // Sort all changes by date descending
        var sorted = changes.OrderByDescending(c => c.ChangeDate).ToList();

        // Simple in-memory pagination
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 200;

        var paged = sorted
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return ApiResponse<List<FileChangeLogDto>>.Success(paged);
    }

    /// <summary>
    /// بناء قائمة بالتغييرات على مستوى الحقول انطلاقاً من OldValues/NewValues (بصيغة JSON).
    /// التنسيق المتوقع: قاموس { اسم_الحقل : القيمة } في كل من القديم والجديد.
    /// </summary>
    private static List<FieldChangeDto>? BuildFieldChanges(string? oldValuesJson, string? newValuesJson)
    {
        if (string.IsNullOrWhiteSpace(oldValuesJson) && string.IsNullOrWhiteSpace(newValuesJson))
            return null;

        try
        {
            var oldDict = string.IsNullOrWhiteSpace(oldValuesJson)
                ? new Dictionary<string, JsonElement>()
                : JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(oldValuesJson) ?? new Dictionary<string, JsonElement>();

            var newDict = string.IsNullOrWhiteSpace(newValuesJson)
                ? new Dictionary<string, JsonElement>()
                : JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(newValuesJson) ?? new Dictionary<string, JsonElement>();

            var allKeys = oldDict.Keys.Union(newDict.Keys).Distinct();
            var result = new List<FieldChangeDto>();

            foreach (var key in allKeys)
            {
                string? oldVal = oldDict.TryGetValue(key, out var oldEl) && oldEl.ValueKind != JsonValueKind.Null ? oldEl.ToString() : null;
                string? newVal = newDict.TryGetValue(key, out var newEl) && newEl.ValueKind != JsonValueKind.Null ? newEl.ToString() : null;

                // إذا لا يوجد فرق، نتجاهل الحقل
                if (string.Equals(oldVal, newVal, StringComparison.Ordinal))
                    continue;

                result.Add(new FieldChangeDto
                {
                    FieldName = key,
                    OldValue = oldVal,
                    NewValue = newVal
                });
            }

            return result.Count == 0 ? null : result;
        }
        catch
        {
            // في حال فشل التحليل لأي سبب، لا نفشل الطلب — فقط لا نعيد تفاصيل الحقول
            return null;
        }
    }

    /// <summary>
    /// Safely loads user names for the given IDs without generating OPENJSON/IN SQL,
    /// to keep compatibility with older SQL Server versions.
    /// </summary>
    private async Task AddUserNamesAsync(IEnumerable<int> userIds, Dictionary<int, string> userNames)
    {
        var distinctIds = userIds.Distinct().ToList();
        foreach (var id in distinctIds)
        {
            if (userNames.ContainsKey(id))
                continue;

            var user = await _unitOfWork.Users.Query()
                .AsNoTracking()
                .Where(u => u.UserID == id)
                .Select(u => new { u.UserID, u.FullName })
                .FirstOrDefaultAsync();

            if (user != null)
            {
                userNames[user.UserID] = user.FullName;
            }
        }
    }

    // ── Role-based endpoints ──

    public async Task<ApiResponse<PagedResult<RetirementFileListDto>>> GetForDataEntryAsync(int page, int pageSize, string? search = null)
    {
        var queries = await _access.ForPermissionAsync("File.Edit.BasicData");
        // Get files with any status except "Active" (نشط)
        var query = queries.Files()
            .AsNoTracking()
            .Where(f => !f.IsDeleted && f.StatusID != FileStatusConstants.Active);

        // ── فلترة حسب نطاق فرع المستخدم ──
// Apply search filter if provided
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(f => f.FileNumber.Contains(search) ||
                                     (f.Retiree != null && f.Retiree.FullName.Contains(search)) ||
                                     (f.Retiree != null && f.Retiree.NationalID.Contains(search)) ||
                                     (f.Retiree != null && f.Retiree.MilitaryNumber.Contains(search)));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(f => f.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(f => new RetirementFileListDto
            {
                FileID = f.FileID,
                FileNumber = f.FileNumber,
                RetireeFullName = f.Retiree != null ? f.Retiree.FullName : "",
                NationalID = f.Retiree != null ? f.Retiree.NationalID : null,
                MilitaryNumber = f.Retiree != null ? f.Retiree.MilitaryNumber : null,
                RankName = f.Rank != null ? f.Rank.RankName : null,
                FileStatusName = f.Status != null ? f.Status.StatusName : null,
                FileOpenDate = f.FileOpenDate,
                CreatedAt = f.CreatedAt,
                ActivePaymentBookNumber = f.PaymentBooks
                    .Where(pb => pb.Status == PaymentBookStatus.Active && !pb.IsDeleted)
                    .OrderByDescending(pb => pb.CreatedAt)
                    .Select(pb => pb.BookNumber)
                    .FirstOrDefault()
            })
            .ToListAsync();

        return ApiResponse<PagedResult<RetirementFileListDto>>.Success(new PagedResult<RetirementFileListDto>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        });
    }

    public async Task<ApiResponse<List<RetirementFileDocumentUploadDto>>> GetForDocumentUploadAsync()
    {
        var queries = await _access.ForPermissionAsync("File.Documents.Upload");
        // ── 1. جلب أنواع الوثائق المطلوبة ──
        var requiredDocTypeIds = await _unitOfWork.DocumentTypes.Query()
            .AsNoTracking()
            .Where(dt => dt.IsActive && dt.IsRequired)
            .Select(dt => dt.DocumentTypeID)
            .ToListAsync();

        var requiredDocTypes = await _unitOfWork.DocumentTypes.Query()
            .AsNoTracking()
            .Where(dt => dt.IsActive && dt.IsRequired)
            .ToListAsync();

        // ── 2. استعلام مباشر للخادم يحدد الملفات التي لديها نواقص ──
        // ملف بنواقص = (يوجد وثيقة مسجلة كـ Pending) أو (عدد الوثائق المرفوعة المطلوبة < إجمالي المطلوبة)
        var query = queries.Files()
            .AsNoTracking()
            .Where(f => !f.IsDeleted && f.StatusID != FileStatusConstants.EntitlementEnded);

        // ── فلترة حسب نطاق فرع المستخدم ──
        var filesQuery = query
            .Where(f => f.MissingDocuments.Any(m => m.Status == MissingDocumentStatusConstants.Pending) ||
                        f.Documents.Count(d => !d.IsDeleted && requiredDocTypeIds.Contains(d.DocumentTypeID)) < requiredDocTypeIds.Count);

        var files = await filesQuery
            .Select(f => new
            {
                f.FileID,
                f.FileNumber,
                RetireeFullName = f.Retiree.FullName,
                f.Retiree.NationalID,
                f.StatusID,
                FileStatusName = f.Status.StatusName,
                UploadedDocs = f.Documents
                    .Where(d => !d.IsDeleted)
                    .Select(d => new
                    {
                        d.DocumentID,
                        d.DocumentTypeID,
                        DocumentTypeName = d.DocumentType != null ? d.DocumentType.TypeName : "غير محدد",
                        d.FileName,
                        UploadDate = d.CreatedAt,
                        Status = d.Verified ? "approved" : "pending"
                    }).ToList(),
                ManualMissing = f.MissingDocuments
                    .Where(m => m.Status == MissingDocumentStatusConstants.Pending)
                    .Select(m => new
                    {
                        m.DocumentTypeID,
                        DocumentTypeName = m.DocumentType.TypeName,
                        IsRequired = m.DocumentType.IsRequired,
                        IsAutoDetected = false
                    }).ToList()
            })
            .ToListAsync();

        var result = new List<RetirementFileDocumentUploadDto>();

        foreach (var file in files)
        {
            var uploadedTypeIds = file.UploadedDocs.Select(d => d.DocumentTypeID).ToHashSet();
            var manualTypeIds = file.ManualMissing.Select(m => m.DocumentTypeID).ToHashSet();

            // Auto-detect missing (Server-side couldn't easily build the full list with labels)
            var autoDetected = requiredDocTypes
                .Where(dt => !uploadedTypeIds.Contains(dt.DocumentTypeID) && !manualTypeIds.Contains(dt.DocumentTypeID))
                .Select(dt => new
                {
                    documentTypeID = dt.DocumentTypeID,
                    documentTypeName = dt.TypeName,
                    isRequired = true,
                    isAutoDetected = true
                }).ToList();

            var allMissingCount = file.ManualMissing.Count + autoDetected.Count;

            result.Add(new RetirementFileDocumentUploadDto
            {
                FileID = file.FileID,
                FileNumber = file.FileNumber,
                RetireeFullName = file.RetireeFullName ?? "",
                NationalID = file.NationalID,
                StatusID = file.StatusID,
                FileStatusName = file.FileStatusName,
                DocumentsCount = file.UploadedDocs.Count,
                MissingDocumentsCount = allMissingCount,
                IsIncomplete = allMissingCount > 0
            });
        }

        return ApiResponse<List<RetirementFileDocumentUploadDto>>.Success(result);
    }

    public async Task<ApiResponse<DocumentUploadStatsDto>> GetDocumentUploadStatsAsync()
    {
        var queries = await _access.ForPermissionAsync("File.Documents.Upload");
        var requiredDocTypeIds = await _unitOfWork.DocumentTypes.Query()
            .AsNoTracking()
            .Where(dt => dt.IsActive && dt.IsRequired)
            .Select(dt => dt.DocumentTypeID)
            .ToListAsync();

        var query = queries.Files()
            .AsNoTracking()
            .Where(f => !f.IsDeleted && f.StatusID != FileStatusConstants.EntitlementEnded);

        var files = await query
            .Select(f => new
            {
                f.FileID,
                UploadedDocTypeIds = f.Documents
                    .Where(d => !d.IsDeleted)
                    .Select(d => d.DocumentTypeID)
                    .ToList(),
                ManualMissingTypeIds = f.MissingDocuments
                    .Where(m => m.Status == MissingDocumentStatusConstants.Pending)
                    .Select(m => m.DocumentTypeID)
                    .ToList()
            })
            .ToListAsync();

        var totalFiles = files.Count;
        var filesWithMissing = 0;
        var filesWithoutDocuments = 0;
        var totalMissingDocuments = 0;
        var filesWithDocuments = 0;

        foreach (var file in files)
        {
            var uploadedSet = file.UploadedDocTypeIds.ToHashSet();
            var manualSet = file.ManualMissingTypeIds.ToHashSet();
            var autoDetectedCount = requiredDocTypeIds.Count(dt => !uploadedSet.Contains(dt) && !manualSet.Contains(dt));
            var missingCount = file.ManualMissingTypeIds.Count + autoDetectedCount;
            var hasDocs = file.UploadedDocTypeIds.Count > 0;

            if (hasDocs) filesWithDocuments++;
            else filesWithoutDocuments++;
            if (missingCount > 0) filesWithMissing++;
            totalMissingDocuments += missingCount;
        }

        return ApiResponse<DocumentUploadStatsDto>.Success(new DocumentUploadStatsDto
        {
            TotalFiles = totalFiles,
            FilesWithDocuments = filesWithDocuments,
            FilesWithoutDocuments = filesWithoutDocuments,
            FilesWithMissing = filesWithMissing,
            TotalMissingDocuments = totalMissingDocuments
        });
    }

    public async Task<ApiResponse<List<UploaderStatsDto>>> GetUploaderStatsAsync(UploaderStatsFilterDto filter)
    {
        var queries = await _access.ForPermissionAsync("File.Documents.Upload");
        var query = queries.Query(_unitOfWork.Documents)
            .AsNoTracking()
            .Where(d => !d.IsDeleted && d.FileID.HasValue);

// Apply date filters
        if (filter.Period is "today" or "yesterday" or "week" or "month" or "year")
        {
            var (start, end) = PensionMS.Core.Time.YemenTime.PeriodUtc(filter.Period);
            query = query.Where(d => d.CreatedAt >= start && d.CreatedAt < end);
        }
        else if (filter.FromDate.HasValue || filter.ToDate.HasValue)
        {
            if (filter.FromDate.HasValue)
            {
                var start = PensionMS.Core.Time.YemenTime.StartUtc(DateOnly.FromDateTime(filter.FromDate.Value));
                query = query.Where(d => d.CreatedAt >= start);
            }
            if (filter.ToDate.HasValue)
            {
                var end = PensionMS.Core.Time.YemenTime.StartUtc(DateOnly.FromDateTime(filter.ToDate.Value).AddDays(1));
                query = query.Where(d => d.CreatedAt < end);
            }
        }

        // Step 1: Count documents per (File, User)
        var fileUserCounts = await query
            .GroupBy(d => new { d.FileID, d.UploadedByUserID })
            .Select(g => new
            {
                FileID = g.Key.FileID!.Value,
                UserID = g.Key.UploadedByUserID,
                DocCount = g.Count(),
                LastUpload = g.Max(d => d.CreatedAt)
            })
            .ToListAsync();

        // Step 2: For each file, pick the user with the most documents (tie-break: highest UserID)
        var fileWinners = fileUserCounts
            .GroupBy(x => x.FileID)
            .Select(g => g.OrderByDescending(x => x.DocCount).ThenByDescending(x => x.UserID).First())
            .ToList();

        // Step 3: Build stats per winning user
        var stats = fileWinners
            .GroupBy(w => w.UserID)
            .Select(g => new UploaderStatsDto
            {
                UserID = g.Key,
                TotalUploads = g.Sum(x => x.DocCount),
                UniqueFiles = g.Count(),
                LastUploadDate = g.Max(x => x.LastUpload)
            })
            .OrderByDescending(s => s.TotalUploads)
            .ToList();

        // Get user names
        var userIds = stats.Select(s => s.UserID).ToList();
        var users = await _unitOfWork.Users.Query()
            .AsNoTracking()
            .Where(u => userIds.Contains(u.UserID))
            .Select(u => new { u.UserID, u.FullName, u.Username })
            .ToListAsync();

        foreach (var stat in stats)
        {
            var user = users.FirstOrDefault(u => u.UserID == stat.UserID);
            if (user != null)
            {
                stat.UserName = user.Username;
                stat.FullName = user.FullName;
            }
        }

        return ApiResponse<List<UploaderStatsDto>>.Success(stats);
    }

    public async Task<ApiResponse<List<DataEntryStatsDto>>> GetDataEntryStatsAsync(UploaderStatsFilterDto filter)
    {
        var queries = await _access.ForPermissionAsync("File.Documents.Upload");
        // مصدر البيانات: جدول ملفات التقاعد — مَن أنشأ الملف
        var query = queries.Files()
            .AsNoTracking()
            .Where(f => !f.IsDeleted);

// فلترة حسب الفترة
        if (filter.Period is "today" or "yesterday" or "week" or "month" or "year")
        {
            var (start, end) = PensionMS.Core.Time.YemenTime.PeriodUtc(filter.Period);
            query = query.Where(d => d.CreatedAt >= start && d.CreatedAt < end);
        }
        else if (filter.FromDate.HasValue || filter.ToDate.HasValue)
        {
            if (filter.FromDate.HasValue)
            {
                var start = PensionMS.Core.Time.YemenTime.StartUtc(DateOnly.FromDateTime(filter.FromDate.Value));
                query = query.Where(f => f.CreatedAt >= start);
            }
            if (filter.ToDate.HasValue)
            {
                var end = PensionMS.Core.Time.YemenTime.StartUtc(DateOnly.FromDateTime(filter.ToDate.Value).AddDays(1));
                query = query.Where(f => f.CreatedAt < end);
            }
        }

        // جلب بيانات الملفات مجمّعة حسب منشئ الملف
        var files = await query
            .Select(f => new { f.FileID, f.StatusID, f.ReviewStatus, f.CreatedAt, CreatedByUserID = (int?)f.CreatedByUserID })
            .ToListAsync();

        // تجميع حسب مَن أنشأ الملف
        var stats = files
            .GroupBy(f => f.CreatedByUserID ?? 0)
            .Where(g => g.Key > 0) // استبعاد الملفات بدون منشئ
            .Select(g => new DataEntryStatsDto
            {
                UserID = g.Key,
                TotalFilesCreated = g.Count(),
                CompletedFiles = g.Count(f =>
                    f.StatusID == FileStatusConstants.Active ||
                    f.StatusID == FileStatusConstants.EntitlementEnded),
                PendingFiles = g.Count(f =>
                    f.StatusID == FileStatusConstants.New ||
                    f.StatusID == FileStatusConstants.Pending),
                RejectedFiles = g.Count(f =>
                    f.ReviewStatus == FileReviewStatus.Rejected),
                LastCreatedDate = g.Max(f => f.CreatedAt),
            })
            .OrderByDescending(s => s.TotalFilesCreated)
            .ToList();

        // جلب أسماء المستخدمين
        var userIds = stats.Select(s => s.UserID).ToList();
        var users = await _unitOfWork.Users.Query()
            .AsNoTracking()
            .Where(u => userIds.Contains(u.UserID))
            .Select(u => new { u.UserID, u.FullName, u.Username })
            .ToListAsync();

        foreach (var stat in stats)
        {
            var user = users.FirstOrDefault(u => u.UserID == stat.UserID);
            if (user != null)
            {
                stat.UserName = user.Username;
                stat.FullName = user.FullName;
            }
        }

        return ApiResponse<List<DataEntryStatsDto>>.Success(stats);
    }

    public async Task<ApiResponse<PagedResult<RetirementFileListDto>>> GetDataEntryFilesByUserAsync(int userId, int page, int pageSize, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var queries = await _access.ForPermissionAsync("File.Documents.Upload");
        // جلب الملفات التي أنشأها المستخدم
        var filesQuery = queries.Files()
            .AsNoTracking()
            .Where(f => !f.IsDeleted && f.CreatedByUserID == userId);

        if (fromDate.HasValue)
        {
            var start = PensionMS.Core.Time.YemenTime.StartUtc(DateOnly.FromDateTime(fromDate.Value));
            filesQuery = filesQuery.Where(f => f.CreatedAt >= start);
        }
        if (toDate.HasValue)
        {
            var end = PensionMS.Core.Time.YemenTime.StartUtc(DateOnly.FromDateTime(toDate.Value).AddDays(1));
            filesQuery = filesQuery.Where(f => f.CreatedAt < end);
        }

        var total = await filesQuery.CountAsync();
        var items = await filesQuery
            .OrderByDescending(f => f.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(f => new RetirementFileListDto
            {
                FileID = f.FileID,
                FileNumber = f.FileNumber,
                RetireeFullName = f.Retiree != null ? f.Retiree.FullName : "غير محدد",
                NationalID = f.Retiree != null ? f.Retiree.NationalID : "غير محدد",
                MilitaryNumber = f.Retiree != null ? f.Retiree.MilitaryNumber : null,
                RankName = f.Rank != null ? f.Rank.RankName : null,
                StatusID = f.StatusID,
                FileStatusName = f.Status != null ? f.Status.StatusName : null,
                FileOpenDate = f.FileOpenDate,
                CreatedAt = f.CreatedAt,
                MissingDocumentsCount = f.MissingDocuments
                    .Count(m => m.Status == PensionMS.Core.Constants.MissingDocumentStatusConstants.Pending ||
                                m.Status == PensionMS.Core.Constants.MissingDocumentStatusConstants.Pledged),
                HasDocuments = f.Documents.Any(),
                LastUploadDate = f.Documents
                    .OrderByDescending(d => d.CreatedAt)
                    .Select(d => d.CreatedAt)
                    .FirstOrDefault()
            })
            .ToListAsync();

        return ApiResponse<PagedResult<RetirementFileListDto>>.Success(new PagedResult<RetirementFileListDto>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
        });
    }

}
