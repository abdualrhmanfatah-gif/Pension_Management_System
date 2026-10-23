using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Documents;
using PensionMS.Core.DTOs.EntitledPersons;
using PensionMS.Core.DTOs.Notifications;
using PensionMS.Core.DTOs.RetirementFiles;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Helpers;
using PensionMS.Infrastructure.Helpers;

using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace PensionMS.Infrastructure.Services.Pension.Files;

public class RetirementFileService : IRetirementFileService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISoDGuard _sodGuard;
    private readonly IImmutableAuditService _auditService;
    private readonly IComplianceEngine _complianceEngine;
    private readonly IDocumentService _documentService;
    private readonly IFileStorageService _fileStorage;
    private readonly IBranchScopeProvider _branchScopeProvider;
    private readonly IEntitledPersonService _entitledPersonService;
    private readonly INotificationService _notificationService;
    private readonly IFileReviewService _reviewService;
    private readonly ILogger<RetirementFileService> _logger;

    private readonly RetirementFileCompleteness _completeness;

    public RetirementFileService(
        IUnitOfWork unitOfWork, 
        ISoDGuard sodGuard,
        IImmutableAuditService auditService,
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
        _completeness = new RetirementFileCompleteness(unitOfWork, missingDocumentService);
        _sodGuard = sodGuard;
        _auditService = auditService;
        _complianceEngine = complianceEngine;
        _documentService = documentService;
        _fileStorage = fileStorage;
        _branchScopeProvider = branchScopeProvider;
        _entitledPersonService = entitledPersonService;
        _notificationService = notificationService;
        _reviewService = reviewService;
        _logger = logger;
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
            var missingData = await _completeness.CalculateMissingDataAsync(fileId);

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

            return ApiResponse<RetirementFileDto>.Success(RetirementFileDtoMapper.MapToDto(newFile ?? file, missingDataCount: missingData.Count, missingDataList: missingData.Items), "تم إنشاء الملف بنجاح" + warning);
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
        // تغيير حالة الملف يتم عبر IFileStatusService، وليس عبر تحديث بيانات الملف.
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
        
        var missingDocsList = await _completeness.GetMissingDocsListAsync(id);

        var activeBook = await _unitOfWork.PaymentBooks.Query()
            .AsNoTracking()
            .Where(pb => pb.FileID == id && pb.Status == PaymentBookStatus.Active)
            .OrderByDescending(pb => pb.CreatedAt)
            .FirstOrDefaultAsync();

        // حساب البيانات الناقصة
        var missingData = await _completeness.CalculateMissingDataAsync(id);

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

        var fileResponse = ApiResponse<RetirementFileDto>.Success(RetirementFileDtoMapper.MapToDto(updated, entitledCount, agentsCount, guardiansCount, missingDocsList.Count, missingDocsList, missingData.Count, missingData.Items, activeBook), fileMessage);
        if (attachmentErrors.Count > 0)
            fileResponse.Errors = attachmentErrors;
        return fileResponse;
    }

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

}
