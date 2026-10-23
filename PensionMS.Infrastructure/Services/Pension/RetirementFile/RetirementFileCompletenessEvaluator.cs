using PensionMS.Core.Constants;
using Microsoft.EntityFrameworkCore;
using PensionMS.Core.Entities;
using PensionMS.Core.Interfaces;

namespace PensionMS.Infrastructure.Services.Pension.RetirementFile;

internal sealed class RetirementFileCompletenessEvaluator
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMissingDocumentService _missingDocumentService;

    public RetirementFileCompletenessEvaluator(IUnitOfWork unitOfWork, IMissingDocumentService missingDocumentService)
    {
        _unitOfWork = unitOfWork;
        _missingDocumentService = missingDocumentService;
    }

    /// <summary>
    /// حساب عدد وقائمة البيانات الناقصة للملف ولجميع الكيانات (متقاعد، وكلاء، أوصياء، مستحقون) بشكل كامل وبدون استثناء
    /// </summary>
    internal async Task<(int Count, List<string> Items)> CalculateMissingDataAsync(int fileId)
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

    private static (int Count, List<string> Items) CalculateMissingData(PensionMS.Core.Entities.RetirementFile? file)
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


    internal async Task<List<string>> GetMissingDocsListAsync(int fileId)
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

}
