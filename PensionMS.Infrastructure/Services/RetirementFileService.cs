using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Documents;
using PensionMS.Core.DTOs.EntitledPersons;
using PensionMS.Core.DTOs.Notifications;
using PensionMS.Core.DTOs.Discrepancy;
using PensionMS.Core.DTOs.Reports;
using PensionMS.Core.DTOs.RetirementFiles;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Helpers;
using PensionMS.Infrastructure.Helpers;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PensionMS.Infrastructure.Services;

public class RetirementFileService : IRetirementFileService, IFileQueryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWebHostEnvironment _env;
    private readonly ISoDGuard _sodGuard;
    private readonly IWorkflowEngine _workflowEngine;
    private readonly IRiskScoringService _riskScoringService;
    private readonly IImmutableAuditService _auditService;
    private readonly ISnapshotService _snapshotService;
    private readonly IComplianceEngine _complianceEngine;
    private readonly IMissingDocumentService _missingDocumentService;
    private readonly IDocumentService _documentService;
    private readonly IFileStorageService _fileStorage;
    private readonly IBranchScopeProvider _branchScopeProvider;
    private readonly IEntitledPersonService _entitledPersonService;
    private readonly INotificationService _notificationService;
    private readonly IFileReviewService _reviewService;
    private readonly ILogger<RetirementFileService> _logger;

    public RetirementFileService(
        IUnitOfWork unitOfWork, 
        IWebHostEnvironment env, 
        ISoDGuard sodGuard,
        IWorkflowEngine workflowEngine,
        IRiskScoringService riskScoringService,
        IImmutableAuditService auditService,
        ISnapshotService snapshotService,
        IComplianceEngine complianceEngine,
        IMissingDocumentService missingDocumentService,
        IDocumentService documentService,
        IFileStorageService fileStorage,
        IBranchScopeProvider branchScopeProvider,
        IEntitledPersonService entitledPersonService,
        INotificationService notificationService,
        IFileReviewService reviewService,
        ILogger<RetirementFileService> logger)
    {
        _unitOfWork = unitOfWork;
        _env = env;
        _sodGuard = sodGuard;
        _workflowEngine = workflowEngine;
        _riskScoringService = riskScoringService;
        _auditService = auditService;
        _snapshotService = snapshotService;
        _complianceEngine = complianceEngine;
        _missingDocumentService = missingDocumentService;
        _documentService = documentService;
        _fileStorage = fileStorage;
        _branchScopeProvider = branchScopeProvider;
        _entitledPersonService = entitledPersonService;
        _notificationService = notificationService;
        _reviewService = reviewService;
        _logger = logger;
    }

    public async Task<ApiResponse<RetirementFileDto>> GetByIdAsync(int id)
    {
        var file = await _unitOfWork.RetirementFiles.Query()
            .AsNoTracking()
            .Include(f => f.Retiree)
            .Include(f => f.Rank)
            .Include(f => f.PensionBranch)
            .Include(f => f.Status)
            .Include(f => f.AssignedEmployee)
            .Include(f => f.Documents.Where(d => d.DocumentTypeID == 1 && !d.IsDeleted))
            .FirstOrDefaultAsync(f => f.FileID == id);

        if (file == null)
            return ApiResponse<RetirementFileDto>.Fail("الملف غير موجود");

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

        var compliance = await _complianceEngine.GetComplianceSummaryAsync(id);
        var missingDocsList = await GetMissingDocsListAsync(id);
        var missingData = await CalculateMissingDataAsync(id);

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

        return ApiResponse<RetirementFileDto>.Success(MapToDto(file, entitledCount, agentsCount, guardiansCount, missingDocsList.Count, missingDocsList, missingData.Count, missingData.Items, activeBook, hasMinors, hasPledgesOrExemptions, hasSettlements));
    }

    private async Task<List<string>> GetMissingDocsListAsync(int fileId)
    {
        var missingDocsList = new List<string>();

        var manualMissingDocsRes = await _missingDocumentService.GetByFileAsync(fileId);
        if (manualMissingDocsRes.IsSuccess && manualMissingDocsRes.Data != null)
        {
            var manual = manualMissingDocsRes.Data
                .Where(m => PensionMS.Core.Constants.MissingDocumentStatusConstants.OutstandingStatuses.Contains(m.Status))
                .Select(m => {
                    if (!string.IsNullOrEmpty(m.AgentName)) return $"الوكيل ({m.AgentName}) — {m.DocumentTypeName}";
                    if (!string.IsNullOrEmpty(m.GuardianName)) return $"الوصي ({m.GuardianName}) — {m.DocumentTypeName}";
                    if (!string.IsNullOrEmpty(m.EntitledName)) return $"المستحق ({m.EntitledName}) — {m.DocumentTypeName}";
                    return $"المتقاعد — {m.DocumentTypeName}";
                });
            missingDocsList.AddRange(manual);
        }

        var autoDetectedRes = await _missingDocumentService.GetAutoDetectedByFileAsync(fileId);
        if (autoDetectedRes.IsSuccess && autoDetectedRes.Data != null)
        {
            foreach (var ad in autoDetectedRes.Data)
            {
                var entityTypeAr = ad.EntityType == EntityTypeConstants.EntitledPerson ? "المستحق" :
                                   ad.EntityType == EntityTypeConstants.Guardian ? "الوصي" :
                                   ad.EntityType == EntityTypeConstants.Agent ? "الوكيل" : "المتقاعد";
                var label = !string.IsNullOrEmpty(ad.EntityName) 
                    ? $"{entityTypeAr} ({ad.EntityName}) — {ad.DocumentTypeName}"
                    : $"المتقاعد — {ad.DocumentTypeName}";
                missingDocsList.Add(label);
            }
        }

        return missingDocsList;
    }

    public async Task<ApiResponse<PagedResult<RetirementFileListDto>>> GetAllAsync(int page, int pageSize, string? search = null, int? statusId = null, int? userId = null, string? statusIds = null, int? uploadedBy = null, int? cursor = null, string? period = null, DateTime? fromDate = null, DateTime? toDate = null, int? categoryId = null, int? rankId = null, string? governorate = null, byte? gender = null, string? unitName = null, bool? hasSettlements = null)
    {
        var query = _unitOfWork.RetirementFiles.Query()
            .AsNoTracking()
            .AsQueryable();

        // ── فلترة حسب نطاق فرع المستخدم ──
        var branchId = _branchScopeProvider.GetCurrentBranchId();
        if (branchId.HasValue && !_branchScopeProvider.IsHQ())
        {
            query = query.Where(f => f.PensionBranchID == branchId.Value);
        }

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
            var from = fromDate ?? DateTime.MinValue;
            var to = toDate?.AddDays(1).AddTicks(-1) ?? DateTime.MaxValue;
            query = query.Where(f => f.Documents.Any(d => d.CreatedAt >= from && d.CreatedAt <= to));
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
        var file = await _unitOfWork.RetirementFiles.Query()
            .AsNoTracking()
            .Include(f => f.Retiree)
            .Include(f => f.Rank)
            .Include(f => f.PensionBranch)
            .Include(f => f.Status)
            .FirstOrDefaultAsync(f => f.FileNumber == fileNumber);

        if (file == null)
            return ApiResponse<RetirementFileDto>.Fail("الملف غير موجود");

        var activeBook = await _unitOfWork.PaymentBooks.Query()
            .AsNoTracking()
            .Where(pb => pb.FileID == file.FileID && pb.Status == PaymentBookStatus.Active)
            .OrderByDescending(pb => pb.CreatedAt)
            .FirstOrDefaultAsync();

        // حساب البيانات الناقصة
        var missingData = await CalculateMissingDataAsync(file.FileID);
        var missingDocsList = await GetMissingDocsListAsync(file.FileID);

        return ApiResponse<RetirementFileDto>.Success(MapToDto(file, missingDocsCount: missingDocsList.Count, missingDocsList: missingDocsList, missingDataCount: missingData.Count, missingDataList: missingData.Items, activeBook: activeBook));
    }

    public async Task<ApiResponse<List<RetirementFileListDto>>> GetByRetireeIdAsync(int retireeId)
    {
        var files = await _unitOfWork.RetirementFiles.Query()
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

    public async Task<ApiResponse<RetirementFileDto>> CreateAsync(CreateRetirementFileDto dto, int userId)
    {
        // ── Phase 1.2: منع إنشاء ملف مكرر لنفس المتقاعد ──
        var existingActive = await _unitOfWork.RetirementFiles.Query()
            .AnyAsync(f => f.RetireeID == dto.RetireeID
                        && f.StatusID != FileStatusConstants.Closed);
        if (existingActive)
            return ApiResponse<RetirementFileDto>.Fail("يوجد ملف نشط لهذا المتقاعد بالفعل");

        // ── Phase 2.4: فحص تواريخ منطقية ──
        if (dto.ServiceStartDate.HasValue && dto.ServiceEndDate.HasValue
            && dto.ServiceEndDate <= dto.ServiceStartDate)
            return ApiResponse<RetirementFileDto>.Fail("تاريخ نهاية الخدمة يجب أن يكون بعد تاريخ البداية");

        // جلب المستخدم لتحديد الفرع (بدون الاعتماد على Claims في JWT)
        var user = await _unitOfWork.Users.Query()
            .FirstOrDefaultAsync(u => u.UserID == userId && u.IsActive);

        if (user == null)
            return ApiResponse<RetirementFileDto>.Fail("المستخدم غير موجود أو غير فعّال");

        int branchId;
        if (user.BranchID.HasValue)
        {
            branchId = user.BranchID.Value;
        }
        else
        {
            // fallback: أول فرع فعّال في النظام (يمكن تخصيصه لاحقاً حسب متطلبات العمل)
            branchId = await _unitOfWork.PensionFundBranches.Query()
                .Where(b => b.IsActive)
                .Select(b => b.PensionBranchID)
                .FirstOrDefaultAsync();

            if (branchId == 0)
                return ApiResponse<RetirementFileDto>.Fail("لم يتم إعداد فروع الصندوق بعد");
        }

        // Fetch Retiree to determine Rank
        var retiree = await _unitOfWork.Retirees.Query()
            .IgnoreQueryFilters() // Bypass MinistryApprovalItem IsDeleted filter cascade
            .Where(r => !r.IsDeleted) // Ensure the Retiree itself is not soft-deleted
            .Include(r => r.ApprovalItem)
                .ThenInclude(ai => ai!.SettlementItems)
                    .ThenInclude(si => si.ProcessedRank)
            .FirstOrDefaultAsync(r => r.RetireeID == dto.RetireeID);

        if (retiree == null)
            return ApiResponse<RetirementFileDto>.Fail("المتقاعد غير موجود");

        // ── بدء المعاملة لضمان ذرَية العملية ──
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            // Update Retiree Data (First time creation)
            retiree.NationalID = dto.NationalID ?? string.Empty;
            retiree.Phone = dto.Phone;
            retiree.Address = dto.Address;
            retiree.City = dto.City;
            retiree.Governorate = dto.Governorate;
            retiree.Gender = dto.Gender;
            retiree.SocialStatusID = dto.SocialStatusID;
            retiree.HealthStatusID = dto.HealthStatusID;
            retiree.BirthDate = dto.BirthDate;
            
            // Life Status
            retiree.IsAlive = dto.IsAlive;
            if (!dto.IsAlive)
            {
                retiree.DeathDate = dto.DeathDate;
            }
            else
            {
                retiree.DeathDate = null;
            }
            
            _unitOfWork.Retirees.Update(retiree);

            bool photoUploadFailed = false;
            // Upload Personal Photo
            if (dto.PersonalPhoto != null)
            {
                try
                {
                    var photoPath = await SaveFileAsync(dto.PersonalPhoto, "photos");
                    retiree.PersonalPhotoPath = photoPath;
                }
                catch (Exception ex)
                {
                    photoUploadFailed = true;
                    _logger.LogWarning(ex, "فشل رفع الصورة الشخصية للمتقاعد {RetireeID} عند فتح الملف", dto.RetireeID);
                }
            }

            // ── Determine Rank & Initial Salary ───────────────────────────
            int? rankId = null;
            decimal? initialSalary = null;
            string salarySource = "";
            var milNum = StringHelpers.NormalizeDigits(retiree.MilitaryNumber);

            // Try to find the latest settlement (Priority 1)
            var settlement = await _unitOfWork.SettlementItems.Query()
                .AsNoTracking()
                .Include(si => si.ProcessedRank)
                .Where(si => !si.IsDeleted && (
                    (retiree.ApprovalItem != null && si.ApprovalItemID == retiree.ApprovalItem.ApprovalItemID) ||
                    si.RetireeID == retiree.RetireeID ||
                    (!string.IsNullOrEmpty(milNum) && si.MilitaryNumber == milNum)
                ))
                .OrderByDescending(si => si.CreatedAt)
                .FirstOrDefaultAsync();

            if (settlement?.ProcessedRank != null)
            {
                rankId = settlement.ProcessedRankID;
                initialSalary = settlement.ProcessedRank.BaseSalary;
                salarySource = "المعاش الإساسي للرتبة المعالجة في قرار التسوية";
            }
            else if (retiree.ApprovalItem != null)
            {
                // Fallback to Approval Item data (Priority 2)
                rankId = retiree.ApprovalItem.RankID;
                if (retiree.ApprovalItem.Salary.HasValue && retiree.ApprovalItem.Salary > 0)
                {
                    initialSalary = retiree.ApprovalItem.Salary.Value;
                    salarySource = "المعاش كشف الإحالة إلى التقاعد";
                }
            }

            var fileNumber = await GenerateNextFileNumberAsync();

            var file = new RetirementFile
            {
                RetireeID = dto.RetireeID,
                FileNumber = fileNumber,
                RankID = rankId,
                PensionBranchID = branchId, // Phase 1.5: استخدام الفرع الفعلي
                // الحالة الافتراضية للملف: قيد المراجعة
                StatusID = FileStatusConstants.New,
                Notes = dto.Notes,
                
                // Auto-populate from Ministry Data
                ServiceStartDate = dto.ServiceStartDate ?? retiree.ApprovalItem?.EnrollmentDate,
                ServiceEndDate = dto.ServiceEndDate ?? retiree.ApprovalItem?.RetirementDate,
                RetirementDate = dto.RetirementDate ?? retiree.ApprovalItem?.RetirementDate,

                FileOpenDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserID = userId
            };

            await _unitOfWork.RetirementFiles.AddAsync(file);
            await _unitOfWork.SaveChangesAsync(); // Save to get FileID

            bool idUploadFailed = false;
            // Upload ID Document (if provided) - Unified Front+Back
            if (dto.IdDocument != null || dto.IdDocumentBack != null)
            {
                try
                {
                    var uploadDto = new UploadDocumentDto
                    {
                        DocumentTypeID = DocumentTypeConstants.NationalId,
                        RetireeID = file.RetireeID,
                        RetirementFileID = file.FileID,
                        Notes = "الهوية الوطنية الموحدة (تم رفعها عند فتح الملف)",
                        File = dto.IdDocument!,
                        FileBack = dto.IdDocumentBack
                    };

                    if (dto.IdDocument == null && dto.IdDocumentBack != null)
                    {
                        uploadDto.File = dto.IdDocumentBack;
                        uploadDto.FileBack = null;
                    }

                    var result = await _documentService.UploadAsync(uploadDto, userId);
                    if (!result.IsSuccess)
                    {
                        idUploadFailed = true;
                        _logger.LogWarning("فشل في دمج الهوية الوطنية للمتقاعد عند فتح الملف: {Message}", result.Message);
                    }
                }
                catch (Exception ex)
                {
                    idUploadFailed = true;
                    _logger.LogWarning(ex, "خطأ غير متوقع عند رفع هوية المتقاعد عند فتح الملف");
                }
            }

            // Upload Death Certificate (if Dead and provided)
            bool deathCertUploadFailed = false;
            if (!dto.IsAlive && dto.DeathCertificate != null)
            {
                try
                {
                    var docPath = await SaveFileAsync(dto.DeathCertificate, "documents");
                    var doc = new PensionMS.Core.Entities.Document
                    {
                        FileID = file.FileID,
                        DocumentTypeID = DocumentTypeConstants.DeathCertificate, // Death Certificate
                        OwnerType = DocumentOwnerType.RetirementFile,
                        FilePath = docPath,
                        FileName = dto.DeathCertificate.FileName,
                        FileSize = dto.DeathCertificate.Length,
                        MimeType = dto.DeathCertificate.ContentType,
                        UploadedByUserID = userId,
                        Verified = false,
                        Notes = "شهادة الوفاة"
                    };
                    await _unitOfWork.Documents.AddAsync(doc);
                    await _unitOfWork.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    deathCertUploadFailed = true;
                    _logger.LogError(ex, "فشل رفع شهادة الوفاة للملف {FileID}", file.FileID);
                }
            }

            // Upload Primary Document (أوليات الملف) (if provided)
            bool primaryDocUploadFailed = false;
            if (dto.PrimaryDocument != null)
            {
                try
                {
                    var docPath = await SaveFileAsync(dto.PrimaryDocument, "documents");
                    var doc = new PensionMS.Core.Entities.Document
                    {
                        FileID = file.FileID,
                        DocumentTypeID = DocumentTypeConstants.FileOriginals, // أوليات الملف
                        OwnerType = DocumentOwnerType.RetirementFile,
                        FilePath = docPath,
                        FileName = dto.PrimaryDocument.FileName,
                        FileSize = dto.PrimaryDocument.Length,
                        MimeType = dto.PrimaryDocument.ContentType,
                        UploadedByUserID = userId,
                        Verified = false,
                        Notes = "أوليات الملف (تم رفعها عند فتح الملف)"
                    };
                    await _unitOfWork.Documents.AddAsync(doc);
                    await _unitOfWork.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    primaryDocUploadFailed = true;
                    _logger.LogError(ex, "فشل رفع الأوليات للملف {FileID}", file.FileID);
                }
            }

            // ── تسجيل النواقص تلقائياً ──────────────────────────────
            var missingDocs = new List<MissingDocument>();

            if ((dto.IdDocument == null || idUploadFailed) && dto.IsAlive)
            {
                missingDocs.Add(new MissingDocument
                {
                    FileID = file.FileID,
                    DocumentTypeID = DocumentTypeConstants.NationalId, // الهوية الوطنية
                    Status = MissingDocumentStatusConstants.Pending,
                    Notes = idUploadFailed ? "فشل رفع هوية المتقاعد (يجب إعادة المحاولة)" : "هوية المتقاعد"
                });
            }

            if (!dto.IsAlive && (dto.DeathCertificate == null || deathCertUploadFailed))
            {
                missingDocs.Add(new MissingDocument
                {
                    FileID = file.FileID,
                    DocumentTypeID = DocumentTypeConstants.DeathCertificate, // شهادة الوفاة
                    Status = MissingDocumentStatusConstants.Pending,
                    Notes = deathCertUploadFailed ? "فشل رفع شهادة وفاة المتقاعد (يجب إعادة المحاولة)" : "شهادة وفاة المتقاعد"
                });
            }

            if (dto.PrimaryDocument == null || primaryDocUploadFailed)
            {
                missingDocs.Add(new MissingDocument
                {
                    FileID = file.FileID,
                    DocumentTypeID = DocumentTypeConstants.FileOriginals, // أوليات الملف
                    Status = MissingDocumentStatusConstants.Pending,
                    Notes = primaryDocUploadFailed ? "فشل رفع أوليات الملف (يجب إعادة المحاولة)" : "أوليات الملف"
                });
            }

            if (missingDocs.Count > 0)
            {
                foreach (var md in missingDocs)
                    await _unitOfWork.MissingDocuments.AddAsync(md);
                await _unitOfWork.SaveChangesAsync();
            }

            // ── إنشاء الاستحقاق المالي تلقائياً ───────────
            if (initialSalary.HasValue && initialSalary.Value > 0)
            {
                var entitlement = new FinancialEntitlement
                {
                    RetireeID = retiree.RetireeID,
                    Amount = initialSalary.Value,
                    EffectiveDate = file.RetirementDate?.ToDateTime(TimeOnly.MinValue) ?? PensionMS.Core.Time.YemenTime.TodayDateTime,
                    Notes = $"استحقاق تلقائي من {salarySource}"
                };
                await _unitOfWork.FinancialEntitlements.AddAsync(entitlement);
                await _unitOfWork.SaveChangesAsync();
            }

            await _unitOfWork.CommitAsync();

            string warning = "";
            if (photoUploadFailed || idUploadFailed || deathCertUploadFailed || primaryDocUploadFailed)
            {
                warning = " (مع تنبيه: فشل رفع بعض الوثائق وتم تسجيلها كنواقص)";
            }

            // Load relationships for DTO mapping (بعد الـ commit)
            var newFile = await _unitOfWork.RetirementFiles.Query()
                .Include(f => f.Retiree)
                .Include(f => f.Rank)
                .Include(f => f.PensionBranch)
                .Include(f => f.Status)
                .FirstOrDefaultAsync(f => f.FileID == file.FileID);

            // حساب البيانات الناقصة للملف الجديد
            var fileId = newFile?.FileID ?? file.FileID;
            var missingData = await CalculateMissingDataAsync(fileId);

            await _notificationService.SendNotificationAsync(new CreateNotificationDto
            {
                UserID = userId,
                Title = "إنشاء ملف تقاعد جديد",
                Message = $"تم إنشاء الملف رقم ({newFile?.FileNumber ?? file.FileNumber}) بنجاح.",
                Type = NotificationConstants.Types.Success,
                Category = NotificationConstants.Categories.Workflow,
                Priority = NotificationConstants.Priorities.Normal,
                ActorID = userId,
                RelatedTableID = SystemTableConstants.RetirementFiles,
                RelatedRecordID = fileId
            });

            return ApiResponse<RetirementFileDto>.Success(MapToDto(newFile ?? file, missingDataCount: missingData.Count, missingDataList: missingData.Items), "تم إنشاء الملف بنجاح" + warning);
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<ApiResponse<bool>> HandleRetireeDeathAsync(
        int fileId, DateOnly deathDate, int userId)
    {
        var file = await _unitOfWork.RetirementFiles.Query()
            .Include(f => f.Retiree)
            .FirstOrDefaultAsync(f => f.FileID == fileId);

        if (file == null)
            return ApiResponse<bool>.Fail("الملف غير موجود");

        if (file.Retiree == null)
            return ApiResponse<bool>.Fail("بيانات المتقاعد غير مرتبطة بهذا الملف");

        if (!file.Retiree.IsAlive)
            return ApiResponse<bool>.Fail("المتقاعد مُسجَّل متوفى مسبقاً");

        if (file.StatusID == FileStatusConstants.Closed)
            return ApiResponse<bool>.Fail("لا يمكن تغيير حالة الحياة لملف مغلق");
        if (!_branchScopeProvider.IsHQ() && file.PensionBranchID != _branchScopeProvider.GetCurrentBranchId())
            return ApiResponse<bool>.Fail("لا تملك صلاحية تعديل ملف في فرع آخر");
        if (deathDate == default || deathDate > PensionMS.Core.Time.YemenTime.Today ||
            (file.Retiree.BirthDate.HasValue && deathDate < file.Retiree.BirthDate.Value))
            return ApiResponse<bool>.Fail("تاريخ الوفاة غير صحيح");
        var sodResult = await _sodGuard.ValidateAsync(fileId, userId, SoDAction.Approve);
        if (!sodResult.IsAllowed)
            return ApiResponse<bool>.Fail(sodResult.Reason ?? "تعارض فصل المهام");

        await _unitOfWork.BeginTransactionAsync();
        try
        {

        // ═════ 1. تسجيل الوفاة ═════
        file.Retiree.IsAlive = false;
        file.Retiree.DeathDate = deathDate;
        file.Retiree.HealthStatusID = HealthStatusConstants.Deceased;
        file.Retiree.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Retirees.Update(file.Retiree);

        // ═════ 2. إيقاف الملف مؤقتاً (Suspended بدلاً من Closed) ═════
        var oldFileStatus = file.StatusID;
        file.StatusID = FileStatusConstants.Suspended;
        file.StatusReason = "وفاة المتقاعد — إيقاف مؤقت لاستكمال إجراءات الورثة";
        file.UpdatedAt = DateTime.UtcNow;
        file.UpdatedByUserID = userId;
        _unitOfWork.RetirementFiles.Update(file);

        // سجل انتقال الحالة
        await _unitOfWork.StatusTransitionLogs.AddAsync(new StatusTransitionLog
        {
            TableID = SystemTableConstants.RetirementFiles,
            RecordID = file.FileID,
            OldStatusID = oldFileStatus,
            NewStatusID = FileStatusConstants.Suspended,
            Reason = $"وفاة المتقاعد بتاريخ {deathDate:yyyy-MM-dd}",
            ChangedByUserID = userId,
            TransitionDate = DateTime.UtcNow
        });

        // ═════ 3. إيقاف المتقاعد كمستلم صرف ═════
        var retireeRecipients = await _unitOfWork.SalaryRecipients.Query()
            .Where(r => r.FileID == fileId
                     && r.RetireeID == file.RetireeID
                     && r.IsActive)
            .ToListAsync();

        foreach (var r in retireeRecipients)
        {
            r.IsActive = false;
            r.EndDate = deathDate.ToDateTime(TimeOnly.MinValue);
            r.Notes = (r.Notes ?? "") + " | إيقاف تلقائي — وفاة المتقاعد";
            r.UpdatedAt = DateTime.UtcNow;
        }

        // ═════ 4. إلغاء دفتر الصرف الحالي ═════
        var activeBooks = await _unitOfWork.PaymentBooks.Query()
            .Where(pb => pb.FileID == fileId
                      && pb.Status == PaymentBookStatus.Active
                      && !pb.IsDeleted)
            .ToListAsync();

        foreach (var book in activeBooks)
        {
            book.Status = PaymentBookStatus.Cancelled;
            book.Notes = (book.Notes ?? "") + " | إلغاء تلقائي — وفاة المتقاعد";
            book.UpdatedAt = DateTime.UtcNow;
        }

        // ═════ 5. فحص أهلية المستحقين ═════
        var entitledPersons = await _unitOfWork.EntitledPersons.Query()
            .Where(e => e.FileID == fileId && !e.IsDeleted
                     && (e.StopCode == EntitledStopCodes.RetireeAlive || e.StopCode == null))
            .ToListAsync();

        foreach (var ep in entitledPersons)
        {
            // استدعاء الخدمة المتخصصة لإعادة تقييم الحالة تلقائياً
            // ستقوم الخدمة بفحص البيانات والوثائق وتحديث IsActive و StopCode
            var result = await _entitledPersonService.ReEvaluateEntitlementStatusAsync(ep.EntitledID);
            if (!result.IsSuccess)
                throw new InvalidOperationException(result.Message);
        }

        // ═════ 6. تسجيل نواقص الوثائق على مستوى الملف ═════
        var missingDocs = new List<MissingDocument>();
        bool hasDeathCert = await _unitOfWork.Documents.Query()
            .AnyAsync(d => d.FileID == fileId
                        && d.DocumentTypeID == DocumentTypeConstants.DeathCertificate
                        && !d.IsDeleted);

        if (!hasDeathCert && !await _unitOfWork.MissingDocuments.Query()
            .AnyAsync(m => m.FileID == fileId && m.DocumentTypeID == DocumentTypeConstants.DeathCertificate &&
                !m.IsDeleted && (m.Status == MissingDocumentStatusConstants.Pending || m.Status == MissingDocumentStatusConstants.Pledged)))
        {
            missingDocs.Add(new MissingDocument
            {
                FileID = fileId,
                DocumentTypeID = DocumentTypeConstants.DeathCertificate,
                Status = MissingDocumentStatusConstants.Pending,
                Notes = "شهادة وفاة المتقاعد"
            });
        }

        // حصر الورثة
        bool hasInheritanceCert = await _unitOfWork.Documents.Query()
            .AnyAsync(d => d.FileID == fileId
                        && d.DocumentTypeID == DocumentTypeConstants.InheritanceCert
                        && !d.IsDeleted);

        if (!hasInheritanceCert && !await _unitOfWork.MissingDocuments.Query()
            .AnyAsync(m => m.FileID == fileId && m.DocumentTypeID == DocumentTypeConstants.InheritanceCert &&
                !m.IsDeleted && (m.Status == MissingDocumentStatusConstants.Pending || m.Status == MissingDocumentStatusConstants.Pledged)))
        {
            missingDocs.Add(new MissingDocument
            {
                FileID = fileId,
                DocumentTypeID = DocumentTypeConstants.InheritanceCert,
                Status = MissingDocumentStatusConstants.Pending,
                Notes = "حصر ورثة المتقاعد المتوفى"
            });
        }

        // حفظ جميع النواقص
        foreach (var md in missingDocs)
            await _unitOfWork.MissingDocuments.AddAsync(md);

        await _unitOfWork.SaveChangesAsync();

        await _complianceEngine.RefreshComplianceCacheAsync(fileId);

        await _auditService.LogAsync(userId, "", "RecordRetireeDeath", "Retiree",
            file.RetireeID, JsonSerializer.Serialize(new { IsAlive = true, DeathDate = (DateOnly?)null }),
            JsonSerializer.Serialize(new { IsAlive = false, DeathDate = deathDate }), file.PensionBranchID);
        await _unitOfWork.CommitAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }

        try
        {
            var notifyDeathUserId = file.AssignedEmployeeID;
            if (notifyDeathUserId == null || notifyDeathUserId == 0)
            {
                var creatorId = await _unitOfWork.AuditLogs.Query()
                    .Where(a => a.TableID == SystemTableConstants.RetirementFiles && a.RecordID == fileId && a.Action == "Create")
                    .Select(a => a.UserID)
                    .FirstOrDefaultAsync();
                notifyDeathUserId = creatorId;
            }

            if (notifyDeathUserId.HasValue && notifyDeathUserId.Value > 0 && notifyDeathUserId.Value != userId)
            {
                await _notificationService.SendNotificationAsync(new CreateNotificationDto
                {
                    UserID = notifyDeathUserId.Value,
                    Title = "تسجيل وفاة متقاعد",
                    Message = $"تم تسجيل وفاة المتقاعد ({file.Retiree.FullName}) في الملف رقم ({file.FileNumber}) وإيقاف الملف مؤقتاً.",
                    Type = NotificationConstants.Types.Warning,
                    Category = NotificationConstants.Categories.Workflow,
                    Priority = NotificationConstants.Priorities.High,
                    ActorID = userId,
                    RelatedTableID = SystemTableConstants.RetirementFiles,
                    RelatedRecordID = file.FileID
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "تعذر إرسال إشعار وفاة المتقاعد للملف {FileID}", fileId);
        }

        return ApiResponse<bool>.Success(true,
            "تم تسجيل وفاة المتقاعد وإيقاف الملف مؤقتاً لاستكمال إجراءات الورثة");
    }

    public async Task<ApiResponse<bool>> CorrectRetireeLifeStatusAsync(
        int fileId, CorrectRetireeLifeStatusDto dto, int userId)
    {
        if (string.IsNullOrWhiteSpace(dto.Reason) || dto.Reason.Trim().Length < 10)
            return ApiResponse<bool>.Fail("سبب التصحيح يجب ألا يقل عن 10 أحرف");
        if (dto.HealthStatusID is < HealthStatusConstants.Healthy or > HealthStatusConstants.FullyDisabled)
            return ApiResponse<bool>.Fail("الحالة الصحية المختارة غير صحيحة");

        var file = await _unitOfWork.RetirementFiles.Query()
            .Include(f => f.Retiree)
            .FirstOrDefaultAsync(f => f.FileID == fileId);
        if (file?.Retiree == null)
            return ApiResponse<bool>.Fail("الملف أو بيانات المتقاعد غير موجودة");
        if (file.Retiree.IsAlive && file.Retiree.DeathDate == null &&
            file.Retiree.HealthStatusID is not (HealthStatusConstants.Deceased or HealthStatusConstants.Martyr))
            return ApiResponse<bool>.Fail("بيانات حالة حياة المتقاعد سليمة بالفعل");
        if (file.StatusID == FileStatusConstants.Closed)
            return ApiResponse<bool>.Fail("لا يمكن تغيير حالة الحياة لملف مغلق");
        if (!_branchScopeProvider.IsHQ() && file.PensionBranchID != _branchScopeProvider.GetCurrentBranchId())
            return ApiResponse<bool>.Fail("لا تملك صلاحية تعديل ملف في فرع آخر");

        var sodResult = await _sodGuard.ValidateAsync(fileId, userId, SoDAction.Override);
        if (!sodResult.IsAllowed)
            return ApiResponse<bool>.Fail(sodResult.Reason ?? "تعارض فصل المهام");

        var oldIsAlive = file.Retiree.IsAlive;
        var oldDeathDate = file.Retiree.DeathDate;
        var oldFileStatus = file.StatusID;
        var now = DateTime.UtcNow;
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            file.Retiree.IsAlive = true;
            file.Retiree.DeathDate = null;
            file.Retiree.HealthStatusID = dto.HealthStatusID;
            file.Retiree.UpdatedAt = now;
            _unitOfWork.Retirees.Update(file.Retiree);

            file.StatusID = FileStatusConstants.UnderReview;
            file.PreviousStatusID = FileStatusConstants.Suspended;
            file.ReviewStatus = FileReviewStatus.PendingReview;
            file.ReviewRequestedAt = now;
            file.StatusReason = "تصحيح حالة حياة المتقاعد — الصرف موقوف حتى المراجعة والاعتماد: " + dto.Reason.Trim();
            file.UpdatedAt = now;
            file.UpdatedByUserID = userId;
            _unitOfWork.RetirementFiles.Update(file);

            if (oldFileStatus != FileStatusConstants.UnderReview)
                await _unitOfWork.StatusTransitionLogs.AddAsync(new StatusTransitionLog
                {
                    TableID = SystemTableConstants.RetirementFiles,
                    RecordID = fileId,
                    OldStatusID = oldFileStatus,
                    NewStatusID = FileStatusConstants.UnderReview,
                    Reason = "تصحيح حالة حياة المتقاعد: " + dto.Reason.Trim(),
                    ChangedByUserID = userId,
                    TransitionDate = now
                });

            var entitled = await _unitOfWork.EntitledPersons.Query()
                .Where(e => e.FileID == fileId && !e.IsDeleted && !e.IsDeceased)
                .ToListAsync();
            foreach (var person in entitled)
            {
                if (person.IsActive || person.StopCode == null || person.StopCode == EntitledStopCodes.MissingData)
                {
                    person.IsActive = false;
                    person.StopCode = EntitledStopCodes.RetireeAlive;
                    person.StopReason = "المتقاعد على قيد الحياة — الاستحقاق معلق";
                    person.StopDate = now;
                    person.UpdatedAt = now;
                }
            }

            var activeRecipients = await _unitOfWork.SalaryRecipients.Query()
                .Where(r => r.FileID == fileId && r.IsActive)
                .ToListAsync();
            foreach (var recipient in activeRecipients)
            {
                recipient.IsActive = false;
                recipient.EndDate = now;
                recipient.Notes = (recipient.Notes ?? "") + " | إيقاف بعد تصحيح حالة حياة المتقاعد";
                recipient.UpdatedAt = now;
            }

            var activeAgents = await _unitOfWork.Agents.Query()
                .Where(a => a.FileID == fileId && a.IsActive && !a.IsDeleted)
                .ToListAsync();
            foreach (var agent in activeAgents)
            {
                agent.IsActive = false;
                agent.UpdatedAt = now;
            }

            var activeBooks = await _unitOfWork.PaymentBooks.Query()
                .Where(pb => pb.FileID == fileId && pb.Status == PaymentBookStatus.Active && !pb.IsDeleted)
                .ToListAsync();
            foreach (var book in activeBooks)
            {
                book.Status = PaymentBookStatus.Cancelled;
                book.Notes = (book.Notes ?? "") + " | إلغاء بعد تصحيح حالة حياة المتقاعد";
                book.UpdatedAt = now;
            }

            var deathRequirements = await _unitOfWork.MissingDocuments.Query()
                .Where(m => m.FileID == fileId && m.Status == MissingDocumentStatusConstants.Pending &&
                    (m.Notes == "شهادة وفاة المتقاعد" || m.Notes == "حصر ورثة المتقاعد المتوفى"))
                .ToListAsync();
            foreach (var requirement in deathRequirements)
            {
                requirement.IsDeleted = true;
                requirement.DeletedAt = now;
                requirement.DeletedByUserID = userId;
            }

            await _unitOfWork.SaveChangesAsync();
            var reviewResult = await _reviewService.RequestReviewAsync(fileId, userId, now);
            if (!reviewResult.IsSuccess)
                throw new InvalidOperationException(reviewResult.Message);
            await _complianceEngine.RefreshComplianceCacheAsync(fileId);
            await _auditService.LogAsync(userId, "", "CorrectRetireeLifeStatus", "Retiree",
                file.RetireeID,
                JsonSerializer.Serialize(new { IsAlive = oldIsAlive, DeathDate = oldDeathDate, FileStatusID = oldFileStatus }),
                JsonSerializer.Serialize(new { IsAlive = true, DeathDate = (DateOnly?)null, FileStatusID = file.StatusID, Reason = dto.Reason.Trim() }),
                file.PensionBranchID);
            await _unitOfWork.CommitAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }

        if (file.AssignedEmployeeID.HasValue && file.AssignedEmployeeID.Value > 0 && file.AssignedEmployeeID.Value != userId)
        {
            try
            {
                await _notificationService.SendNotificationAsync(new CreateNotificationDto
                {
                    UserID = file.AssignedEmployeeID.Value,
                    Title = "تصحيح حالة حياة متقاعد",
                    Message = $"تم تصحيح حالة المتقاعد ({file.Retiree.FullName}) في الملف رقم ({file.FileNumber})، والملف بحاجة إلى مراجعة قبل استئناف الصرف.",
                    Type = NotificationConstants.Types.Warning,
                    Category = NotificationConstants.Categories.Workflow,
                    Priority = NotificationConstants.Priorities.High,
                    ActorID = userId,
                    RelatedTableID = SystemTableConstants.RetirementFiles,
                    RelatedRecordID = fileId
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "تعذر إرسال إشعار تصحيح حالة حياة المتقاعد للملف {FileID}", fileId);
            }
        }
        return ApiResponse<bool>.Success(true,
            "تم تصحيح حالة المتقاعد وإحالة الملف للمراجعة مع إبقاء الصرف موقوفاً");
    }


    // Phase 4.3: إصلاح Race condition مع retry logic


    private async Task<string> GenerateNextFileNumberAsync()
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"{year}-";
        
        // This is safe under a serialized transaction or table lock. 
        // Best practice is to use a DB Sequence (e.g. NEXT VALUE FOR FileNumberSeq)
        var lastFile = await _unitOfWork.RetirementFiles.Query()
            .Where(f => f.FileNumber.StartsWith(prefix))
            .OrderByDescending(f => f.FileNumber)
            .FirstOrDefaultAsync();

        int nextSeq = 1;
        if (lastFile != null)
        {
            var parts = lastFile.FileNumber.Split('-');
            if (parts.Length > 1 && int.TryParse(parts[1], out int lastSeq))
            {
                nextSeq = lastSeq + 1;
            }
        }

        return $"{prefix}{nextSeq:D5}";
    }

    public async Task<ApiResponse<RetirementFileDto>> UpdateAsync(int id, UpdateRetirementFileDto dto, int userId)
    {
        var file = await _unitOfWork.RetirementFiles.Query()
            .Include(f => f.Retiree)
            .FirstOrDefaultAsync(f => f.FileID == id);
        
        if (file == null)
            return ApiResponse<RetirementFileDto>.Fail("الملف غير موجود");

        // ── Phase 1.4: منع تعديل ملف مغلق ──
        if (file.StatusID == FileStatusConstants.Closed)
            return ApiResponse<RetirementFileDto>.Fail("لا يمكن تعديل ملف مغلق");

        // ── Phase 2.3: فحص التزامن (Optimistic Concurrency) ──
        if (dto.RowVersion != null && file.RowVersion != null
            && !dto.RowVersion.SequenceEqual(file.RowVersion))
            return ApiResponse<RetirementFileDto>.Fail(
                "تم تعديل الملف من قبل مستخدم آخر. يرجى تحديث الصفحة والمحاولة مرة أخرى");

        if (file.Retiree != null &&
            ((dto.IsAlive.HasValue && dto.IsAlive.Value != file.Retiree.IsAlive) ||
             (dto.DeathDate.HasValue && dto.DeathDate != file.Retiree.DeathDate) ||
             (dto.HealthStatusID.HasValue &&
              (dto.HealthStatusID == HealthStatusConstants.Deceased || dto.HealthStatusID == HealthStatusConstants.Martyr) !=
              !file.Retiree.IsAlive)))
            return ApiResponse<RetirementFileDto>.Fail("تغيير حالة حياة المتقاعد يتطلب الإجراء المخصص من صفحة الملف");

        // م27: أخطاء المرفقات تُجمع وتظهر في الاستجابة بدلاً من إخفائها
        var attachmentErrors = new List<string>();

        // Update file fields
        if (dto.RankID.HasValue) file.RankID = dto.RankID;
        if (dto.BranchID.HasValue) file.PensionBranchID = dto.BranchID.Value;
        // Phase 1.3: تم إزالة تعيين FileStatusID المباشر — استخدم ChangeStatusAsync بدلاً من ذلك
        if (dto.RetirementDate.HasValue) file.RetirementDate = dto.RetirementDate;
        if (dto.ServiceStartDate.HasValue) file.ServiceStartDate = dto.ServiceStartDate;
        if (dto.ServiceEndDate.HasValue) file.ServiceEndDate = dto.ServiceEndDate;
        if (dto.Notes != null) file.Notes = dto.Notes;
        
        file.UpdatedAt = DateTime.UtcNow;
        file.UpdatedByUserID = userId;

        // Update retiree fields if provided
        if (file.Retiree != null)
        {
            if (!string.IsNullOrWhiteSpace(dto.FullName)) file.Retiree.FullName = dto.FullName;
            if (!string.IsNullOrWhiteSpace(dto.NationalID)) file.Retiree.NationalID = dto.NationalID;
            if (dto.Gender.HasValue) file.Retiree.Gender = dto.Gender.Value;
            if (dto.SocialStatusID.HasValue) file.Retiree.SocialStatusID = dto.SocialStatusID;
            if (dto.HealthStatusID.HasValue) file.Retiree.HealthStatusID = dto.HealthStatusID;
            if (dto.Phone != null) file.Retiree.Phone = dto.Phone;
            if (dto.Address != null) file.Retiree.Address = dto.Address;
            if (dto.City != null) file.Retiree.City = dto.City;
            if (dto.Governorate != null) file.Retiree.Governorate = dto.Governorate;
            if (dto.BirthDate.HasValue) file.Retiree.BirthDate = dto.BirthDate;
            if (dto.MilitaryNumber != null) file.Retiree.MilitaryNumber = dto.MilitaryNumber;

            // تحديث الصورة الشخصية
            if (dto.PersonalPhoto != null && dto.PersonalPhoto.Length > 0)
            {
                if (!string.IsNullOrEmpty(file.Retiree.PersonalPhotoPath))
                    await _fileStorage.DeleteFileAsync(file.Retiree.PersonalPhotoPath);

                using var stream = dto.PersonalPhoto.OpenReadStream();
                file.Retiree.PersonalPhotoPath = await _fileStorage.SaveFileAsync(stream, dto.PersonalPhoto.FileName, "photos");
            }

            file.Retiree.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Retirees.Update(file.Retiree);

            // تحديث الهوية الوطنية (دمج تلقائي)
            if (dto.IdDocument != null || dto.IdDocumentBack != null)
            {
                try
                {
                    var uploadDto = new UploadDocumentDto
                    {
                        DocumentTypeID = DocumentTypeConstants.NationalId,
                        RetireeID = file.RetireeID,
                        RetirementFileID = file.FileID,
                        Notes = $"تحديث الهوية الوطنية للمتقاعد: {file.Retiree.FullName}",
                        File = dto.IdDocument!,
                        FileBack = dto.IdDocumentBack
                    };

                    if (dto.IdDocument == null && dto.IdDocumentBack != null)
                    {
                        uploadDto.File = dto.IdDocumentBack;
                        uploadDto.FileBack = null;
                    }

                    var idResult = await _documentService.UploadAsync(uploadDto, userId);
                    if (!idResult.IsSuccess)
                    {
                        _logger.LogWarning("فشل في تحديث هوية المتقاعد (ملف {FileID}): {Message}", file.FileID, idResult.Message);
                        attachmentErrors.Add($"الهوية الوطنية: {idResult.Message}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "فشل في تحديث هوية المتقاعد (ملف {FileID})", file.FileID);
                    attachmentErrors.Add("الهوية الوطنية: تعذر الرفع بشكل غير متوقع");
                }
            }

            // تحديث شهادة الوفاة
            if (dto.DeathCertificate != null && dto.DeathCertificate.Length > 0)
            {
                try
                {
                    var uploadDto = new UploadDocumentDto
                    {
                        DocumentTypeID = DocumentTypeConstants.DeathCertificate,
                        RetireeID = file.RetireeID,
                        RetirementFileID = file.FileID,
                        Notes = $"تحديث شهادة الوفاة للمتقاعد: {file.Retiree.FullName}",
                        File = dto.DeathCertificate,
                    };
                    var deathResult = await _documentService.UploadAsync(uploadDto, userId);
                    if (!deathResult.IsSuccess)
                    {
                        _logger.LogWarning("فشل في تحديث شهادة الوفاة (ملف {FileID}): {Message}", file.FileID, deathResult.Message);
                        attachmentErrors.Add($"شهادة الوفاة: {deathResult.Message}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "فشل في تحديث شهادة الوفاة (ملف {FileID})", file.FileID);
                    attachmentErrors.Add("شهادة الوفاة: تعذر الرفع بشكل غير متوقع");
                }
            }
        }
        _unitOfWork.RetirementFiles.Update(file);
        var now = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        // ── Immutable Audit Log ──
        await _auditService.LogAsync(userId, "", "Update", "RetirementFile", file.FileID, null, null);

        // ── طلب مراجعة بعد التعديل ──
        await _reviewService.RequestReviewAsync(file.FileID, userId, now);

        // Reload with all relationships for DTO mapping
        var updated = await _unitOfWork.RetirementFiles.Query()
            .AsNoTracking()
            .Include(f => f.Retiree)
            .Include(f => f.Rank)
            .Include(f => f.PensionBranch)
            .Include(f => f.Status)
            .Include(f => f.AssignedEmployee)
            .Include(f => f.Documents.Where(d => d.DocumentTypeID == 1 && !d.IsDeleted))
            .FirstAsync(f => f.FileID == id);

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
        
        var missingDocsList = await GetMissingDocsListAsync(id);

        var activeBook = await _unitOfWork.PaymentBooks.Query()
            .AsNoTracking()
            .Where(pb => pb.FileID == id && pb.Status == PaymentBookStatus.Active)
            .OrderByDescending(pb => pb.CreatedAt)
            .FirstOrDefaultAsync();

        // حساب البيانات الناقصة
        var missingData = await CalculateMissingDataAsync(id);

        var notifyUserId = updated.AssignedEmployeeID;
        if (notifyUserId == null || notifyUserId == 0)
        {
            var creatorId = await _unitOfWork.AuditLogs.Query()
                .Where(a => a.TableID == SystemTableConstants.RetirementFiles && a.RecordID == id && a.Action == "Create")
                .Select(a => a.UserID)
                .FirstOrDefaultAsync();
            notifyUserId = creatorId;
        }

        if (notifyUserId.HasValue && notifyUserId.Value > 0 && notifyUserId.Value != userId)
        {
            await _notificationService.SendNotificationAsync(new CreateNotificationDto
            {
                UserID = notifyUserId.Value,
                Title = "تحديث بيانات الملف",
                Message = $"تم تحديث بيانات الملف رقم ({updated.FileNumber}).",
                Type = NotificationConstants.Types.Info,
                Category = NotificationConstants.Categories.Workflow,
                Priority = NotificationConstants.Priorities.Normal,
                ActorID = userId,
                RelatedTableID = SystemTableConstants.RetirementFiles,
                RelatedRecordID = updated.FileID
            });
        }

        // م27: نتيجة منفصلة وواضحة للمرفقات
        var fileMessage = "تم تحديث الملف وبيانات المتقاعد";
        if (attachmentErrors.Count > 0)
            fileMessage += " — تعذر رفع بعض المرفقات: " + string.Join("؛ ", attachmentErrors);

        var fileResponse = ApiResponse<RetirementFileDto>.Success(MapToDto(updated, entitledCount, agentsCount, guardiansCount, missingDocsList.Count, missingDocsList, missingData.Count, missingData.Items, activeBook), fileMessage);
        if (attachmentErrors.Count > 0)
            fileResponse.Errors = attachmentErrors;
        return fileResponse;
    }

    public async Task<ApiResponse<bool>> ChangeStatusAsync(int id, int newStatusId, string? reason, int userId)
    {
        var file = await _unitOfWork.RetirementFiles.Query()
            .Include(f => f.MissingDocuments)
            .Include(f => f.Documents.Where(d => !d.IsDeleted))
            .FirstOrDefaultAsync(f => f.FileID == id);

        if (file == null)
            return ApiResponse<bool>.Fail("الملف غير موجود");

        // ── 1. Transition Validation Matrix ──
        var allowed = GetAllowedTransitions(file.StatusID ?? 0);
        if (!allowed.Contains(newStatusId))
            return ApiResponse<bool>.Fail($"لا يمكن الانتقال من الحالة {file.StatusID} إلى {newStatusId}");

        if (newStatusId == FileStatusConstants.Active && file.ReviewStatus == FileReviewStatus.PendingReview)
            return ApiResponse<bool>.Fail("لا يمكن تفعيل الملف قبل اعتماد المراجعة المطلوبة");

        // ── 2. SoD Guard — فحص تعارض المصالح ──
        var sodAction = newStatusId switch
        {
            FileStatusConstants.UnderReview => SoDAction.Approve,
            FileStatusConstants.Active => SoDAction.Activate,
            _ => (SoDAction?)null
        };

        if (sodAction.HasValue)
        {
            var sodResult = await _sodGuard.ValidateAsync(id, userId, sodAction.Value);
            if (!sodResult.IsAllowed)
                return ApiResponse<bool>.Fail(sodResult.Reason ?? "تعارض فصل المهام");
        }

        // ── 3. Pre-condition Guards ──
        switch (newStatusId)
        {
            case FileStatusConstants.Active: // نشط — Phase 3.4: شروط التفعيل المعززة
                var hasEntitlement = await _unitOfWork.FinancialEntitlements.Query()
                    .AnyAsync(fe => fe.RetireeID == file.RetireeID && !fe.IsDeleted);
                if (!hasEntitlement)
                    return ApiResponse<bool>.Fail("لا يمكن تفعيل الملف بدون استحقاق مالي");

                // Phase 3.4: فحص الوثائق الناقصة (يدوياً + تلقائياً)
                var manuallyPendingDocs = file.MissingDocuments.Any(m => MissingDocumentStatusConstants.OutstandingStatuses.Contains(m.Status));
                
                // Auto-detect missing required documents (global + contextual)
                var requiredDocTypes = await GetContextualRequiredDocTypeIdsAsync(id);
                
                var uploadedDocTypeIds = file.Documents?
                    .Where(d => !d.IsDeleted)
                    .Select(d => d.DocumentTypeID)
                    .Distinct()
                    .ToList() ?? new List<int>();
                
                var manuallyMissingTypeIds = file.MissingDocuments
                    .Select(m => m.DocumentTypeID)
                    .Distinct()
                    .ToList();
                
                var hasAutoDetectedMissing = requiredDocTypes
                    .Any(dt => !uploadedDocTypeIds.Contains(dt) && !manuallyMissingTypeIds.Contains(dt));
                
                if (manuallyPendingDocs || hasAutoDetectedMissing)
                    return ApiResponse<bool>.Fail("لا يمكن تفعيل الملف مع وجود وثائق ناقصة");
                break;

            case FileStatusConstants.Closed: // مغلق — auto-set ClosedDate
                file.ClosedDate = DateTime.UtcNow;
                break;
        }

        // ── 4. Log Transition ──
        var log = new StatusTransitionLog
        {
            TableID = SystemTableConstants.RetirementFiles, // RetirementFiles (from SystemTables seed)
            RecordID = file.FileID,
            OldStatusID = file.StatusID,
            NewStatusID = newStatusId,
            Reason = reason,
            ChangedByUserID = userId,
            TransitionDate = DateTime.UtcNow
        };

        // ── 5. Apply ──
        file.StatusID = newStatusId;
        file.StatusReason = reason;
        file.UpdatedAt = DateTime.UtcNow;
        file.UpdatedByUserID = userId;

        _unitOfWork.RetirementFiles.Update(file);
        await _unitOfWork.StatusTransitionLogs.AddAsync(log);
        await _unitOfWork.SaveChangesAsync();

        // ── 6. Workflow & Fraud Detection (Phase 5) ──
        await _workflowEngine.RouteFileAsync(file.FileID, $"ChangeStatus_{newStatusId}", userId);
        await _riskScoringService.EvaluateRiskAsync(file.FileID, $"ChangeStatus_{newStatusId}", userId, null, newStatusId.ToString());

        // ── 7. Immutable Audit + Snapshot (Phase 3) ──
        await _auditService.LogAsync(
            userId, "", "ChangeStatus", "RetirementFile", file.FileID,
            log.OldStatusID?.ToString(), newStatusId.ToString());
        await _snapshotService.CaptureAsync(file.FileID, "StatusChange", userId, reason);

        // إشعار لمدخل البيانات (منشئ الملف) بتحديث الحالة
        var fileCreatorId = await _unitOfWork.AuditLogs.Query()
            .Where(a => a.TableID == SystemTableConstants.RetirementFiles && a.RecordID == id && a.Action == "Create")
            .Select(a => a.UserID)
            .FirstOrDefaultAsync();

        if (fileCreatorId.HasValue && fileCreatorId.Value > 0)
        {
            await _notificationService.SendNotificationAsync(new CreateNotificationDto
            {
                UserID = fileCreatorId.Value,
                Title = "تحديث حالة الملفات",
                Message = $"تم تغيير حالة ملفك رقم ({file.FileNumber}) إلى [{newStatusId}].",
                Type = NotificationConstants.Types.Success,
                Category = NotificationConstants.Categories.Workflow,
                Priority = NotificationConstants.Priorities.Normal,
                ActorID = userId,
                RelatedTableID = SystemTableConstants.RetirementFiles,
                RelatedRecordID = file.FileID
            });
        }

        return ApiResponse<bool>.Success(true, "تم تغيير حالة الملفات بنجاح");
        }

    /// <summary>
/// مصفوفة التحولات المسموحة لحالات الملف
/// جديد → تحت المراجعة → نشط (اعتماد) أو معلق (استكمال نواقص) → نشط
/// </summary>
private static HashSet<int> GetAllowedTransitions(int currentStatus) => currentStatus switch
{
    FileStatusConstants.New         => new() { FileStatusConstants.UnderReview, FileStatusConstants.Pending, FileStatusConstants.Active },
    FileStatusConstants.UnderReview => new() { FileStatusConstants.Active, FileStatusConstants.Pending },
    FileStatusConstants.Pending     => new() { FileStatusConstants.Active, FileStatusConstants.UnderReview },
    FileStatusConstants.Active      => new() { FileStatusConstants.Suspended, FileStatusConstants.Closed },
    FileStatusConstants.Suspended   => new() { FileStatusConstants.Active, FileStatusConstants.Closed },
    FileStatusConstants.Closed      => new(),  // Terminal — no further transitions
    _ => new()
};

    public async Task<ApiResponse<List<FileStatusDto>>> GetFileStatusesAsync()
    {
        // نعرض فقط الحالات المطلوبة في الواجهة:
        // قيد المراجعة، نشط، موقوف، مغلق
        var allowedIds = new[]
        {
            FileStatusConstants.UnderReview,
            FileStatusConstants.Active,
            FileStatusConstants.Suspended,
            FileStatusConstants.Closed
        };

        // ملاحظة: لتفادي أي تعقيد في الـ SQL (مثل استخدام WITH/CTE)،
        // نقوم بالترشيح في الذاكرة بدلاً من داخل الاستعلام.
        var allStatuses = await _unitOfWork.FileStatuses.Query()
            .AsNoTracking()
            .ToListAsync();

        var statuses = allStatuses
            .Where(s => allowedIds.Contains(s.StatusID))
            .Select(s => new FileStatusDto
            {
                StatusID = s.StatusID,
                StatusName = s.StatusID == FileStatusConstants.UnderReview
                    ? "قيد المراجعة"
                    : s.StatusName,
                Color = s.StatusColor
            })
            .ToList();

        return ApiResponse<List<FileStatusDto>>.Success(statuses);
    }

    // ── Phase 3.1: الحالات المسموحة ديناميكياً ──
    public async Task<ApiResponse<List<FileStatusDto>>> GetAllowedStatusTransitionsAsync(int fileId)
    {
        var file = await _unitOfWork.RetirementFiles.GetByIdAsync(fileId);
        if (file == null)
            return ApiResponse<List<FileStatusDto>>.Fail("الملف غير موجود");

        var allowedIds = GetAllowedTransitions(file.StatusID ?? 0);
        if (allowedIds.Count == 0)
            return ApiResponse<List<FileStatusDto>>.Success(new List<FileStatusDto>());

        var allStatuses = await _unitOfWork.FileStatuses.Query()
            .AsNoTracking()
            .ToListAsync();

        var statuses = allStatuses
            .Where(s => allowedIds.Contains(s.StatusID))
            .Select(s => new FileStatusDto
            {
                StatusID = s.StatusID,
                StatusName = s.StatusID == FileStatusConstants.UnderReview
                    ? "قيد المراجعة"
                    : s.StatusName,
                Color = s.StatusColor
            })
            .ToList();

        return ApiResponse<List<FileStatusDto>>.Success(statuses);
    }

    // ── Phase 3.3: إسناد الملف لموظف ──
    public async Task<ApiResponse<bool>> AssignEmployeeAsync(int fileId, int employeeId, int userId)
    {
        var file = await _unitOfWork.RetirementFiles.GetByIdAsync(fileId);
        if (file == null)
            return ApiResponse<bool>.Fail("الملف غير موجود");

        var employee = await _unitOfWork.Users.Query()
            .AsNoTracking()
            .AnyAsync(u => u.UserID == employeeId && u.IsActive);
        if (!employee)
            return ApiResponse<bool>.Fail("الموظف غير موجود أو غير نشط");

        file.AssignedEmployeeID = employeeId;
        file.UpdatedAt = DateTime.UtcNow;
        file.UpdatedByUserID = userId;
        _unitOfWork.RetirementFiles.Update(file);
        await _unitOfWork.SaveChangesAsync();

        // إرسال إشعار للموظف المسند إليه الملف
        await _notificationService.SendNotificationAsync(new CreateNotificationDto
        {
            UserID = employeeId,
            Title = "إسناد ملف جديد",
            Message = $"تم إسناد الملف رقم ({file.FileNumber}) إليك للمراجعة واتخاذ اللازم.",
            Type = NotificationConstants.Types.Info,
            Category = NotificationConstants.Categories.Workflow,
            Priority = NotificationConstants.Priorities.Normal,
            ActorID = userId,
            RelatedTableID = SystemTableConstants.RetirementFiles,
            RelatedRecordID = file.FileID
        });

        return ApiResponse<bool>.Success(true, "تم إسناد الملف بنجاح");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id, int deletedByUserId)
    {
        var file = await _unitOfWork.RetirementFiles.GetByIdAsync(id);
        if (file == null)
            return ApiResponse<bool>.Fail("الملف غير موجود");

        // ── Phase 1.4: منع حذف الملفات النشطة أو الهامة ──
        if (file.StatusID == FileStatusConstants.Active || 
            file.StatusID == FileStatusConstants.Suspended || 
            file.StatusID == FileStatusConstants.Closed)
        {
            return ApiResponse<bool>.Fail("لا يمكن حذف هذا الملف في حالته الحالية (نشط/موقوف/مغلق)");
        }

        file.IsDeleted = true;
        file.DeletedAt = DateTime.UtcNow;
        file.DeletedByUserID = deletedByUserId;

        _unitOfWork.RetirementFiles.Update(file);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.Success(true, "تم حذف الملف");
    }

    private static RetirementFileDto MapToDto(
        RetirementFile f,
        int entitledCount = 0,
        int agentsCount = 0,
        int guardiansCount = 0,
        int missingDocsCount = 0,
        List<string>? missingDocsList = null,
        int missingDataCount = 0,
        List<string>? missingDataList = null,
        PaymentBook? activeBook = null,
        bool hasMinors = false,
        bool hasPledgesOrExemptions = false,
        bool hasSettlements = false) => new()
    {
        FileID = f.FileID,
        RetireeID = f.RetireeID,
        FileNumber = f.FileNumber,
        RetireeFullName = f.Retiree?.FullName ?? "",
        NationalID = f.Retiree?.NationalID,
        RankName = f.Rank?.RankName,
        BranchName = f.PensionBranch?.BranchName,
        MilitaryNumber = f.Retiree?.MilitaryNumber,
        StatusID = f.StatusID,
        FileStatusName = f.Status?.StatusID == FileStatusConstants.UnderReview
            ? "قيد المراجعة"
            : f.Status?.StatusName,
        RetirementDate = f.RetirementDate,
        ServiceStartDate = f.ServiceStartDate,
        ServiceEndDate = f.ServiceEndDate,
        FileOpenDate = f.FileOpenDate,
        ClosedDate = f.ClosedDate,
        StatusReason = f.StatusReason,
        Notes = f.Notes,
        CreatedAt = f.CreatedAt,
        UpdatedAt = f.UpdatedAt,
        AssignedEmployeeID = f.AssignedEmployeeID,
        AssignedEmployeeName = f.AssignedEmployee?.FullName,
        // Active Payment Book
        ActivePaymentBookNumber = activeBook?.BookNumber,
        ActivePaymentBookExpiry = activeBook?.ExpiryDate,
        ActivePaymentBookStatus = activeBook?.Status,
        // Retiree Additional Info
        BirthDate = f.Retiree?.BirthDate,
        Gender = f.Retiree?.Gender ?? 0,
        Address = f.Retiree?.Address,
        DeathDate = f.Retiree?.DeathDate,
        IsAlive = f.Retiree?.IsAlive ?? true,
        EntitledPersonsCount = entitledCount,
        AgentsCount = agentsCount,
        GuardiansCount = guardiansCount,
        MissingDocumentsCount = missingDocsCount,
        MissingDocumentsList = missingDocsList ?? new List<string>(),
        MissingDataCount = missingDataCount,
        MissingDataList = missingDataList ?? new List<string>(),
        PersonalPhotoPath = f.Retiree?.PersonalPhotoPath,
        IdDocumentPath = f.Documents?.FirstOrDefault(d => d.DocumentTypeID == 1)?.FilePath,
        // Contextual Flags
        HasMinors = hasMinors,
        HasPledgesOrExemptions = hasPledgesOrExemptions,
        HasSettlements = hasSettlements,
        // Review Status
        ReviewStatus = (int)f.ReviewStatus,
        ReviewRequestedAt = f.ReviewRequestedAt,
        ReviewCompletedAt = f.ReviewCompletedAt,
        ReviewNotes = f.ReviewNotes,
    };

    /// <summary>
    /// حساب عدد وقائمة البيانات الناقصة للملف ولجميع الكيانات (متقاعد، وكلاء، أوصياء، مستحقون) بشكل كامل وبدون استثناء
    /// </summary>
    private async Task<(int Count, List<string> Items)> CalculateMissingDataAsync(int fileId)
    {
        var file = await _unitOfWork.RetirementFiles.Query()
            .AsNoTracking()
            .Include(f => f.Retiree)
            .Include(f => f.EntitledPersons.Where(e => !e.IsDeleted && e.IsActive))
            .Include(f => f.Agents.Where(a => !a.IsDeleted && a.IsActive))
            .Include(f => f.Guardians.Where(g => !g.IsDeleted && g.IsActive))
            .FirstOrDefaultAsync(f => f.FileID == fileId);

        return CalculateMissingData(file);
    }

    private (int Count, List<string> Items) CalculateMissingData(RetirementFile? file)
    {
        if (file == null) return (0, new List<string>());

        var missingCount = 0;
        var missingList = new List<string>();

        void Check(bool condition, string message)
        {
            if (condition)
            {
                missingCount++;
                missingList.Add(message);
            }
        }

        // ─── 1. بيانات المتقاعد المطلوبة ────────────────────────────────
        var retiree = file.Retiree;
        if (retiree != null)
        {
            Check(string.IsNullOrWhiteSpace(retiree.FullName), "المتقاعد: الاسم الكامل");
            Check(string.IsNullOrWhiteSpace(retiree.MilitaryNumber), "المتقاعد: الرقم العسكري / المالي");
            Check(retiree.Gender == 0, "المتقاعد: الجنس");
            Check(retiree.SocialStatusID == null, "المتقاعد: الحالة الاجتماعية");
            Check(retiree.HealthStatusID == null, "المتقاعد: الحالة الصحية");
            Check(!retiree.IsAlive && (!retiree.DeathDate.HasValue || retiree.DeathDate.Value == default), "المتقاعد: تاريخ الوفاة");

            // ── البيانات التالية إلزامية فقط للأحياء ──
            if (retiree.IsAlive)
            {
                Check(string.IsNullOrWhiteSpace(retiree.NationalID), "المتقاعد: رقم الهوية");
                Check(!retiree.BirthDate.HasValue || retiree.BirthDate.Value == default, "المتقاعد: تاريخ الميلاد");
                Check(string.IsNullOrWhiteSpace(retiree.Phone), "المتقاعد: رقم الهاتف");
                Check(string.IsNullOrWhiteSpace(retiree.Address), "المتقاعد: العنوان");
                Check(string.IsNullOrWhiteSpace(retiree.City), "المتقاعد: المدينة");
                Check(string.IsNullOrWhiteSpace(retiree.Governorate), "المتقاعد: المحافظة");
                Check(string.IsNullOrWhiteSpace(retiree.PersonalPhotoPath), "المتقاعد: الصورة الشخصية (بيانات)");
            }
        }

        // ─── 2. بيانات المستحقين (Entitled Persons) ──────────────────────
        if (file.EntitledPersons != null)
        {
            foreach (var entitled in file.EntitledPersons)
            {
                string prefix = $"المستحق ({entitled.FullName})";
                Check(string.IsNullOrWhiteSpace(entitled.FullName), $"{prefix}: الاسم الكامل");

                bool isAdult = true;
                if (entitled.BirthDate != default)
                {
                    var age = PensionMS.Core.Time.YemenTime.Age(entitled.BirthDate);
                    if (age < 18) isAdult = false;
                }
                if (isAdult)
                {
                    Check(string.IsNullOrWhiteSpace(entitled.NationalID), $"{prefix}: رقم الهوية");
                }

                Check(entitled.BirthDate == default, $"{prefix}: تاريخ الميلاد");
                Check(string.IsNullOrWhiteSpace(entitled.Phone), $"{prefix}: رقم الهاتف");
                Check(entitled.SocialStatusID == null, $"{prefix}: الحالة الاجتماعية");
                Check(entitled.HealthStatusID == null, $"{prefix}: الحالة الصحية");
                Check(entitled.IsDeceased && entitled.DeceasedDate == null, $"{prefix}: تاريخ الوفاة");
            }
        }

        // ─── 3. بيانات الوكلاء (Agents) ──────────────────────────────────
        if (file.Agents != null)
        {
            foreach (var agent in file.Agents)
            {
                string prefix = $"الوكيل ({agent.FullName})";
                Check(string.IsNullOrWhiteSpace(agent.FullName), $"{prefix}: الاسم الكامل");
                Check(string.IsNullOrWhiteSpace(agent.NationalID), $"{prefix}: رقم الهوية");
                Check(string.IsNullOrWhiteSpace(agent.Phone), $"{prefix}: رقم الهاتف");
                if (!string.IsNullOrWhiteSpace(agent.AgencyNumber))
                {
                    Check(agent.AgencyDate == null || agent.AgencyDate.Value == default, $"{prefix}: تاريخ الوكالة");
                }
            }
        }

        // ─── 4. بيانات الأوصياء (Guardians) ──────────────────────────────
        if (file.Guardians != null)
        {
            foreach (var guardian in file.Guardians)
            {
                string prefix = $"الوصي ({guardian.FullName})";
                Check(string.IsNullOrWhiteSpace(guardian.FullName), $"{prefix}: الاسم الكامل");
                Check(string.IsNullOrWhiteSpace(guardian.NationalID), $"{prefix}: رقم الهوية");
                Check(string.IsNullOrWhiteSpace(guardian.Phone), $"{prefix}: رقم الهاتف");
                Check(string.IsNullOrWhiteSpace(guardian.GuardianshipDocNumber), $"{prefix}: رقم صك الوصاية");
                Check(guardian.StartDate == default, $"{prefix}: تاريخ بداية الوصاية");
            }
        }

        return (missingCount, missingList);
    }

    /// <summary>
    /// جلب أنواع الوثائق المطلوبة (عامة + سياقية) للملف
    /// وثيقة الوكالة (ID=3) مطلوبة عند وجود وكيل نشط
    /// وثيقة التعهد (ID=9) مطلوبة عند وجود تعهد نشط
    /// </summary>
    // أنواع الوثائق المشروطة — تُعالج سياقياً وليس عالمياً
    private async Task<List<int>> GetContextualRequiredDocTypeIdsAsync(int fileId)
    {
        // Global required — باستثناء الأنواع المشروطة (الوكالة=3، التعهد=9، إيقاف المعاش=11)
        var required = await _unitOfWork.DocumentTypes.Query()
            .AsNoTracking()
            .Where(dt => dt.IsRequired && dt.IsActive && dt.DocumentTypeID != 3 && dt.DocumentTypeID != 9 && dt.DocumentTypeID != 11)
            .Select(dt => dt.DocumentTypeID)
            .ToListAsync();

        // Contextual: وثيقة الوكالة (ID=3) — مطلوبة فقط عند وجود وكيل نشط
        var hasActiveAgent = await _unitOfWork.Agents.Query()
            .AsNoTracking()
            .AnyAsync(a => a.FileID == fileId && a.IsActive && !a.IsDeleted);
        if (hasActiveAgent)
            required.Add(3);

        // Contextual: وثيقة التعهد (ID=9) — مطلوبة فقط عند وجود تعهد نشط غير مستوفى
        var hasActivePledge = await _unitOfWork.DocumentPledges.Query()
            .AsNoTracking()
            .AnyAsync(p => p.FileID == fileId && !p.IsDeleted && p.Status != MissingDocumentStatusConstants.Fulfilled);
        if (hasActivePledge)
            required.Add(9);

        // Contextual: أمر إيقاف المعاش (ID=11) — مطلوب فقط إذا كان الملف موقوفاً أو به إيقاف فعّال
        var fileStatus = await _unitOfWork.RetirementFiles.Query()
            .AsNoTracking()
            .Where(f => f.FileID == fileId)
            .Select(f => f.StatusID)
            .FirstOrDefaultAsync();

        var hasSuspension = await _unitOfWork.PensionSuspensions.Query()
            .AsNoTracking()
            .AnyAsync(s => s.FileID == fileId && s.Status == SuspensionStatusConstants.Active && !s.IsDeleted);

        if (fileStatus == FileStatusConstants.Suspended || hasSuspension)
            required.Add(11);

        return required;
    }

    public async Task<ApiResponse<List<StatusTransitionDto>>> GetStatusTransitionsAsync(int fileId)
    {
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
        var documentIds = await _unitOfWork.Documents.Query().AsNoTracking()
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
        var retireeId = await _unitOfWork.RetirementFiles.Query().AsNoTracking()
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
        // Get files with any status except "Active" (نشط)
        var query = _unitOfWork.RetirementFiles.Query()
            .AsNoTracking()
            .Where(f => !f.IsDeleted && f.StatusID != FileStatusConstants.Active);

        // ── فلترة حسب نطاق فرع المستخدم ──
        var branchId = _branchScopeProvider.GetCurrentBranchId();
        if (branchId.HasValue && !_branchScopeProvider.IsHQ())
        {
            query = query.Where(f => f.PensionBranchID == branchId.Value);
        }

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
        var query = _unitOfWork.RetirementFiles.Query()
            .AsNoTracking()
            .Where(f => !f.IsDeleted && f.StatusID != FileStatusConstants.Closed);

        // ── فلترة حسب نطاق فرع المستخدم ──
        var branchId = _branchScopeProvider.GetCurrentBranchId();
        if (branchId.HasValue && !_branchScopeProvider.IsHQ())
        {
            query = query.Where(f => f.PensionBranchID == branchId.Value);
        }

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
                IsIncomplete = file.StatusID == FileStatusConstants.Incomplete || allMissingCount > 0
            });
        }

        return ApiResponse<List<RetirementFileDocumentUploadDto>>.Success(result);
    }

    public async Task<ApiResponse<DocumentUploadStatsDto>> GetDocumentUploadStatsAsync()
    {
        var requiredDocTypeIds = await _unitOfWork.DocumentTypes.Query()
            .AsNoTracking()
            .Where(dt => dt.IsActive && dt.IsRequired)
            .Select(dt => dt.DocumentTypeID)
            .ToListAsync();

        var query = _unitOfWork.RetirementFiles.Query()
            .AsNoTracking()
            .Where(f => !f.IsDeleted && f.StatusID != FileStatusConstants.Closed);

        var branchId = _branchScopeProvider.GetCurrentBranchId();
        if (branchId.HasValue && !_branchScopeProvider.IsHQ())
        {
            query = query.Where(f => f.PensionBranchID == branchId.Value);
        }

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
        var query = _unitOfWork.Documents.Query()
            .AsNoTracking()
            .Where(d => !d.IsDeleted && d.FileID.HasValue);

        // Apply date filters
        if (filter.Period is "today" or "week" or "month" or "year")
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
        // مصدر البيانات: جدول ملفات التقاعد — مَن أنشأ الملف
        var query = _unitOfWork.RetirementFiles.Query()
            .AsNoTracking()
            .Where(f => !f.IsDeleted);

        // فلترة حسب الفترة
        if (filter.Period is "today" or "week" or "month" or "year")
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
            .Select(f => new { f.FileID, f.StatusID, CreatedByUserID = (int?)f.CreatedByUserID })
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
                    f.StatusID == FileStatusConstants.Closed),
                PendingFiles = g.Count(f =>
                    f.StatusID == FileStatusConstants.New ||
                    f.StatusID == FileStatusConstants.UnderReview ||
                    f.StatusID == FileStatusConstants.Pending),
                RejectedFiles = g.Count(f =>
                    f.StatusID == FileStatusConstants.Rejected),
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
        // جلب الملفات التي أنشأها المستخدم
        var filesQuery = _unitOfWork.RetirementFiles.Query()
            .AsNoTracking()
            .Where(f => !f.IsDeleted && f.CreatedByUserID == userId);

        if (fromDate.HasValue)
            filesQuery = filesQuery.Where(f => f.CreatedAt >= fromDate.Value);
        if (toDate.HasValue)
            filesQuery = filesQuery.Where(f => f.CreatedAt <= toDate.Value.AddDays(1).AddTicks(-1));

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

    private async Task<string> SaveFileAsync(IRemoteFile file, string folder)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("الملف فارغ أو غير موجود");

        using var stream = file.OpenReadStream();
        return await _fileStorage.SaveFileAsync(stream, file.FileName, folder);
    }

    private async Task<MemoryStream> ToMemoryStreamAsync(Stream stream)
    {
        if (stream is MemoryStream ms) return ms;
        var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;
        return memoryStream;
    }

    // ═══════════════════════════════════════════════════════
    // REPORTS
    // ═══════════════════════════════════════════════════════

    public async Task<ApiResponse<PagedResult<RetirementFileReportRow>>> GetRetirementFileReportAsync(RetirementFileReportFilter filter)
    {
        var query = BuildReportQuery();
        query = ApplyReportFilters(query, filter);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(x => x.FileID)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var result = new PagedResult<RetirementFileReportRow>
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.PageNumber,
            PageSize = filter.PageSize
        };

        return ApiResponse<PagedResult<RetirementFileReportRow>>.Success(result);
    }

    public async Task<ApiResponse<RetirementFileReportSummaryDto>> GetRetirementFileReportSummaryAsync(RetirementFileReportFilter filter)
    {
        var query = BuildReportQuery();
        query = ApplyReportFilters(query, filter);

        var allItems = await query.ToListAsync();

        var summary = new RetirementFileReportSummaryDto
        {
            TotalCount = allItems.Count,
            ActiveCount = allItems.Count(x => x.StatusName != null && x.StatusName.Contains("نشط")),
            SuspendedCount = allItems.Count(x => x.StatusName != null && x.StatusName.Contains("معلق")),
            ClosedCount = allItems.Count(x => x.StatusName != null && x.StatusName.Contains("مغلق")),
            IncompleteCount = allItems.Count(x => x.StatusName != null && x.StatusName.Contains("ناقص")),
            PendingReviewCount = allItems.Count(x => x.ReviewStatusName != null && x.ReviewStatusName.Contains("بانتظار")),
            AliveCount = allItems.Count(x => x.IsAlive),
            DeceasedCount = allItems.Count(x => !x.IsAlive),
            ByRank = allItems
                .Where(x => !string.IsNullOrEmpty(x.RankName))
                .GroupBy(x => x.RankName)
                .Select(g => new NameValuePairDto { Name = g.Key!, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList(),
            ByBranch = allItems
                .Where(x => !string.IsNullOrEmpty(x.BranchName))
                .GroupBy(x => x.BranchName)
                .Select(g => new NameValuePairDto { Name = g.Key!, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList(),
            ByGovernorate = allItems
                .Where(x => !string.IsNullOrEmpty(x.GovernorateName))
                .GroupBy(x => x.GovernorateName)
                .Select(g => new NameValuePairDto { Name = g.Key!, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList(),
            ByGender = allItems
                .GroupBy(x => x.Gender)
                .Select(g => new NameValuePairDto
                {
                    Name = g.Key == 2 ? "أنثى" : "ذكر",
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToList(),
            ByStatus = allItems
                .Where(x => !string.IsNullOrEmpty(x.StatusName))
                .GroupBy(x => x.StatusName)
                .Select(g => new NameValuePairDto { Name = g.Key!, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList(),
            BySocialStatus = allItems
                .Where(x => !string.IsNullOrEmpty(x.SocialStatusName))
                .GroupBy(x => x.SocialStatusName)
                .Select(g => new NameValuePairDto { Name = g.Key!, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList()
        };

        return ApiResponse<RetirementFileReportSummaryDto>.Success(summary);
    }

    public async Task<byte[]> ExportRetirementFileReportAsync(RetirementFileReportFilter filter, string format)
    {
        var query = BuildReportQuery();
        query = ApplyReportFilters(query, filter);

        var items = await query.OrderByDescending(x => x.FileID).ToListAsync();

        if (format.ToLower() == ExportFormatConstants.Excel)
        {
            using var package = new OfficeOpenXml.ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("تقرير ملفات التقاعد");
            ws.View.RightToLeft = true;

            var headers = new[] { "م", "رقم الملف", "المتقاعد", "الرقم العسكري", "الرتبة", "الفرع", "الجنس", "الحالة الاجتماعية", "تاريخ التقاعد", "حالة الملفات", "مراجع", "مستندات ناقصة" };
            for (int i = 0; i < headers.Length; i++) ws.Cells[1, i + 1].Value = headers[i];

            using (var range = ws.Cells[1, 1, 1, headers.Length])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(31, 73, 125));
                range.Style.Font.Color.SetColor(System.Drawing.Color.White);
                range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            }

            for (int i = 0; i < items.Count; i++)
            {
                var it = items[i];
                ws.Cells[i + 2, 1].Value = i + 1;
                ws.Cells[i + 2, 2].Value = it.FileNumber;
                ws.Cells[i + 2, 3].Value = it.RetireeName;
                ws.Cells[i + 2, 4].Value = it.MilitaryNumber;
                ws.Cells[i + 2, 5].Value = it.RankName;
                ws.Cells[i + 2, 6].Value = it.BranchName;
                ws.Cells[i + 2, 7].Value = it.Gender == 2 ? "أنثى" : "ذكر";
                ws.Cells[i + 2, 8].Value = it.SocialStatusName;
                ws.Cells[i + 2, 9].Value = it.RetirementDate?.ToString("yyyy/MM/dd");
                ws.Cells[i + 2, 10].Value = it.StatusName;
                ws.Cells[i + 2, 11].Value = it.ReviewStatusName;
                ws.Cells[i + 2, 12].Value = it.MissingDocumentsCount;
            }

            ws.Cells.AutoFitColumns();
            return package.GetAsByteArray();
        }
        else if (format.ToLower() == ExportFormatConstants.Pdf)
        {
            QuestPDF.Settings.DocumentLayoutExceptionThreshold = 1000;
            return QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1, QuestPDF.Infrastructure.Unit.Centimetre);
                    page.PageColor(QuestPDF.Helpers.Colors.White);
                    page.ContentFromRightToLeft();
                    page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Arial"));

                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("الجمهورية اليمنية").Bold();
                            col.Item().Text("وزارة الداخلية");
                            col.Item().Text("صندوق التقاعد الأمني");
                        });
                        row.ConstantItem(120).AlignCenter().Text("تقرير ملفات التقاعد").FontSize(14).Bold();
                        row.RelativeItem().AlignLeft().Column(col =>
                        {
                            col.Item().Text($"التاريخ: {PensionMS.Core.Time.YemenTime.Now:yyyy/MM/dd}");
                        });
                    });

                    page.Content().PaddingVertical(10).Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.ConstantColumn(25);
                            cols.ConstantColumn(60);
                            cols.RelativeColumn(3);
                            cols.ConstantColumn(60);
                            cols.ConstantColumn(60);
                            cols.ConstantColumn(50);
                            cols.ConstantColumn(70);
                            cols.ConstantColumn(50);
                        });

                        table.Header(h =>
                        {
                            h.Cell().Element(CellStyle).Text("م");
                            h.Cell().Element(CellStyle).Text("رقم الملف");
                            h.Cell().Element(CellStyle).Text("المتقاعد");
                            h.Cell().Element(CellStyle).Text("الرقم العسكري");
                            h.Cell().Element(CellStyle).Text("الرتبة");
                            h.Cell().Element(CellStyle).Text("الجنس");
                            h.Cell().Element(CellStyle).Text("تاريخ التقاعد");
                            h.Cell().Element(CellStyle).Text("الحالة");

                            static QuestPDF.Infrastructure.IContainer CellStyle(QuestPDF.Infrastructure.IContainer c) => c.DefaultTextStyle(x => x.Bold()).PaddingVertical(5).BorderBottom(1).AlignCenter();
                        });

                        int rowNumber = 1;
                        foreach (var it in items)
                        {
                            table.Cell().Element(ContentStyle).Text(rowNumber.ToString());
                            table.Cell().Element(ContentStyle).Text(it.FileNumber ?? "");
                            table.Cell().Element(ContentStyle).Text(it.RetireeName ?? "");
                            table.Cell().Element(ContentStyle).Text(it.MilitaryNumber ?? "");
                            table.Cell().Element(ContentStyle).Text(it.RankName ?? "");
                            table.Cell().Element(ContentStyle).Text(it.Gender == 2 ? "أنثى" : "ذكر");
                            table.Cell().Element(ContentStyle).Text(it.RetirementDate?.ToString("yyyy/MM/dd") ?? "---");
                            table.Cell().Element(ContentStyle).Text(it.StatusName ?? "");

                            static QuestPDF.Infrastructure.IContainer ContentStyle(QuestPDF.Infrastructure.IContainer c) => c.BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2).PaddingVertical(5).AlignCenter();
                            rowNumber++;
                        }
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("صفحة ");
                        x.CurrentPageNumber();
                        x.Span(" من ");
                        x.TotalPages();
                    });
                });
            }).GeneratePdf();
        }

        return Array.Empty<byte>();
    }

    private IQueryable<RetirementFileReportRow> BuildReportQuery()
    {
        return from rf in _unitOfWork.RetirementFiles.Query()
                    .AsNoTracking()
                    .Include(f => f.Rank)
                    .Include(f => f.Status)
                    .Include(f => f.PensionBranch)
                join r in _unitOfWork.Retirees.Query() on rf.RetireeID equals r.RetireeID
                join ss in _unitOfWork.SocialStatuses.Query() on r.SocialStatusID equals ss.SocialStatusID into ssJoin
                from ss in ssJoin.DefaultIfEmpty()
                join hs in _unitOfWork.HealthStatuses.Query() on r.HealthStatusID equals hs.HealthStatusID into hsJoin
                from hs in hsJoin.DefaultIfEmpty()
                where !rf.IsDeleted
                select new RetirementFileReportRow
                {
                    FileID = rf.FileID,
                    FileNumber = rf.FileNumber,
                    RetireeName = r.FullName,
                    MilitaryNumber = r.MilitaryNumber,
                    NationalID = r.NationalID,
                    RankID = rf.RankID,
                    RankName = rf.Rank != null ? rf.Rank.RankName : null,
                    PensionBranchID = rf.PensionBranchID,
                    BranchName = rf.PensionBranch != null ? rf.PensionBranch.BranchName : null,
                    GovernorateName = r.Governorate,
                    StatusID = rf.StatusID,
                    StatusName = rf.Status != null ? rf.Status.StatusName : null,
                    Gender = r.Gender,
                    SocialStatusID = r.SocialStatusID,
                    SocialStatusName = ss != null ? ss.StatusName : null,
                    HealthStatusName = hs != null ? hs.HealthStatusName : null,
                    RetirementDate = rf.RetirementDate,
                    FileOpenDate = rf.FileOpenDate,
                    MissingDocumentsCount = rf.MissingDocumentsCount,
                    ReviewStatusName = rf.ReviewStatus == Core.Entities.FileReviewStatus.PendingReview ? "بانتظار المراجعة" :
                                       rf.ReviewStatus == Core.Entities.FileReviewStatus.Approved ? "تمت المراجعة" :
                                       rf.ReviewStatus == Core.Entities.FileReviewStatus.Rejected ? "مرفوض" : "---",
                    IsAlive = r.IsAlive
                };
    }

    private static IQueryable<RetirementFileReportRow> ApplyReportFilters(IQueryable<RetirementFileReportRow> query, RetirementFileReportFilter filter)
    {
        if (!string.IsNullOrEmpty(filter.SearchTerm))
            query = query.Where(x => x.RetireeName!.Contains(filter.SearchTerm) || x.MilitaryNumber!.Contains(filter.SearchTerm) || x.FileNumber!.Contains(filter.SearchTerm));

        if (filter.StatusId.HasValue)
            query = query.Where(x => x.StatusID == filter.StatusId.Value);

        if (filter.RankId.HasValue)
            query = query.Where(x => x.RankID == filter.RankId.Value);

        if (filter.Gender.HasValue)
            query = query.Where(x => x.Gender == filter.Gender.Value);

        if (filter.BranchId.HasValue)
            query = query.Where(x => x.PensionBranchID == filter.BranchId.Value);

        if (!string.IsNullOrEmpty(filter.Governorate))
            query = query.Where(x => x.GovernorateName == filter.Governorate);

        if (filter.ReviewStatus.HasValue)
        {
            if (filter.ReviewStatus.Value == 1)
                query = query.Where(x => x.ReviewStatusName != null && x.ReviewStatusName.Contains("بانتظار"));
            else if (filter.ReviewStatus.Value == 2)
                query = query.Where(x => x.ReviewStatusName != null && x.ReviewStatusName.Contains("تمت"));
            else if (filter.ReviewStatus.Value == 3)
                query = query.Where(x => x.ReviewStatusName != null && x.ReviewStatusName.Contains("مرفوض"));
        }

        if (filter.SocialStatusId.HasValue)
            query = query.Where(x => x.SocialStatusID == filter.SocialStatusId.Value);

        return query;
    }
}
