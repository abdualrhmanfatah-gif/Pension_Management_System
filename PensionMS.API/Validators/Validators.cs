using FluentValidation;
using PensionMS.Core.DTOs.Auth;
using PensionMS.Core.DTOs.Retirees;
using PensionMS.Core.DTOs.RetirementFiles;
using PensionMS.Core.DTOs.EntitledPersons;
using PensionMS.Core.DTOs.Users;
using PensionMS.Core.DTOs.Documents;

namespace PensionMS.API.Validators;

public class CreateAgentValidator : AbstractValidator<PensionMS.Core.DTOs.Agents.CreateAgentDto>
{
    public CreateAgentValidator()
    {
        RuleFor(x => x.FileID).GreaterThan(0).WithMessage("معرف الملف غير صالح");
        RuleFor(x => x.FullName).NotEmpty().MinimumLength(3).MaximumLength(200).WithMessage("اسم الوكيل مطلوب ويجب أن يتراوح بين 3 و200 حرف");
        RuleFor(x => x.NationalID).NotEmpty().MaximumLength(20).WithMessage("رقم هوية الوكيل مطلوب ولا يتجاوز 20 حرفًا");
        RuleFor(x => x.Phone).MaximumLength(20).WithMessage("رقم الهاتف لا يتجاوز 20 حرفًا");
        RuleFor(x => x.AgencyNumber).MaximumLength(100).WithMessage("رقم الوكالة لا يتجاوز 100 حرف");
    }
}

public class UpdateAgentValidator : AbstractValidator<PensionMS.Core.DTOs.Agents.UpdateAgentDto>
{
    public UpdateAgentValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MinimumLength(3).MaximumLength(200).WithMessage("اسم الوكيل مطلوب ويجب أن يتراوح بين 3 و200 حرف");
        RuleFor(x => x.NationalID).NotEmpty().MaximumLength(20).WithMessage("رقم هوية الوكيل مطلوب ولا يتجاوز 20 حرفًا");
        RuleFor(x => x.Phone).MaximumLength(20).WithMessage("رقم الهاتف لا يتجاوز 20 حرفًا");
        RuleFor(x => x.AgencyNumber).MaximumLength(100).WithMessage("رقم الوكالة لا يتجاوز 100 حرف");
    }
}

// ═══════════════════════════════════════
// AUTH
// ═══════════════════════════════════════
public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("اسم المستخدم مطلوب");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("كلمة المرور مطلوبة");
    }
}

public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
}



// ═══════════════════════════════════════
// RETIREMENT FILES
// ═══════════════════════════════════════
public class CreateRetirementFileValidator : AbstractValidator<CreateRetirementFileDto>
{
    public CreateRetirementFileValidator()
    {
        RuleFor(x => x.RetireeID)
            .GreaterThan(0).WithMessage("معرف المتقاعد مطلوب");

        // Phase 2.1: التحقق من الرقم الوطني
        RuleFor(x => x.NationalID)
            .NotEmpty().WithMessage("الرقم الوطني مطلوب")
            .When(x => x.IsAlive);

        RuleFor(x => x.NationalID)
            .Length(12).WithMessage("الرقم الوطني يجب أن يكون 12 أرقام")
            .Matches(@"^\d{12}$").WithMessage("الرقم الوطني يجب أن يحتوي على أرقام فقط")
            .When(x => !string.IsNullOrEmpty(x.NationalID));

        // التحقق من رقم الهاتف — موبايل 7XXXXXXXX أو أرضي 0XXXXXXX
        RuleFor(x => x.Phone)
            .Matches(@"^(7\d{8}|0\d{7})$").WithMessage("رقم الهاتف غير صحيح (موبايل: 7XXXXXXXX / أرضي: 0XXXXXXX)")
            .When(x => !string.IsNullOrEmpty(x.Phone));

        // Phase 2.1: تاريخ الوفاة مطلوب عند حالة متوفي
        RuleFor(x => x.DeathDate)
            .NotNull().WithMessage("تاريخ الوفاة مطلوب عند حالة متوفي")
            .When(x => !x.IsAlive);

        RuleFor(x => x.ServiceEndDate)
            .GreaterThan(x => x.ServiceStartDate)
            .WithMessage("تاريخ نهاية الخدمة يجب أن يكون بعد تاريخ البداية")
            .When(x => x.ServiceStartDate.HasValue && x.ServiceEndDate.HasValue);
    }
}

