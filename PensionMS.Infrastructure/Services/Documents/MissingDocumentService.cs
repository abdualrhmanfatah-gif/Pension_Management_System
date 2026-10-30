using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.MissingDocuments;
using PensionMS.Core.DTOs.EntitledPersons;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Rules.Documents;

namespace PensionMS.Infrastructure.Services.Documents;

public class MissingDocumentService : IMissingDocumentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEntitlementEngine _entitlementEngine;
    private readonly ILogger<MissingDocumentService> _logger;
    private readonly IAccessiblePensionRepository _access;

    public MissingDocumentService(IUnitOfWork unitOfWork, IEntitlementEngine entitlementEngine, ILogger<MissingDocumentService> logger,
        IAccessiblePensionRepository access)
    {
        _unitOfWork = unitOfWork;
        _entitlementEngine = entitlementEngine;
        _logger = logger;
        _access = access;
    }

    public async Task<ComplianceSummary> GetComplianceSummaryAsync(int fileId)
    {
        var queries = await _access.ForCurrentOperationAsync();
        if (!await queries.Files().AnyAsync(f => f.FileID == fileId && !f.IsDeleted))
            throw new PensionMS.Core.Exceptions.EntityNotFoundException("الملف غير موجود ضمن نطاق الصلاحية");
        var (pairs, documents, manual) = await BuildPairsCoreAsync(fileId);
        return RetirementFileDocumentRules.Summarize(pairs, manual, documents, PensionMS.Core.Time.YemenTime.Today);
    }

    public async Task RefreshComplianceCacheAsync(int fileId)
    {
        var queries = await _access.ForCurrentOperationAsync();
        var summary = await GetComplianceSummaryAsync(fileId);
        var file = await queries.Files().FirstOrDefaultAsync(f => f.FileID == fileId && !f.IsDeleted);
        if (file == null) return;
        file.MissingDocumentsCount = summary.MissingCount;
        file.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<ApiResponse<List<MissingDocumentDto>>> GetByFileAsync(int fileId)
    {
        await EnsureFileAccessAsync(fileId);
        // إظهار فقط الوثائق المعلّقة والمتعهد بها — إخفاء المسلّمة والمعفاة
        var list = await _unitOfWork.MissingDocuments.Query()
            .AsNoTracking()
            .Where(m => m.FileID == fileId && 
                        (m.Status == MissingDocumentStatusConstants.Pending || 
                         m.Status == MissingDocumentStatusConstants.Pledged))
            // استثناء الوثائق المرتبطة بكيانات غير نشطة
            .Where(m =>
                // وثيقة الملف العامة (لا كيان مرتبط) — دائمًا تظهر
                (m.AgentID == null && m.GuardianID == null && m.EntitledID == null)
                // وثيقة وكيل: يظهر فقط إذا كان الوكيل نشطاً
                || (m.AgentID != null    && m.Agent    != null && m.Agent.IsActive    && !m.Agent.IsDeleted)
                // وثيقة وصي: يظهر فقط إذا كان الوصي نشطاً
                || (m.GuardianID != null && m.Guardian != null && m.Guardian.IsActive && !m.Guardian.IsDeleted)
                // وثيقة مستحق: يظهر فقط إذا كان المستحق غير متوفى وغير محذوف ومؤهل قانونياً
                || (m.EntitledID != null && m.EntitledPerson != null 
                    && !m.EntitledPerson.IsDeceased && !m.EntitledPerson.IsDeleted
                    && m.EntitledPerson.StopCode != EntitledStopCodes.AgeLimit 
                    && m.EntitledPerson.StopCode != EntitledStopCodes.DaughterMarried)
            )
            .OrderByDescending(m => m.CreatedAt)
            // تحسين الأداء باستخدام Projection مباشر
            .Select(m => new MissingDocumentDto
            {
                MissingDocumentID = m.MissingDocumentID,
                FileID            = m.FileID,
                DocumentTypeID    = m.DocumentTypeID,
                DocumentTypeName  = m.DocumentType.TypeName,
                Status            = m.Status,
                PledgeID          = m.PledgeID,
                ExemptionID       = m.ExemptionID,
                Notes             = m.Notes,
                CreatedAt         = m.CreatedAt,
                AgentID           = m.AgentID,
                AgentName         = m.Agent != null ? m.Agent.FullName : null,
                GuardianID        = m.GuardianID,
                GuardianName      = m.Guardian != null ? m.Guardian.FullName : null,
                EntitledID        = m.EntitledID,
                EntitledName      = m.EntitledPerson != null ? m.EntitledPerson.FullName : null
            })
            .ToListAsync();

        return ApiResponse<List<MissingDocumentDto>>.Success(list);
    }

    public async Task<ApiResponse<MissingDocumentDto>> GetByIdAsync(int id)
    {
        var queries = await _access.ForCurrentOperationAsync();
        var doc = await queries.Query(_unitOfWork.MissingDocuments)
            .AsNoTracking()
            .Include(m => m.DocumentType)
            .Include(m => m.Agent)
            .Include(m => m.Guardian)
            .Include(m => m.EntitledPerson)
            .Where(m => m.MissingDocumentID == id)
            .Select(m => new MissingDocumentDto
            {
                MissingDocumentID = m.MissingDocumentID,
                FileID            = m.FileID,
                DocumentTypeID    = m.DocumentTypeID,
                DocumentTypeName  = m.DocumentType.TypeName,
                Status            = m.Status,
                PledgeID          = m.PledgeID,
                ExemptionID       = m.ExemptionID,
                Notes             = m.Notes,
                CreatedAt         = m.CreatedAt,
                AgentID           = m.AgentID,
                AgentName         = m.Agent != null ? m.Agent.FullName : null,
                GuardianID        = m.GuardianID,
                GuardianName      = m.Guardian != null ? m.Guardian.FullName : null,
                EntitledID        = m.EntitledID,
                EntitledName      = m.EntitledPerson != null ? m.EntitledPerson.FullName : null
            })
            .FirstOrDefaultAsync();

        if (doc == null)
            return ApiResponse<MissingDocumentDto>.Fail("الوثيقة الناقصة غير موجودة");

        return ApiResponse<MissingDocumentDto>.Success(doc);
    }

    public Task<ApiResponse<MissingDocumentDto>> CreateAsync(CreateMissingDocumentDto dto, int userId) =>
        PensionMS.Infrastructure.Data.UnitOfWorkTransaction.ExecuteAsync(_unitOfWork, async () =>
        {
            await EnsureFileAccessAsync(dto.FileID);
            await _unitOfWork.LockFileReviewAsync(dto.FileID);
            return await CreateCoreAsync(dto, userId);
        });

    private async Task<ApiResponse<MissingDocumentDto>> CreateCoreAsync(CreateMissingDocumentDto dto, int userId)
    {
        // 1. التحقق من وجود الملف
        var queries = await _access.ForCurrentOperationAsync();
        var fileExists = await queries.Files()
            .AnyAsync(f => f.FileID == dto.FileID && !f.IsDeleted);
        if (!fileExists)
            return ApiResponse<MissingDocumentDto>.Fail("الملف المحدد غير موجود أو محذوف");

        if (new[] { dto.AgentID, dto.GuardianID, dto.EntitledID }.Count(id => id.HasValue) > 1)
            return ApiResponse<MissingDocumentDto>.Fail("يجب تحديد مالك واحد فقط للنقص");
        if (dto.AgentID.HasValue && !await queries.Query(_unitOfWork.Agents).AnyAsync(a => a.AgentID == dto.AgentID && a.FileID == dto.FileID && !a.IsDeleted) ||
            dto.GuardianID.HasValue && !await queries.Query(_unitOfWork.Guardians).AnyAsync(g => g.GuardianID == dto.GuardianID && g.FileID == dto.FileID && !g.IsDeleted) ||
            dto.EntitledID.HasValue && !await queries.Query(_unitOfWork.EntitledPersons).AnyAsync(e => e.EntitledID == dto.EntitledID && e.FileID == dto.FileID && !e.IsDeleted))
            return ApiResponse<MissingDocumentDto>.Fail("مالك النقص غير موجود ضمن الملف المحدد");

        // 2. التحقق من نوع الوثيقة
        var documentType = await _unitOfWork.DocumentTypes.Query()
            .FirstOrDefaultAsync(dt => dt.DocumentTypeID == dto.DocumentTypeID && dt.IsActive);
        if (documentType == null)
            return ApiResponse<MissingDocumentDto>.Fail("نوع الوثيقة المحدد غير موجود أو غير نشط");

        // 3. التحقق من عدم التكرار لنفس النوع ونفس الملف ونفس الكيان بحالة معلّقة أو متعهد بها
        var exists = await _unitOfWork.MissingDocuments.Query()
            .AnyAsync(m => m.FileID == dto.FileID 
                        && m.DocumentTypeID == dto.DocumentTypeID 
                        && (m.Status == MissingDocumentStatusConstants.Pending || m.Status == MissingDocumentStatusConstants.Pledged)
                        && m.AgentID == dto.AgentID
                        && m.GuardianID == dto.GuardianID
                        && m.EntitledID == dto.EntitledID);

        if (exists)
            return ApiResponse<MissingDocumentDto>.Fail("توجد وثيقة ناقصة من نفس النوع ونفس الكيان مطلوبة حالياً لهذا الملف");

        var entity = new MissingDocument
        {
            FileID         = dto.FileID,
            DocumentTypeID = dto.DocumentTypeID,
            Status         = MissingDocumentStatusConstants.Pending,
            Notes          = dto.Notes,
            CreatedAt      = DateTime.UtcNow,
            AgentID        = dto.AgentID,
            GuardianID     = dto.GuardianID,
            EntitledID     = dto.EntitledID
        };

        await _unitOfWork.MissingDocuments.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();
        await RefreshComplianceCacheAsync(entity.FileID);
        
        _logger.LogInformation("تم إنشاء وثيقة ناقصة جديدة بنجاح (ID: {DocId}) للملف {FileId} بواسطة المستخدم {UserId}", 
            entity.MissingDocumentID, dto.FileID, userId);

        var docDto = new MissingDocumentDto
        {
            MissingDocumentID = entity.MissingDocumentID,
            FileID            = entity.FileID,
            DocumentTypeID    = entity.DocumentTypeID,
            DocumentTypeName  = documentType.TypeName,
            Status            = entity.Status,
            Notes             = entity.Notes,
            CreatedAt         = entity.CreatedAt,
            AgentID           = entity.AgentID,
            GuardianID        = entity.GuardianID,
            EntitledID        = entity.EntitledID
        };

        return ApiResponse<MissingDocumentDto>.Success(docDto, "تم تسجيل الوثيقة الناقصة");
    }

    public Task<ApiResponse<bool>> MarkFulfilledAsync(int id, int userId)
        => ChangeMissingAsync(id, () => MarkFulfilledCoreAsync(id, userId));

    private async Task<ApiResponse<bool>> MarkFulfilledCoreAsync(int id, int userId)
    {
        var doc = await (await _access.ForCurrentOperationAsync()).Query(_unitOfWork.MissingDocuments)
            .FirstOrDefaultAsync(m => m.MissingDocumentID == id);

        if (doc == null)
            return ApiResponse<bool>.Fail("الوثيقة الناقصة غير موجودة");

        if (doc.Status == MissingDocumentStatusConstants.Fulfilled)
            return ApiResponse<bool>.Fail("الوثيقة الناقصة مُسلّمة مسبقاً");
            
        if (doc.Status == MissingDocumentStatusConstants.Exempted)
            return ApiResponse<bool>.Fail("لا يمكن تسليم وثيقة معفاة");

        var requirements = RetirementFileDocumentRules.Requirements(await GetRequiredPairsAsync(doc.FileID), [doc]);
        var documents = await _unitOfWork.Documents.Query()
            .Where(d => d.FileID == doc.FileID && d.DocumentTypeID == doc.DocumentTypeID).ToListAsync();
        if (RetirementFileDocumentRules.FindValidDocument(documents,
            RetirementFileDocumentRules.Owner(doc, requirements), PensionMS.Core.Time.YemenTime.Today) == null)
            return ApiResponse<bool>.Fail("لا يمكن تسجيل التسليم دون وثيقة سارية وغير مرفوضة تخص المالك والملف نفسيهما");

        doc.Status      = MissingDocumentStatusConstants.Fulfilled;
        doc.SubmittedAt = DateTime.UtcNow;

        _unitOfWork.MissingDocuments.Update(doc);
        await _unitOfWork.SaveChangesAsync();
        await RefreshComplianceCacheAsync(doc.FileID);
        
        _logger.LogInformation("تم تعليم الوثيقة الناقصة {DocId} كمسلّمة للمستخدم {UserId}", id, userId);

        return ApiResponse<bool>.Success(true, "تم تعليم الوثيقة كمسلّمة");
    }

    public Task<ApiResponse<bool>> DeleteAsync(int id, int userId)
        => ChangeMissingAsync(id, () => DeleteCoreAsync(id, userId));

    private async Task<ApiResponse<bool>> DeleteCoreAsync(int id, int userId)
    {
        var doc = await (await _access.ForCurrentOperationAsync()).Query(_unitOfWork.MissingDocuments)
            .FirstOrDefaultAsync(m => m.MissingDocumentID == id);

        if (doc == null)
            return ApiResponse<bool>.Fail("الوثيقة الناقصة غير موجودة");

        if (doc.PledgeID != null)
            return ApiResponse<bool>.Fail("لا يمكن حذف وثيقة ناقصة مرتبطة بتعهد. يجب إلغاء التعهد أولاً.");
            
        if (doc.ExemptionID != null)
            return ApiResponse<bool>.Fail("لا يمكن حذف وثيقة ناقصة معفاة. يجب إلغاء الإعفاء أولاً.");

        doc.IsDeleted = true;
        doc.DeletedAt = DateTime.UtcNow;
        doc.DeletedByUserID = userId;
        await _unitOfWork.SaveChangesAsync();
        await RefreshComplianceCacheAsync(doc.FileID);
        
        _logger.LogInformation("تم حذف الوثيقة الناقصة {DocId} للملف {FileId} بواسطة المستخدم {UserId}", id, doc.FileID, userId);

        return ApiResponse<bool>.Success(true, "تم حذف سجل الوثيقة الناقصة");
    }

    private Task<ApiResponse<bool>> ChangeMissingAsync(int id, Func<Task<ApiResponse<bool>>> change)
        => PensionMS.Infrastructure.Data.UnitOfWorkTransaction.ExecuteAsync(_unitOfWork, async () =>
        {
            var fileId = await (await _access.ForCurrentOperationAsync()).Query(_unitOfWork.MissingDocuments)
                .Where(m => m.MissingDocumentID == id).Select(m => (int?)m.FileID).FirstOrDefaultAsync();
            if (!fileId.HasValue) return ApiResponse<bool>.Fail("الوثيقة الناقصة غير موجودة");
            await _unitOfWork.LockFileReviewAsync(fileId.Value);
            return await change();
        });

    public async Task<List<AutoDetectedMissingDocumentDto>> GetRequiredPairsAsync(int fileId)
    {
        await EnsureFileAccessAsync(fileId);
        var (pairs, _, _) = await BuildPairsCoreAsync(fileId);
        return pairs;
    }

    public async Task<int> StageRequiredAsync(int fileId, int userId)
    {
        if (!_unitOfWork.HasActiveTransaction)
            throw new InvalidOperationException("تسجيل النواقص المطلوبة يستلزم معاملة نشطة");
        await EnsureFileAccessAsync(fileId);
        await _unitOfWork.LockFileReviewAsync(fileId);
        var file = await _unitOfWork.RetirementFiles.Query().Include(f => f.MissingDocuments)
            .FirstOrDefaultAsync(f => f.FileID == fileId && !f.IsDeleted)
            ?? throw new PensionMS.Core.Exceptions.EntityNotFoundException("الملف غير موجود");
        var (pairs, documents, _) = await BuildPairsCoreAsync(fileId);
        var requirements = RetirementFileDocumentRules.Requirements(pairs, []);
        var today = PensionMS.Core.Time.YemenTime.Today;
        var uploaded = documents.Where(d => RetirementFileDocumentRules.IsValid(d, today))
            .Select(RetirementFileDocumentRules.Owner).ToHashSet();
        // The tracked navigation includes rows staged by an earlier call before SaveChanges.
        var recorded = file.MissingDocuments.Where(m => !m.IsDeleted &&
                m.Status != MissingDocumentStatusConstants.Fulfilled)
            .Select(m => RetirementFileDocumentRules.Owner(m, requirements)).ToHashSet();
        var added = 0;
        foreach (var pair in pairs)
        {
            var owner = RetirementFileDocumentRules.Owner(pair);
            if (uploaded.Contains(owner) || !recorded.Add(owner)) continue;
            var row = new MissingDocument
            {
                FileID = fileId, DocumentTypeID = pair.DocumentTypeID, AgentID = pair.AgentID,
                GuardianID = pair.GuardianID, EntitledID = pair.EntitledID,
                PledgeID = pair.PledgeID, ExemptionID = pair.ExemptionID,
                Status = MissingDocumentStatusConstants.Pending,
                Notes = pair.EntityType == EntityTypeConstants.Retiree && pair.DocumentTypeID == DocumentTypeConstants.DeathCertificate
                    ? "شهادة وفاة المتقاعد"
                    : pair.EntityType == EntityTypeConstants.File && pair.DocumentTypeID == DocumentTypeConstants.InheritanceCert
                        ? "حصر ورثة المتقاعد المتوفى"
                        : $"{pair.DocumentTypeName} — {pair.EntityName}"
            };
            await _unitOfWork.MissingDocuments.AddAsync(row);
            if (!file.MissingDocuments.Contains(row)) file.MissingDocuments.Add(row);
            added++;
        }
        _logger.LogInformation("تم تجهيز {Count} نواقص للملف {FileId} بواسطة المستخدم {UserId}", added, fileId, userId);
        return added;
    }

    public async Task<ApiResponse<List<AutoDetectedMissingDocumentDto>>> GetAutoDetectedByFileAsync(int fileId)
    {
        await EnsureFileAccessAsync(fileId);
        var (pairs, uploadedDocs, manualMissing) = await BuildPairsCoreAsync(fileId);

        var today = PensionMS.Core.Time.YemenTime.Today;
        var uploaded = uploadedDocs.Where(d => RetirementFileDocumentRules.IsValid(d, today))
            .Select(RetirementFileDocumentRules.Owner).ToHashSet();
        var requirements = RetirementFileDocumentRules.Requirements(pairs, []);
        // Keep existing missing records in their own list, matching the same owner as compliance and activation.
        var manuallyFlagged = manualMissing.Where(m => m.Status != MissingDocumentStatusConstants.Fulfilled)
            .Select(m => RetirementFileDocumentRules.Owner(m, requirements)).ToHashSet();

        return ApiResponse<List<AutoDetectedMissingDocumentDto>>.Success(
            pairs.Where(p => !uploaded.Contains(RetirementFileDocumentRules.Owner(p)) &&
                !manuallyFlagged.Contains(RetirementFileDocumentRules.Owner(p))).ToList());
    }

    private async Task EnsureFileAccessAsync(int fileId)
    {
        var queries = await _access.ForCurrentOperationAsync();
        if (!await queries.Files().AnyAsync(f => f.FileID == fileId && !f.IsDeleted))
            throw new PensionMS.Core.Exceptions.EntityNotFoundException("الملف غير موجود ضمن نطاق الصلاحية");
    }

    private async Task<(List<AutoDetectedMissingDocumentDto> Pairs, List<Document> UploadedDocs, List<MissingDocument> ManualMissing)> BuildPairsCoreAsync(int fileId)
    {
        // ── 1. جلب كافة الكيانات والبيانات النشطة المرتبطة بالملف ──
        
        var retiree = await _unitOfWork.Retirees.Query()
            .AsNoTracking()
            .Where(r => r.RetirementFiles.Any(rf => rf.FileID == fileId) && !r.IsDeleted)
            .Select(r => new { r.RetireeID, r.FullName, r.IsAlive, r.HealthStatusID, r.SocialStatusID })
            .FirstOrDefaultAsync();

        var entitledPersonsRaw = await _unitOfWork.EntitledPersons.Query()
            .AsNoTracking()
            .Where(e => e.FileID == fileId && !e.IsDeleted && !e.IsDeceased)
            .Select(e => new { e.EntitledID, e.FullName, e.IsDeceased, e.HealthStatusID, e.SocialStatusID, e.HasDisabilityException, e.BirthDate, e.RelationshipID, e.StopCode })
            .ToListAsync();

        var activeAgents = await _unitOfWork.Agents.Query()
            .AsNoTracking()
            .Where(a => a.FileID == fileId && a.IsActive && !a.IsDeleted)
            .Select(a => new { a.AgentID, a.FullName })
            .ToListAsync();

        var activeGuardians = await _unitOfWork.Guardians.Query()
            .AsNoTracking()
            .Where(g => g.FileID == fileId && g.IsActive && !g.IsDeleted)
            .Select(g => new { g.GuardianID, g.FullName })
            .ToListAsync();

        var hasAnyPaymentBooks = await _unitOfWork.PaymentBooks.Query()
            .AsNoTracking()
            .Where(pb => pb.FileID == fileId && !pb.IsDeleted)
            .AnyAsync();

        var activePledges = await _unitOfWork.DocumentPledges.Query()
            .AsNoTracking()
            .Where(p => p.FileID == fileId && !p.IsDeleted && p.Status != MissingDocumentStatusConstants.Fulfilled)
            .Select(p => new { p.PledgeID, p.PledgeNumber, p.PledgedByName })
            .ToListAsync();

        var activeExemptions = await _unitOfWork.DocumentExemptions.Query()
            .AsNoTracking()
            .Where(e => e.FileID == fileId)
            .Select(e => new { e.ExemptionID, e.DirectiveSource })
            .ToListAsync();

        // ── 2. جلب أنواع الوثائق، والمستندات المرفوعة والمسجلة بدلالة المالك ──
        
        var allDocTypes = await _unitOfWork.DocumentTypes.Query()
            .AsNoTracking()
            .Where(dt => dt.IsActive)
            .ToDictionaryAsync(dt => dt.DocumentTypeID, dt => dt.TypeName);

        // جلب جميع سجلات النواقص (بما فيها المسلّمة والمعفاة) لمنع إعادة اكتشاف الوثائق المعالجة
        var manualMissing = await _unitOfWork.MissingDocuments.Query()
            .AsNoTracking()
            .Where(m => m.FileID == fileId && !m.IsDeleted)
            .Select(m => new MissingDocument { DocumentTypeID = m.DocumentTypeID, EntitledID = m.EntitledID,
                AgentID = m.AgentID, GuardianID = m.GuardianID, PledgeID = m.PledgeID, ExemptionID = m.ExemptionID,
                Status = m.Status })
            .ToListAsync();

        var uploadedDocs = await _unitOfWork.Documents.Query()
            .AsNoTracking()
            .Where(d => d.FileID == fileId && !d.IsDeleted)
            .Select(d => new Document { DocumentTypeID = d.DocumentTypeID, RetireeID = d.RetireeID,
                EntitledID = d.EntitledID, AgentID = d.AgentID, GuardianID = d.GuardianID,
                PledgeID = d.PledgeID, ExemptionID = d.ExemptionID, ExpiryDate = d.ExpiryDate,
                PendingReviewStatus = d.PendingReviewStatus, IsDeleted = d.IsDeleted })
            .ToListAsync();

        // ── 3. اختيار المتطلبات من الحقائق المحملة، ثم إضافة أسماء العرض في الخدمة ──
        var facts = new RetirementDocumentRequirementRules.FileFacts(
            retiree == null ? null : new RetirementDocumentRequirementRules.RetireeFacts(retiree.RetireeID, retiree.IsAlive, retiree.HealthStatusID),
            hasAnyPaymentBooks,
            entitledPersonsRaw.Select(e => new RetirementDocumentRequirementRules.EntitledPersonFacts(
                e.EntitledID, e.BirthDate, e.RelationshipID, e.SocialStatusID, e.HealthStatusID, e.HasDisabilityException, e.StopCode)).ToArray(),
            activeAgents.Select(a => a.AgentID).ToArray(), activeGuardians.Select(g => g.GuardianID).ToArray(),
            activePledges.Select(p => p.PledgeID).ToArray(), activeExemptions.Select(e => e.ExemptionID).ToArray());
        var requirements = RetirementDocumentRequirementRules.SelectRequirements(facts, allDocTypes.Keys.ToHashSet(),
            PensionMS.Core.Time.YemenTime.Today);

        var entitledNames = entitledPersonsRaw.ToDictionary(e => e.EntitledID, e => e.FullName);
        var agentNames = activeAgents.ToDictionary(a => a.AgentID, a => a.FullName);
        var guardianNames = activeGuardians.ToDictionary(g => g.GuardianID, g => g.FullName);
        var pledgeNames = activePledges.ToDictionary(p => p.PledgeID, p => p.PledgedByName ?? p.PledgeNumber ?? "تعهد نشط");
        var exemptionNames = activeExemptions.ToDictionary(e => e.ExemptionID,
            e => string.IsNullOrEmpty(e.DirectiveSource) ? "إعفاء نشط" : e.DirectiveSource);

        AutoDetectedMissingDocumentDto MapRequiredDoc(RetirementDocumentRequirement requirement)
        {
            var (entityType, entityName) = requirement switch
            {
                { RetireeID: int id } => (EntityTypeConstants.Retiree, retiree!.FullName),
                { EntitledID: int id } => (EntityTypeConstants.EntitledPerson, entitledNames[id]),
                { AgentID: int id } => (EntityTypeConstants.Agent, agentNames[id]),
                { GuardianID: int id } => (EntityTypeConstants.Guardian, guardianNames[id]),
                { PledgeID: int id } => (EntityTypeConstants.Pledge, pledgeNames[id]),
                { ExemptionID: int id } => (EntityTypeConstants.Exemption, exemptionNames[id]),
                _ => (EntityTypeConstants.File, requirement.DocumentTypeID == DocumentTypeConstants.PaymentBookImage ? "دفتر الصرف" : "الملف العام")
            };
            return new AutoDetectedMissingDocumentDto
            {
                DocumentTypeID = requirement.DocumentTypeID, DocumentTypeName = allDocTypes[requirement.DocumentTypeID],
                IsRequired = true, IsAutoDetected = true, EntityType = entityType, EntityName = entityName,
                RetireeID = requirement.RetireeID, EntitledID = requirement.EntitledID, AgentID = requirement.AgentID,
                GuardianID = requirement.GuardianID, PledgeID = requirement.PledgeID, ExemptionID = requirement.ExemptionID
            };
        }

        var autoDetected = requirements.Select(MapRequiredDoc).ToList();

        return (autoDetected, uploadedDocs, manualMissing);
    }

}
