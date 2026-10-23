using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Documents;
using PensionMS.Core.DTOs.EntitledPersons;
using PensionMS.Core.DTOs.Notifications;
using PensionMS.Core.DTOs.RetirementFiles;
using PensionMS.Core.DTOs.Retirees;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Helpers;
using PensionMS.Infrastructure.Helpers;

using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace PensionMS.Infrastructure.Services.Pension.RetirementFile;

public class RetirementFileService : IRetirementFileService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IImmutableAuditService _auditService;
    private readonly IDocumentService _documentService;
    private readonly IMissingDocumentService _missingDocumentService;
    private readonly IAccessiblePensionRepository _access;
    private readonly IEntitledPersonService _entitledPersonService;
    private readonly INotificationService _notificationService;
    private readonly IFileReviewService _reviewService;
    private readonly IRetireeService _retireeService;
    private readonly IFinancialEntitlementService _financialEntitlementService;
    private readonly IPensionSuspensionService _pensionSuspensionService;
    private readonly ILogger<RetirementFileService> _logger;

    private readonly RetirementFileCompletenessEvaluator _completeness;

    public RetirementFileService(
        IUnitOfWork unitOfWork, 
        IImmutableAuditService auditService,
        IMissingDocumentService missingDocumentService,
        IDocumentService documentService,
        IAccessiblePensionRepository access,
        IEntitledPersonService entitledPersonService,
        INotificationService notificationService,
        IFileReviewService reviewService,
        ILogger<RetirementFileService> logger, IRetireeService retireeService,
        IFinancialEntitlementService financialEntitlementService, IPensionSuspensionService pensionSuspensionService)
    {
        _unitOfWork = unitOfWork;
        _completeness = new RetirementFileCompletenessEvaluator(unitOfWork, missingDocumentService);
        _auditService = auditService;
        _documentService = documentService;
        _missingDocumentService = missingDocumentService;
        _access = access;
        _entitledPersonService = entitledPersonService;
        _notificationService = notificationService;
        _reviewService = reviewService;
        _logger = logger;
        _retireeService = retireeService;
        _financialEntitlementService = financialEntitlementService;
        _pensionSuspensionService = pensionSuspensionService;
    }

    public async Task<ApiResponse<RetirementFileDto>> CreateAsync(CreateRetirementFileDto dto, int userId)
    {
        var queries = await _access.ForPermissionAsync("File.Create");
        // ── Phase 1.2: منع إنشاء ملف مكرر لنفس المتقاعد ──
        var existingActive = await _unitOfWork.RetirementFiles.Query()
            .AnyAsync(f => f.RetireeID == dto.RetireeID
                        );
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

        var branchId = dto.BranchID ?? user.BranchID ??
            (queries.Scope.BranchIds.Count == 1 ? queries.Scope.BranchIds[0] : 0);
        if (!queries.Scope.Allows(branchId))
            return ApiResponse<RetirementFileDto>.Fail("اختر فرعاً ضمن نطاق صلاحية إنشاء الملف");
        if (!await _unitOfWork.PensionFundBranches.Query().AnyAsync(b => b.PensionBranchID == branchId && b.IsActive))
            return ApiResponse<RetirementFileDto>.Fail("الفرع غير موجود أو غير فعّال");


        // بيانات المتقاعد اللازمة لفتح الملف؛ المصدر المالي والرتبة تحددهما الخدمة المالية.
        var retiree = await _unitOfWork.Retirees.Query()
            .IgnoreQueryFilters() // Bypass MinistryApprovalItem IsDeleted filter cascade
            .Where(r => !r.IsDeleted) // Ensure the Retiree itself is not soft-deleted
            .Include(r => r.ApprovalItem)
            .FirstOrDefaultAsync(r => r.RetireeID == dto.RetireeID);

        if (retiree == null)
            return ApiResponse<RetirementFileDto>.Fail("المتقاعد غير موجود");

        // ── بدء المعاملة لضمان ذرَية العملية ──
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            // احجز المصدر المالي قبل أي حفظ حتى تتبع عمليات الفتح والإنشاء المالي ترتيب الأقفال نفسه.
            await _unitOfWork.LockRetireeEntitlementsAsync(retiree.RetireeID);
            var staged = await _retireeService.StageFileOpeningAsync(retiree.RetireeID, new UpdateRetireeDto
            {
                NationalID = dto.NationalID, Phone = dto.Phone, Address = dto.Address,
                City = dto.City, Governorate = dto.Governorate, Gender = dto.Gender,
                SocialStatusID = dto.SocialStatusID, HealthStatusID = dto.HealthStatusID,
                BirthDate = dto.BirthDate, PersonalPhoto = dto.PersonalPhoto
            }, dto.IsAlive, dto.DeathDate, userId);
            if (!staged.IsSuccess)
            {
                await _unitOfWork.RollbackAsync();
                return ApiResponse<RetirementFileDto>.Fail(staged.Message!);
            }


            var fileNumber = await GenerateNextFileNumberAsync();

            var file = new PensionMS.Core.Entities.RetirementFile
            {
                RetireeID = dto.RetireeID,
                FileNumber = fileNumber,
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

            var financial = await _financialEntitlementService.CreateForFileAsync(file.FileID, userId);
            if (!financial.IsSuccess || financial.Data?.RankID is null or <= 0)
            {
                await _unitOfWork.RollbackAsync();
                return ApiResponse<RetirementFileDto>.Fail(financial.IsSuccess
                    ? "لم تحدد الخدمة المالية رتبة مصدر المعاش؛ تعذر فتح الملف" : financial.Message!);
            }
            file.RankID = financial.Data.RankID;
            await _unitOfWork.SaveChangesAsync();

            var uploadedDocumentIds = new List<int>();
            bool idUploadFailed = false;
            // Upload ID Document (if provided) - Unified Front+Back
            if (dto.IdDocument != null || dto.IdDocumentBack != null)
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
                idUploadFailed = !await UploadInitialDocumentAsync(uploadDto, userId, uploadedDocumentIds);
            }

            // Upload Death Certificate (if Dead and provided)
            bool deathCertUploadFailed = false;
            if (!dto.IsAlive && dto.DeathCertificate != null)
            {
                deathCertUploadFailed = !await UploadInitialDocumentAsync(new UploadDocumentDto
                {
                    RetirementFileID = file.FileID, RetireeID = file.RetireeID,
                    DocumentTypeID = DocumentTypeConstants.DeathCertificate,
                    File = dto.DeathCertificate, Notes = "شهادة الوفاة"
                }, userId, uploadedDocumentIds);
            }

            // Upload Primary Document (أوليات الملف) (if provided)
            bool primaryDocUploadFailed = false;
            if (dto.PrimaryDocument != null)
            {
                primaryDocUploadFailed = !await UploadInitialDocumentAsync(new UploadDocumentDto
                {
                    RetirementFileID = file.FileID,
                    DocumentTypeID = DocumentTypeConstants.FileOriginals,
                    File = dto.PrimaryDocument, Notes = "أوليات الملف (تم رفعها عند فتح الملف)"
                }, userId, uploadedDocumentIds);
            }

            await _missingDocumentService.StageRequiredAsync(file.FileID, userId);
            await _unitOfWork.SaveChangesAsync();

            if (uploadedDocumentIds.Count > 0)
            {
                var refreshed = await _documentService.RefreshAfterUploadAsync(file.FileID, uploadedDocumentIds, userId);
                if (!refreshed.IsSuccess)
                    throw new InvalidOperationException(refreshed.Message ?? "تعذر تحديث الملف بعد رفع الوثائق");
            }
            await _retireeService.RefreshProfileAsync(retiree.RetireeID);
            var salaryStatus = await _pensionSuspensionService.GetSalaryStatusAsync(file.FileID);
            if (!salaryStatus.IsSuccess) throw new InvalidOperationException(salaryStatus.Message);
            var identity = await _documentService.GetRetireeIdentityPathAsync(file.FileID);
            if (!identity.IsSuccess) throw new InvalidOperationException(identity.Message);
            await _unitOfWork.CommitAsync();

            string warning = "";
            if (idUploadFailed || deathCertUploadFailed || primaryDocUploadFailed)
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

            return ApiResponse<RetirementFileDto>.Success(RetirementFileDtoMapper.MapToDto(newFile ?? file, retiree, new RetirementFileDto
            {
                FileStatusName = FileStatusConstants.Name((newFile ?? file).StatusID ?? 0),
                SalaryStatus = salaryStatus.Data!, IdDocumentPath = identity.Data,
                MissingDataCount = missingData.Count, MissingDataList = missingData.Items
            }), "تم إنشاء الملف بنجاح" + warning);
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
        var queries = await _access.ForPermissionAsync("File.ChangeStatus.Approve");
        var file = await queries.Files()
            .Include(f => f.Retiree)
            .FirstOrDefaultAsync(f => f.FileID == fileId);

        if (file == null)
            return ApiResponse<bool>.Fail("الملف غير موجود");

        if (file.Retiree == null)
            return ApiResponse<bool>.Fail("بيانات المتقاعد غير مرتبطة بهذا الملف");

        if (!await queries.CanEditRetireeAsync(file.RetireeID)) throw new PensionMS.Core.Exceptions.BranchScopeAccessException();

        if (!file.Retiree.IsAlive)
            return ApiResponse<bool>.Fail("المتقاعد مُسجَّل متوفى مسبقاً");

        if (deathDate == default || deathDate > PensionMS.Core.Time.YemenTime.Today ||
            (file.Retiree.BirthDate.HasValue && deathDate < file.Retiree.BirthDate.Value))
            return ApiResponse<bool>.Fail("تاريخ الوفاة غير صحيح");

        await _unitOfWork.BeginTransactionAsync();
        try
        {

        // ═════ 1. تسجيل الوفاة ═════
        file.Retiree.IsAlive = false;
        file.Retiree.DeathDate = deathDate;
        file.Retiree.HealthStatusID = HealthStatusConstants.Deceased;
        file.Retiree.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Retirees.Update(file.Retiree);

        // إعداد التغيير للمراجعة، مع بقاء المسودة الجديدة في مرحلة التجهيز.
        var oldFileStatus = file.StatusID;
        if (file.StatusID != FileStatusConstants.New) file.StatusID = FileStatusConstants.Pending;
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
            NewStatusID = file.StatusID ?? FileStatusConstants.New,
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

        // احفظ حالة الحياة أولاً حتى يقرأ كشف المتطلبات الحالة الجديدة داخل المعاملة نفسها.
        await _unitOfWork.SaveChangesAsync();
        await _missingDocumentService.StageRequiredAsync(fileId, userId);
        await _unitOfWork.SaveChangesAsync();

        await _missingDocumentService.RefreshComplianceCacheAsync(fileId);

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
        var queries = await _access.ForPermissionAsync("File.ChangeStatus.Approve");
        if (string.IsNullOrWhiteSpace(dto.Reason) || dto.Reason.Trim().Length < 10)
            return ApiResponse<bool>.Fail("سبب التصحيح يجب ألا يقل عن 10 أحرف");
        if (dto.HealthStatusID is < HealthStatusConstants.Healthy or > HealthStatusConstants.FullyDisabled)
            return ApiResponse<bool>.Fail("الحالة الصحية المختارة غير صحيحة");

        var file = await queries.Files()
            .Include(f => f.Retiree)
            .FirstOrDefaultAsync(f => f.FileID == fileId);
        if (file?.Retiree == null)
            return ApiResponse<bool>.Fail("الملف أو بيانات المتقاعد غير موجودة");
        if (!await queries.CanEditRetireeAsync(file.RetireeID)) throw new PensionMS.Core.Exceptions.BranchScopeAccessException();
        if (file.Retiree.IsAlive && file.Retiree.DeathDate == null &&
            file.Retiree.HealthStatusID is not (HealthStatusConstants.Deceased or HealthStatusConstants.Martyr))
            return ApiResponse<bool>.Fail("بيانات حالة حياة المتقاعد سليمة بالفعل");
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

            if (oldFileStatus != FileStatusConstants.New)
            {
                file.StatusID = FileStatusConstants.Pending;
                file.PreviousStatusID = oldFileStatus;
                file.ReviewStatus = FileReviewStatus.PendingReview;
                file.ReviewRequestedAt = now;
            }
            file.StatusReason = "تصحيح حالة حياة المتقاعد — الصرف موقوف حتى المراجعة والاعتماد: " + dto.Reason.Trim();
            file.UpdatedAt = now;
            file.UpdatedByUserID = userId;
            _unitOfWork.RetirementFiles.Update(file);

            if (oldFileStatus != file.StatusID)
                await _unitOfWork.StatusTransitionLogs.AddAsync(new StatusTransitionLog
                {
                    TableID = SystemTableConstants.RetirementFiles,
                    RecordID = fileId,
                    OldStatusID = oldFileStatus,
                    NewStatusID = FileStatusConstants.Pending,
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
            await _missingDocumentService.RefreshComplianceCacheAsync(fileId);
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

    public Task<ApiResponse<RetirementFileDto>> UpdateAsync(int id, UpdateRetirementFileDto dto, int userId) =>
        PensionMS.Infrastructure.Data.UnitOfWorkTransaction.ExecuteAsync(_unitOfWork, () => UpdateCoreAsync(id, dto, userId));

    private async Task<ApiResponse<RetirementFileDto>> UpdateCoreAsync(int id, UpdateRetirementFileDto dto, int userId)
    {
        var queries = await _access.ForPermissionAsync("File.Edit.BasicData");
        var file = await queries.Files()
            .Include(f => f.Retiree)
            .FirstOrDefaultAsync(f => f.FileID == id);
        
        if (file == null)
            return ApiResponse<RetirementFileDto>.Fail("الملف غير موجود");
        if (file.Retiree == null)
            return ApiResponse<RetirementFileDto>.Fail("بيانات المتقاعد غير مرتبطة بهذا الملف");

        if (!await queries.CanEditRetireeAsync(file.RetireeID))
            throw new PensionMS.Core.Exceptions.BranchScopeAccessException();

        // ── Phase 2.3: فحص التزامن (Optimistic Concurrency) ──
        if (dto.RowVersion != null && file.RowVersion != null
            && !dto.RowVersion.SequenceEqual(file.RowVersion))
            return ApiResponse<RetirementFileDto>.Fail(
                "تم تعديل الملف من قبل مستخدم آخر. يرجى تحديث الصفحة والمحاولة مرة أخرى");

        if (file.Retiree != null &&
            ((dto.IsAlive.HasValue && dto.IsAlive.Value != file.Retiree.IsAlive) ||
             (dto.DeathDate.HasValue && dto.DeathDate != file.Retiree.DeathDate)))
            return ApiResponse<RetirementFileDto>.Fail("تغيير حالة حياة المتقاعد يتطلب الإجراء المخصص من صفحة الملف");

        if (dto.BranchID.HasValue && dto.BranchID.Value != file.PensionBranchID)
            return ApiResponse<RetirementFileDto>.Fail("لا يمكن تغيير فرع الملف بعد إنشائه");
        if (dto.RankID.HasValue && dto.RankID != file.RankID)
            return ApiResponse<RetirementFileDto>.Fail("رتبة الملف تحددها مصادر المعاش؛ يجب تصحيح كشف الإحالة أو التسوية لتغييرها");
        if (file.Retiree != null)
        {
            var staged = await _retireeService.StageProfileUpdateAsync(file.RetireeID, new UpdateRetireeDto
            {
                FullName = dto.FullName, NationalID = dto.NationalID, Gender = dto.Gender,
                BirthDate = dto.BirthDate, MilitaryNumber = dto.MilitaryNumber,
                SocialStatusID = dto.SocialStatusID, HealthStatusID = dto.HealthStatusID,
                Phone = dto.Phone, Address = dto.Address, City = dto.City, Governorate = dto.Governorate,
                PersonalPhoto = dto.PersonalPhoto
            }, userId);
            if (!staged.IsSuccess) return ApiResponse<RetirementFileDto>.Fail(staged.Message!);
        }

        // م27: أخطاء المرفقات تُجمع وتظهر في الاستجابة بدلاً من إخفائها
        var attachmentErrors = new List<string>();

        // Update file fields
        // تغيير حالة الملف يتم عبر IFileStatusService، وليس عبر تحديث بيانات الملف.
        if (dto.RetirementDate.HasValue) file.RetirementDate = dto.RetirementDate;
        if (dto.ServiceStartDate.HasValue) file.ServiceStartDate = dto.ServiceStartDate;
        if (dto.ServiceEndDate.HasValue) file.ServiceEndDate = dto.ServiceEndDate;
        if (dto.Notes != null) file.Notes = dto.Notes;
        
        file.UpdatedAt = DateTime.UtcNow;
        file.UpdatedByUserID = userId;

        // تظل مرفقات الملف تحت تنسيق العملية؛ الحقول والصورة تُحدّث عبر خدمة المتقاعد.
        if (file.Retiree != null)
        {
            // تحديث الهوية الوطنية  
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
        var now = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        // ── Immutable Audit Log ──
        await _auditService.LogAsync(userId, "", "Update", "RetirementFile", file.FileID, null, null);

        // ── طلب مراجعة بعد التعديل ──
        var review = await _reviewService.RequestReviewAsync(file.FileID, userId, now);
        if (!review.IsSuccess) return ApiResponse<RetirementFileDto>.Fail(review.Message ?? "تعذر طلب مراجعة تعديل الملف");
        await _retireeService.RefreshProfileAsync(file.RetireeID);

        // Reload with all relationships for DTO mapping
        var updated = await queries.Files()
            .AsNoTracking()
            .Include(f => f.Retiree)
            .Include(f => f.Rank)
            .Include(f => f.PensionBranch)
            .Include(f => f.Status)
            .Include(f => f.AssignedEmployee)
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

        var salaryStatus = await _pensionSuspensionService.GetSalaryStatusAsync(id);
        if (!salaryStatus.IsSuccess) return ApiResponse<RetirementFileDto>.Fail(salaryStatus.Message!);
        var identity = await _documentService.GetRetireeIdentityPathAsync(id);
        if (!identity.IsSuccess) return ApiResponse<RetirementFileDto>.Fail(identity.Message!);
        var fileResponse = ApiResponse<RetirementFileDto>.Success(RetirementFileDtoMapper.MapToDto(updated, file.Retiree, new RetirementFileDto
        {
            FileStatusName = FileStatusConstants.Name(updated.StatusID ?? 0),
            SalaryStatus = salaryStatus.Data!, IdDocumentPath = identity.Data,
            EntitledPersonsCount = entitledCount, AgentsCount = agentsCount, GuardiansCount = guardiansCount,
            MissingDocumentsCount = missingDocsList.Count, MissingDocumentsList = missingDocsList,
            MissingDataCount = missingData.Count, MissingDataList = missingData.Items,
            ActivePaymentBookNumber = activeBook?.BookNumber, ActivePaymentBookExpiry = activeBook?.ExpiryDate,
            ActivePaymentBookStatus = activeBook?.Status
        }), fileMessage);
        if (attachmentErrors.Count > 0)
            fileResponse.Errors = attachmentErrors;
        return fileResponse;
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id, int deletedByUserId)
    {
        var queries = await _access.ForPermissionAsync("File.Delete.Soft");
        var file = await queries.Files().FirstOrDefaultAsync(f => f.FileID == id);
        if (file == null)
            return ApiResponse<bool>.Fail("الملف غير موجود");

        // ── Phase 1.4: منع حذف الملفات النشطة أو الهامة ──
        if (file.StatusID != FileStatusConstants.New || file.ReviewStatus != FileReviewStatus.None)
        {
            return ApiResponse<bool>.Fail("الحذف متاح للمسودة الجديدة فقط؛ يُحتفظ بسجل الملفات التي أُرسلت للمراجعة");
        }

        file.IsDeleted = true;
        file.DeletedAt = DateTime.UtcNow;
        file.DeletedByUserID = deletedByUserId;

        _unitOfWork.RetirementFiles.Update(file);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.Success(true, "تم حذف الملف");
    }

    private async Task<bool> UploadInitialDocumentAsync(UploadDocumentDto dto, int userId, List<int> uploadedIds)
    {
        try
        {
            var result = await _documentService.UploadAsync(dto, userId, deferPostProcessing: true);
            if (result.IsSuccess && result.Data != null)
            {
                uploadedIds.Add(result.Data.DocumentID);
                return true;
            }
            _logger.LogWarning("تعذر رفع الوثيقة {TypeId} عند فتح الملف {FileId}: {Message}",
                dto.DocumentTypeID, dto.RetirementFileID, result.Message);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "فشل رفع الوثيقة {TypeId} عند فتح الملف {FileId}", dto.DocumentTypeID, dto.RetirementFileID);
        }
        return false;
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