// Phase 2.2: Validator جديد لتحديث الملف
public class UpdateRetirementFileValidator : AbstractValidator<UpdateRetirementFileDto>
{
    public UpdateRetirementFileValidator()
    {
        RuleFor(x => x.NationalID)
            .Length(12).WithMessage("الرقم الوطني يجب أن يكون 12 أرقام")
            .Matches(@"^\d{12}$").WithMessage("الرقم الوطني يجب أن يحتوي على أرقام فقط")
            .When(x => !string.IsNullOrEmpty(x.NationalID));

        RuleFor(x => x.FullName)
            .MaximumLength(200).WithMessage("الاسم لا يتجاوز 200 حرف")
            .When(x => !string.IsNullOrEmpty(x.FullName));

        RuleFor(x => x.Phone)
            .Matches(@"^(7\d{8}|0\d{7})$").WithMessage("رقم الهاتف غير صحيح (موبايل: 7XXXXXXXX / أرضي: 0XXXXXXX)")
            .When(x => !string.IsNullOrEmpty(x.Phone));

        RuleFor(x => x.ServiceEndDate)
            .GreaterThan(x => x.ServiceStartDate)
            .WithMessage("تاريخ نهاية الخدمة يجب أن يكون بعد البداية")
            .When(x => x.ServiceStartDate.HasValue && x.ServiceEndDate.HasValue);
    }
}

// ═══════════════════════════════════════
// ENTITLED PERSONS
// ═══════════════════════════════════════
public class CreateEntitledPersonValidator : AbstractValidator<CreateEntitledPersonDto>
{
    public CreateEntitledPersonValidator()
    {
        RuleFor(x => x.FileID)
            .GreaterThan(0).WithMessage("معرف الملف مطلوب");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("الاسم الكامل مطلوب")
            .MaximumLength(200).WithMessage("الاسم لا يتجاوز 200 حرف");

        RuleFor(x => x.NationalID)
            .Length(12).WithMessage("رقم الهوية يجب أن يكون 12 أرقام")
            .Matches(@"^\d{12}$").WithMessage("رقم الهوية يجب أن يحتوي على أرقام فقط")
            .When(x => !string.IsNullOrEmpty(x.NationalID));

        RuleFor(x => x.Phone)
            .Matches(@"^(7\d{8}|0\d{7})$").WithMessage("رقم الهاتف غير صحيح (موبايل: 7XXXXXXXX / أرضي: 0XXXXXXX)")
            .When(x => !string.IsNullOrEmpty(x.Phone));
    }

    private static int CalculateAge(DateTime dateOfBirth)
    {
        var today = PensionMS.Core.Time.YemenTime.TodayDateTime;
        var age = today.Year - dateOfBirth.Year;
        if (dateOfBirth.Date > today.AddYears(-age)) age--;
        return age;
    }
}

public class StopEntitledPersonValidator : AbstractValidator<StopEntitledPersonDto>
{
    private static readonly string[] ValidCodes =
    [
        EntitledStopCodes.AgeLimit,
        EntitledStopCodes.DaughterMarried,
        EntitledStopCodes.WidowRemarried,
        EntitledStopCodes.Deceased,
        EntitledStopCodes.AdminDecision,
        EntitledStopCodes.Employed
    ];

    public StopEntitledPersonValidator()
    {
        RuleFor(x => x.StopCode)
            .NotEmpty().WithMessage("كود سبب الإيقاف مطلوب")
            .Must(c => ValidCodes.Contains(c))
            .WithMessage($"كود غير معروف — القيم المسموحة: {string.Join(", ", ValidCodes)}");
    }
}

