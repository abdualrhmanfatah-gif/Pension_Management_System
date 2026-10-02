using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.MissingDocuments;
using PensionMS.Core.DTOs.EntitledPersons;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;

namespace PensionMS.Infrastructure.Services;

public class MissingDocumentService : IMissingDocumentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEntitlementEngine _entitlementEngine;
    private readonly ILogger<MissingDocumentService> _logger;

    public MissingDocumentService(IUnitOfWork unitOfWork, IEntitlementEngine entitlementEngine, ILogger<MissingDocumentService> logger)
    {
        _unitOfWork = unitOfWork;
        _entitlementEngine = entitlementEngine;
        _logger = logger;
    }

    public async Task<ApiResponse<List<MissingDocumentDto>>> GetByFileAsync(int fileId)
    {
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
        var doc = await _unitOfWork.MissingDocuments.Query()
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

    public async Task<ApiResponse<MissingDocumentDto>> CreateAsync(CreateMissingDocumentDto dto, int userId)
    {
        // 1. التحقق من وجود الملف
        var fileExists = await _unitOfWork.RetirementFiles.Query()
            .AnyAsync(f => f.FileID == dto.FileID && !f.IsDeleted);
        if (!fileExists)
            return ApiResponse<MissingDocumentDto>.Fail("الملف المحدد غير موجود أو محذوف");

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

    public async Task<ApiResponse<bool>> MarkFulfilledAsync(int id, int userId)
    {
        var doc = await _unitOfWork.MissingDocuments.Query()
            .FirstOrDefaultAsync(m => m.MissingDocumentID == id);

        if (doc == null)
            return ApiResponse<bool>.Fail("الوثيقة الناقصة غير موجودة");

        if (doc.Status == MissingDocumentStatusConstants.Fulfilled)
            return ApiResponse<bool>.Fail("الوثيقة الناقصة مُسلّمة مسبقاً");
            
        if (doc.Status == MissingDocumentStatusConstants.Exempted)
            return ApiResponse<bool>.Fail("لا يمكن تسليم وثيقة معفاة");

        doc.Status      = MissingDocumentStatusConstants.Fulfilled;
        doc.SubmittedAt = DateTime.UtcNow;

        _unitOfWork.MissingDocuments.Update(doc);
        await _unitOfWork.SaveChangesAsync();
        
        _logger.LogInformation("تم تعليم الوثيقة الناقصة {DocId} كمسلّمة للمستخدم {UserId}", id, userId);

        return ApiResponse<bool>.Success(true, "تم تعليم الوثيقة كمسلّمة");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id, int userId)
    {
        var doc = await _unitOfWork.MissingDocuments.Query()
            .FirstOrDefaultAsync(m => m.MissingDocumentID == id);

        if (doc == null)
            return ApiResponse<bool>.Fail("الوثيقة الناقصة غير موجودة");

        if (doc.PledgeID != null)
            return ApiResponse<bool>.Fail("لا يمكن حذف وثيقة ناقصة مرتبطة بتعهد. يجب إلغاء التعهد أولاً.");
            
        if (doc.ExemptionID != null)
            return ApiResponse<bool>.Fail("لا يمكن حذف وثيقة ناقصة معفاة. يجب إلغاء الإعفاء أولاً.");

        _unitOfWork.MissingDocuments.Delete(doc);
        await _unitOfWork.SaveChangesAsync();
        
        _logger.LogInformation("تم حذف الوثيقة الناقصة {DocId} للملف {FileId} بواسطة المستخدم {UserId}", id, doc.FileID, userId);

        return ApiResponse<bool>.Success(true, "تم حذف سجل الوثيقة الناقصة");
    }

    public async Task<List<AutoDetectedMissingDocumentDto>> GetRequiredPairsAsync(int fileId)
    {
        var (pairs, _, _) = await BuildPairsCoreAsync(fileId);
        return pairs;
    }

    public async Task<ApiResponse<List<AutoDetectedMissingDocumentDto>>> GetAutoDetectedByFileAsync(int fileId)
    {
        var (pairs, uploadedDocs, manualMissing) = await BuildPairsCoreAsync(fileId);

        bool IsUploaded(AutoDetectedMissingDocumentDto p)
        {
            bool match = uploadedDocs.Any(d =>
                d.DocumentTypeID == p.DocumentTypeID &&
                (
                    p.EntityType == EntityTypeConstants.File ||
                    (p.EntityType == EntityTypeConstants.Retiree && d.RetireeID != null) ||
                    (p.EntityType == EntityTypeConstants.EntitledPerson && d.EntitledID == p.EntitledID) ||
                    (p.EntityType == EntityTypeConstants.Agent && d.AgentID == p.AgentID) ||
                    (p.EntityType == EntityTypeConstants.Guardian && d.GuardianID == p.GuardianID) ||
                    p.EntityType == EntityTypeConstants.Pledge ||
                    p.EntityType == EntityTypeConstants.Exemption
                ));

            if (!match && (p.EntityType == EntityTypeConstants.Retiree || p.EntityType == EntityTypeConstants.File))
                match = uploadedDocs.Any(d => d.DocumentTypeID == p.DocumentTypeID && d.EntitledID == null && d.AgentID == null && d.GuardianID == null);

            return match;
        }

        bool IsManuallyFlagged(AutoDetectedMissingDocumentDto p) =>
            manualMissing.Any(m =>
                m.DocumentTypeID == p.DocumentTypeID &&
                m.Status != MissingDocumentStatusConstants.Fulfilled &&
                (
                    ((p.EntityType == EntityTypeConstants.File || p.EntityType == EntityTypeConstants.Retiree
                      || p.EntityType == EntityTypeConstants.Pledge || p.EntityType == EntityTypeConstants.Exemption)
                     && m.EntitledID == null && m.AgentID == null && m.GuardianID == null) ||
                    (p.EntityType == EntityTypeConstants.EntitledPerson && m.EntitledID == p.EntitledID) ||
                    (p.EntityType == EntityTypeConstants.Agent && m.AgentID == p.AgentID) ||
                    (p.EntityType == EntityTypeConstants.Guardian && m.GuardianID == p.GuardianID)
                ));

        return ApiResponse<List<AutoDetectedMissingDocumentDto>>.Success(
            pairs.Where(p => !IsUploaded(p) && !IsManuallyFlagged(p)).ToList());
    }

    private async Task<(List<AutoDetectedMissingDocumentDto> Pairs, List<UploadedDocLink> UploadedDocs, List<ManualMissingLink> ManualMissing)> BuildPairsCoreAsync(int fileId)
    {
        var autoDetected = new List<AutoDetectedMissingDocumentDto>();

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

        // تصفية المستحقين: استبعاد غير المؤهلين قانونياً (تجاوز السن أو زواج البنت)
        var entitledPersons = entitledPersonsRaw
            .Where(e => e.StopCode != EntitledStopCodes.AgeLimit && e.StopCode != EntitledStopCodes.DaughterMarried)
            .ToList();

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
            .Where(m => m.FileID == fileId)
            .Select(m => new ManualMissingLink(m.DocumentTypeID, m.EntitledID, m.AgentID, m.GuardianID, m.Status))
            .ToListAsync();

        var uploadedDocs = await _unitOfWork.Documents.Query()
            .AsNoTracking()
            .Where(d => d.FileID == fileId && !d.IsDeleted)
            .Select(d => new UploadedDocLink(d.DocumentTypeID, d.RetireeID, d.EntitledID, d.AgentID, d.GuardianID))
            .ToListAsync();

        // ── 3. الدالة الذكية للإضافة المتوافقة مع الكيان المحدد (زوج مطلوب فقط — فلتر الاستيفاء لاحقًا في المُظهر) ──
        void AddRequiredDoc(int docTypeId, string entityType, string entityName, int? entityId = null, bool isOptional = false)
        {
            if (!allDocTypes.TryGetValue(docTypeId, out var docTypeName)) return;

            autoDetected.Add(new AutoDetectedMissingDocumentDto
            {
                DocumentTypeID = docTypeId,
                DocumentTypeName = docTypeName,
                IsRequired = !isOptional,
                IsAutoDetected = true,
                EntityType = entityType,
                EntityName = entityName,
                RetireeID = entityType == EntityTypeConstants.Retiree ? entityId : null,
                AgentID = entityType == EntityTypeConstants.Agent ? entityId : null,
                GuardianID = entityType == EntityTypeConstants.Guardian ? entityId : null,
                EntitledID = entityType == EntityTypeConstants.EntitledPerson ? entityId : null,
                PledgeID = entityType == EntityTypeConstants.Pledge ? entityId : null,
                ExemptionID = entityType == EntityTypeConstants.Exemption ? entityId : null
            });
        }

        // ════════════════════════════════════════════
        // 4. تطبيق شروط الرصد حسب الجدول بدقة 100%
        // ════════════════════════════════════════════

        bool retireeIsAlive = retiree?.IsAlive ?? true;

        // ── أوليات وثائق الملف (File Level) ──
        // (ID=8) أوليات الملف: دائمًا
        AddRequiredDoc(DocumentTypeConstants.FileOriginals, EntityTypeConstants.File, "الملف العام");

        // (ID=13) صورة دفتر الصرف: في حال وجود أي دفتر صرف (قديم أو إلكتروني)
        if (hasAnyPaymentBooks)
            AddRequiredDoc(DocumentTypeConstants.PaymentBookImage, EntityTypeConstants.File, "دفتر الصرف");

        // (ID=7) حصر ورثة / قسام شرعي: فقط عند وفاة المتقاعد
        if (retiree != null && (!retiree.IsAlive || retiree.HealthStatusID == HealthStatusConstants.Deceased || retiree.HealthStatusID == HealthStatusConstants.Martyr))
            AddRequiredDoc(DocumentTypeConstants.InheritanceCert, EntityTypeConstants.File, "الملف العام");

        // ── سياق المتقاعد (Retiree Context) ──
        if (retiree != null)
        {
            // الهوية الوطنية تُطلب فقط إذا كان المتقاعد حياً
            if (retiree.IsAlive)
                AddRequiredDoc(DocumentTypeConstants.NationalId, EntityTypeConstants.Retiree, retiree.FullName, retiree.RetireeID);

            if (!retiree.IsAlive || retiree.HealthStatusID == HealthStatusConstants.Deceased || retiree.HealthStatusID == HealthStatusConstants.Martyr)
            {
                AddRequiredDoc(DocumentTypeConstants.DeathCertificate, EntityTypeConstants.Retiree, retiree.FullName, retiree.RetireeID); // شهادة وفاة
            }

            if (retiree.HealthStatusID == HealthStatusConstants.PartiallyDisabled || retiree.HealthStatusID == HealthStatusConstants.FullyDisabled)
                AddRequiredDoc(DocumentTypeConstants.MedicalReport, EntityTypeConstants.Retiree, retiree.FullName, retiree.RetireeID); // تقرير طبي
        }

        // ── سياق المستحقين (Entitled Persons Context) ──
        // وثائق المستحقين تُطلب فقط عند وفاة المتقاعد
        if (!retireeIsAlive)
        {
            var today = PensionMS.Core.Time.YemenTime.Today;
            var femaleRelationshipIds = new[] { 1, 3, 4, 6 }; // زوجة، ابنة، أم، أخت

            foreach (var ep in entitledPersons)
            {
                // احتساب عمر المستحق، تطلب الهوية لمن تفوق أعمارهم أو تساوي 18 عاماً كبالغين
                var age = today.Year - ep.BirthDate.Year;
                if (ep.BirthDate > today.AddYears(-age)) age--;
                
                if (age >= 18)
                {
                    AddRequiredDoc(DocumentTypeConstants.NationalId, EntityTypeConstants.EntitledPerson, ep.FullName, ep.EntitledID); // الهوية الوطنية
                }

                bool isFemale = femaleRelationshipIds.Contains(ep.RelationshipID);

                if (ep.IsDeceased || ep.HealthStatusID == HealthStatusConstants.Deceased || ep.HealthStatusID == HealthStatusConstants.Martyr)
                {
                    // شهادة الوفاة تطلب للإناث فقط (بناءً على طلب تعديل القواعد)
                    if (isFemale)
                        AddRequiredDoc(DocumentTypeConstants.DeathCertificate, EntityTypeConstants.EntitledPerson, ep.FullName, ep.EntitledID);
                }
                else if (isFemale)
                {
                    // القاصر الأنثى معفاة من إثبات العزوبية — فقط البالغة تُطالب بها
                    if (ep.SocialStatusID == SocialStatusConstants.Single && age >= 18) 
                        AddRequiredDoc(DocumentTypeConstants.SingleStatusCert, EntityTypeConstants.EntitledPerson, ep.FullName, ep.EntitledID); // شهادة عزوبية
                        
                    if (ep.SocialStatusID == SocialStatusConstants.Married) 
                        AddRequiredDoc(DocumentTypeConstants.MarriageContract, EntityTypeConstants.EntitledPerson, ep.FullName, ep.EntitledID); // عقد زواج
                        
                    if (ep.SocialStatusID == SocialStatusConstants.Divorced) 
                        AddRequiredDoc(DocumentTypeConstants.DivorceCertificate, EntityTypeConstants.EntitledPerson, ep.FullName, ep.EntitledID); // طلاق
                }

                if (ep.HasDisabilityException || ep.HealthStatusID == HealthStatusConstants.PartiallyDisabled || ep.HealthStatusID == HealthStatusConstants.FullyDisabled)
                {
                    AddRequiredDoc(DocumentTypeConstants.MedicalReport, EntityTypeConstants.EntitledPerson, ep.FullName, ep.EntitledID); // تقرير طبي
                    AddRequiredDoc(DocumentTypeConstants.MaintenanceJudgment, EntityTypeConstants.EntitledPerson, ep.FullName, ep.EntitledID); // حكم إعالة
                }
            }
        }

        // ── سياق الوكلاء (Agents Context) ──
        foreach (var agent in activeAgents)
        {
            AddRequiredDoc(DocumentTypeConstants.NationalId, EntityTypeConstants.Agent, agent.FullName, agent.AgentID); // الهوية الوطنية
            AddRequiredDoc(DocumentTypeConstants.AgencyDocument, EntityTypeConstants.Agent, agent.FullName, agent.AgentID); // صك وكالة
        }

        // ── سياق الأوصياء (Guardians Context) ──
        foreach (var guardian in activeGuardians)
        {
            AddRequiredDoc(DocumentTypeConstants.NationalId, EntityTypeConstants.Guardian, guardian.FullName, guardian.GuardianID); // الهوية الوطنية
            AddRequiredDoc(DocumentTypeConstants.GuardianshipDocument, EntityTypeConstants.Guardian, guardian.FullName, guardian.GuardianID); // صك وصاية
        }

        // ── سياق التعهدات النشطة (Pledges Context) ──
        foreach (var pledge in activePledges)
        {
            var pName = pledge.PledgedByName ?? pledge.PledgeNumber ?? "تعهد نشط";
            AddRequiredDoc(DocumentTypeConstants.PledgeDocument, EntityTypeConstants.Pledge, pName, pledge.PledgeID); // وثيقة تعهد الزاميه
        }

        // ── سياق الإعفاءات النشطة (Exemptions Context) ──
        foreach (var exemption in activeExemptions)
        {
            var eName = string.IsNullOrEmpty(exemption.DirectiveSource) ? "إعفاء نشط" : exemption.DirectiveSource;
            AddRequiredDoc(DocumentTypeConstants.ExemptionDirective, EntityTypeConstants.Exemption, eName, exemption.ExemptionID); // مستند إعفاء الزاميه
        }

        return (autoDetected, uploadedDocs, manualMissing);
    }

    private sealed record UploadedDocLink(int DocumentTypeID, int? RetireeID, int? EntitledID, int? AgentID, int? GuardianID);

    private sealed record ManualMissingLink(int DocumentTypeID, int? EntitledID, int? AgentID, int? GuardianID, string Status);
}