public class ReactivateEntitledPersonValidator : AbstractValidator<ReactivateEntitledPersonDto>
{
    private static readonly string[] ValidCodes =
    [
        ReactivationCodes.Divorced,
        ReactivationCodes.HusbandDeceased,
        ReactivationCodes.DisabilityReport,
        ReactivationCodes.AdminDecision
    ];

    public ReactivateEntitledPersonValidator()
    {
        RuleFor(x => x.ReactivationCode)
            .NotEmpty().WithMessage("رمز سبب إعادة التفعيل مطلوب")
            .Must(c => ValidCodes.Contains(c))
            .WithMessage($"رمز غير معروف — القيم المسموحة: {string.Join(", ", ValidCodes)}");
    }
}

// ═══════════════════════════════════════
// USERS
// ═══════════════════════════════════════
public class CreateUserValidator : AbstractValidator<CreateUserDto>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("الاسم الكامل مطلوب")
            .MaximumLength(200).WithMessage("الاسم لا يتجاوز 200 حرف");

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("اسم المستخدم مطلوب")
            .MinimumLength(4).WithMessage("اسم المستخدم لا يقل عن 4 أحرف")
            .MaximumLength(50).WithMessage("اسم المستخدم لا يتجاوز 50 حرف")
            .Matches(@"^[a-zA-Z0-9_]+$").WithMessage("اسم المستخدم يجب أن يحتوي على أحرف إنجليزية وأرقام فقط");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("كلمة المرور مطلوبة")
            .MinimumLength(8).WithMessage("كلمة المرور لا تقل عن 8 أحرف")
            .Matches(@"[A-Z]").WithMessage("كلمة المرور يجب أن تحتوي على حرف كبير")
            .Matches(@"[a-z]").WithMessage("كلمة المرور يجب أن تحتوي على حرف صغير")
            .Matches(@"\d").WithMessage("كلمة المرور يجب أن تحتوي على رقم");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("صيغة البريد الإلكتروني غير صحيحة")
            .When(x => !string.IsNullOrEmpty(x.Email));
    }
}

public class ChangePasswordValidator : AbstractValidator<ChangePasswordDto>
{
    public ChangePasswordValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("كلمة المرور الحالية مطلوبة");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("كلمة المرور الجديدة مطلوبة")
            .MinimumLength(8).WithMessage("كلمة المرور لا تقل عن 8 أحرف")
            .Matches(@"[A-Z]").WithMessage("كلمة المرور يجب أن تحتوي على حرف كبير")
            .Matches(@"[a-z]").WithMessage("كلمة المرور يجب أن تحتوي على حرف صغير")
            .Matches(@"\d").WithMessage("كلمة المرور يجب أن تحتوي على رقم")
            .NotEqual(x => x.CurrentPassword).WithMessage("كلمة المرور الجديدة يجب أن تختلف عن الحالية");
    }
}

// ═══════════════════════════════════════
// DOCUMENTS
// ═══════════════════════════════════════
public class UploadDocumentValidator : AbstractValidator<UploadDocumentDto>
{
    public UploadDocumentValidator()
    {
        RuleFor(x => x).Must(d =>
            d.RetirementFileID.HasValue || d.RetireeID.HasValue ||
            d.EntitledPersonID.HasValue || d.AgentID.HasValue   ||
            d.GuardianID.HasValue       || d.PledgeID.HasValue  ||
            d.ExemptionID.HasValue      || d.InvestmentProjectID.HasValue ||
            d.InvestmentPropertyID.HasValue || d.PropertyUnitID.HasValue  ||
            d.LeaseContractID.HasValue  || d.TenantID.HasValue ||
            d.InvestmentPortfolioID.HasValue || d.MaintenanceRequestID.HasValue ||
            d.WorkOrderID.HasValue      || d.AssetValuationID.HasValue ||
            d.PropertyValuationID.HasValue || d.DepreciationRecordID.HasValue ||
            d.PaymentScheduleID.HasValue || d.OperationalExpenseID.HasValue ||
            d.InvestmentPlanID.HasValue || d.InvestmentTxnID.HasValue ||
            d.RiskAssessmentID.HasValue || d.EmployeeID.HasValue)
        .WithMessage("يجب تحديد مالك الوثيقة");
    }
}
