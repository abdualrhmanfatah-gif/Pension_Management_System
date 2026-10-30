<<<<<<< HEAD
using PensionMS.Infrastructure.Data;
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
using Microsoft.EntityFrameworkCore;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Investment;
using PensionMS.Core.Entities.Investment;
using PensionMS.Core.Interfaces;

namespace PensionMS.Infrastructure.Services.Investment;

public class InvestmentLeasingService : IInvestmentLeasingService
{
    private readonly IUnitOfWork _unitOfWork;
<<<<<<< HEAD
    private readonly IAccessiblePensionRepository _access;
    public InvestmentLeasingService(IUnitOfWork unitOfWork, IAccessiblePensionRepository access)
    {
        _unitOfWork = unitOfWork;
        _access = access;
=======
    public InvestmentLeasingService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    }

    // ----------------------------------------------------------------------
    // TENANTS
    // ----------------------------------------------------------------------

    public async Task<ApiResponse<int>> CreateTenantAsync(CreateTenantDto dto, int userId)
    {
<<<<<<< HEAD
        await _access.ForCurrentOperationAsync();
        // Tenant identity is globally unique; check existence without loading another tenant's profile.
        var existing = await _unitOfWork.Tenants.Query()
            .AnyAsync(t => t.NationalIDOrCR == dto.NationalIDOrCR && !t.IsDeleted);

        if (existing)
=======
        var existing = await _unitOfWork.Tenants.Query()
            .FirstOrDefaultAsync(t => t.NationalIDOrCR == dto.NationalIDOrCR && !t.IsDeleted);

        if (existing != null)
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            return ApiResponse<int>.Fail("يوجد مستأجر مسجل بنفس رقم الهوية/السجل التجاري");

        var tenant = new Tenant
        {
            FullName = dto.FullName,
            TenantType = dto.TenantType,
            NationalIDOrCR = dto.NationalIDOrCR,
            Phone = dto.Phone,
            RatingScore = dto.RatingScore,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Tenants.AddAsync(tenant);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<int>.Success(tenant.TenantID, "تم إنشاء المستأجر بنجاح");
    }

    public async Task<ApiResponse<bool>> UpdateTenantAsync(int id, UpdateTenantDto dto, int userId)
    {
<<<<<<< HEAD
        var queries = await _access.ForCurrentOperationAsync();
        var tenant = await TenantProfiles(queries).FirstOrDefaultAsync(t => t.TenantID == id);
=======
        var tenant = await _unitOfWork.Tenants.GetByIdAsync(id);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        if (tenant == null || tenant.IsDeleted)
            return ApiResponse<bool>.Fail("المستأجر غير موجود");

        var existing = await _unitOfWork.Tenants.Query()
<<<<<<< HEAD
            .AnyAsync(t => t.NationalIDOrCR == dto.NationalIDOrCR && t.TenantID != id && !t.IsDeleted);

        if (existing)
=======
            .FirstOrDefaultAsync(t => t.NationalIDOrCR == dto.NationalIDOrCR && t.TenantID != id && !t.IsDeleted);

        if (existing != null)
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            return ApiResponse<bool>.Fail("يوجد مستأجر آخر مسجل بنفس رقم الهوية/السجل التجاري");

        tenant.FullName = dto.FullName;
        tenant.TenantType = dto.TenantType;
        tenant.NationalIDOrCR = dto.NationalIDOrCR;
        tenant.Phone = dto.Phone;
        tenant.RatingScore = dto.RatingScore;
        tenant.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Tenants.Update(tenant);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.Success(true, "تم تحديث المستأجر بنجاح");
    }

    public async Task<ApiResponse<bool>> DeleteTenantAsync(int id, int userId)
    {
<<<<<<< HEAD
        var queries = await _access.ForCurrentOperationAsync();
        var tenant = await TenantProfiles(queries).FirstOrDefaultAsync(t => t.TenantID == id);
=======
        var tenant = await _unitOfWork.Tenants.GetByIdAsync(id);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        if (tenant == null || tenant.IsDeleted)
            return ApiResponse<bool>.Fail("المستأجر غير موجود");

        tenant.IsDeleted = true;
        tenant.DeletedAt = DateTime.UtcNow;
        tenant.DeletedByUserID = userId;

        _unitOfWork.Tenants.Update(tenant);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.Success(true, "تم حذف المستأجر بنجاح");
    }

    public async Task<ApiResponse<TenantDto>> GetTenantByIdAsync(int id)
    {
<<<<<<< HEAD
        var queries = await _access.ForCurrentOperationAsync();
        var contracts = queries.Query(_unitOfWork.LeaseContracts);
        var tenant = await TenantProfiles(queries).AsNoTracking()
            .Where(t => t.TenantID == id && !t.IsDeleted)
            .Select(t => new TenantDto
            {
                TenantID = t.TenantID,
                FullName = t.FullName,
                TenantType = t.TenantType,
                NationalIDOrCR = t.NationalIDOrCR,
                Phone = t.Phone,
                RatingScore = t.RatingScore,
                ActiveContractsCount = contracts.Count(c => c.TenantID == t.TenantID &&
                    c.Status == LeaseStatusConstants.Active && !c.IsDeleted)
            }).FirstOrDefaultAsync();
=======
        var tenant = await _unitOfWork.Tenants.Query()
            .Include(t => t.LeaseContracts.Where(c => c.Status == LeaseStatusConstants.Active && !c.IsDeleted))
            .FirstOrDefaultAsync(t => t.TenantID == id && !t.IsDeleted);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

        if (tenant == null)
            return ApiResponse<TenantDto>.Fail("المستأجر غير موجود");

<<<<<<< HEAD
        return ApiResponse<TenantDto>.Success(tenant);
=======
        var dto = new TenantDto
        {
            TenantID = tenant.TenantID,
            FullName = tenant.FullName,
            TenantType = tenant.TenantType,
            NationalIDOrCR = tenant.NationalIDOrCR,
            Phone = tenant.Phone,
            RatingScore = tenant.RatingScore,
            ActiveContractsCount = tenant.LeaseContracts.Count
        };

        return ApiResponse<TenantDto>.Success(dto);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    }

    public async Task<ApiResponse<List<TenantDto>>> GetAllTenantsAsync()
    {
<<<<<<< HEAD
        var queries = await _access.ForCurrentOperationAsync();
        var contracts = queries.Query(_unitOfWork.LeaseContracts);
        var tenants = await TenantProfiles(queries).AsNoTracking()
=======
        var tenants = await _unitOfWork.Tenants.Query()
            .Include(t => t.LeaseContracts.Where(c => c.Status == LeaseStatusConstants.Active && !c.IsDeleted))
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            .Where(t => !t.IsDeleted)
            .Select(t => new TenantDto
            {
                TenantID = t.TenantID,
                FullName = t.FullName,
                TenantType = t.TenantType,
                NationalIDOrCR = t.NationalIDOrCR,
                Phone = t.Phone,
                RatingScore = t.RatingScore,
<<<<<<< HEAD
                ActiveContractsCount = contracts.Count(c => c.TenantID == t.TenantID &&
                    c.Status == LeaseStatusConstants.Active && !c.IsDeleted)
=======
                ActiveContractsCount = t.LeaseContracts.Count
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            })
            .ToListAsync();

        return ApiResponse<List<TenantDto>>.Success(tenants);
    }

    public async Task<ApiResponse<List<PropertyDto>>> GetPropertiesByTenantAsync(int tenantId)
    {
<<<<<<< HEAD
        var queries = await _access.ForCurrentOperationAsync();
        if (!await CanReferenceTenantAsync(queries, tenantId))
            return ApiResponse<List<PropertyDto>>.Fail("المستأجر غير موجود");

        var properties = await queries.Query(_unitOfWork.LeaseContracts)
            .AsNoTracking()
=======
        var tenantExists = await _unitOfWork.Tenants.GetByIdAsync(tenantId);
        if (tenantExists == null || tenantExists.IsDeleted)
            return ApiResponse<List<PropertyDto>>.Fail("المستأجر غير موجود");

        var properties = await _unitOfWork.LeaseContracts.Query()
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            .Include(c => c.Unit)
                .ThenInclude(u => u.Property)
                    .ThenInclude(p => p.PropertyType)
            .Where(c => c.TenantID == tenantId && c.Status == LeaseStatusConstants.Active && !c.IsDeleted)
            .Select(c => c.Unit.Property)
            .Where(p => p != null && !p.IsDeleted)
            .Distinct()
            .ToListAsync();

        var result = properties.Select(p => new PropertyDto
        {
            PropertyID = p.PropertyID,
            ProjectID = p.ProjectID,
            PropertyTypeID = p.PropertyTypeID,
            PropertyTypeName = p.PropertyType?.TypeName ?? string.Empty,
            PropertyName = p.PropertyName,
            LocationCity = p.LocationCity,
            TotalArea = p.TotalArea,
            MainPhotoPath = p.MainPhotoPath
        }).ToList();

        return ApiResponse<List<PropertyDto>>.Success(result);
    }

    // ----------------------------------------------------------------------
    // LEASE CONTRACTS
    // ----------------------------------------------------------------------

<<<<<<< HEAD
    public Task<ApiResponse<int>> CreateLeaseContractAsync(CreateLeaseDto dto, int userId) =>
        UnitOfWorkTransaction.ExecuteAsync(_unitOfWork, () => CreateLeaseContractCoreAsync(dto, userId));

    private async Task<ApiResponse<int>> CreateLeaseContractCoreAsync(CreateLeaseDto dto, int userId)
    {
        var queries = await _access.ForCurrentOperationAsync();
        var unit = await queries.Query(_unitOfWork.PropertyUnits).FirstOrDefaultAsync(u => u.UnitID == dto.UnitID);
        if (unit == null || unit.IsDeleted)
            return ApiResponse<int>.Fail("الوحدة غير موجودة");

        if (!await CanReferenceTenantAsync(queries, dto.TenantID))
=======
    public async Task<ApiResponse<int>> CreateLeaseContractAsync(CreateLeaseDto dto, int userId)
    {
        var unit = await _unitOfWork.PropertyUnits.GetByIdAsync(dto.UnitID);
        if (unit == null || unit.IsDeleted)
            return ApiResponse<int>.Fail("الوحدة غير موجودة");

        var tenant = await _unitOfWork.Tenants.GetByIdAsync(dto.TenantID);
        if (tenant == null || tenant.IsDeleted)
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            return ApiResponse<int>.Fail("المستأجر غير موجود");

        if (dto.EndDate <= dto.StartDate)
            return ApiResponse<int>.Fail("تاريخ النهاية يجب أن يكون بعد تاريخ البداية");

        var contract = new LeaseContract
        {
            UnitID = dto.UnitID,
            TenantID = dto.TenantID,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            TotalContractValue = dto.TotalContractValue,
            PaymentFrequency = dto.PaymentFrequency,
            RenewalTerms = dto.RenewalTerms,
            SignedContractPath = dto.SignedContractPath,
            Status = dto.Status,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.LeaseContracts.AddAsync(contract);
        await _unitOfWork.SaveChangesAsync();

        // ─── ميزة توليد الدفعات تلقائياً ────────────────────────────────
        if (dto.GenerateSchedules)
        {
            await GenerateSchedulesAsync(contract);
        }

        return ApiResponse<int>.Success(contract.ContractID, "تم إنشاء عقد الإيجار وجدولة الدفعات بنجاح");
    }

    private async Task GenerateSchedulesAsync(LeaseContract contract)
    {
        var schedules = new List<PaymentSchedule>();
        int monthsStep = contract.PaymentFrequency switch
        {
            PaymentFrequencyConstants.Monthly => 1,
            PaymentFrequencyConstants.Quarterly => 3,
            PaymentFrequencyConstants.SemiAnnually => 6,
            PaymentFrequencyConstants.Annually => 12,
            _ => 12
        };

        var currentDate = contract.StartDate;
        int totalPeriods = 0;
        
        // حساب عدد الفترات
        var tempDate = contract.StartDate;
        while (tempDate < contract.EndDate)
        {
            totalPeriods++;
            tempDate = tempDate.AddMonths(monthsStep);
        }

        if (totalPeriods == 0) totalPeriods = 1;
        decimal totalAllocatedAmount = 0m;
        decimal amountPerPeriod = Math.Round(contract.TotalContractValue / totalPeriods, 2);

        for (int i = 0; i < totalPeriods; i++)
        {
            decimal currentPeriodAmount;
            if (i == totalPeriods - 1)
            {
                currentPeriodAmount = contract.TotalContractValue - totalAllocatedAmount;
            }
            else
            {
                currentPeriodAmount = amountPerPeriod;
                totalAllocatedAmount += currentPeriodAmount;
            }

            schedules.Add(new PaymentSchedule
            {
                ContractID = contract.ContractID,
                DueDate = currentDate,
                ExpectedAmount = currentPeriodAmount,
                PaidAmount = 0,
                Status = LookupConstants.Pending,
                CreatedAt = DateTime.UtcNow
            });
            currentDate = currentDate.AddMonths(monthsStep);
        }

        foreach (var s in schedules)
        {
            await _unitOfWork.PaymentSchedules.AddAsync(s);
        }
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<ApiResponse<bool>> UpdateLeaseContractAsync(int id, UpdateLeaseDto dto, int userId)
    {
<<<<<<< HEAD
        var queries = await _access.ForCurrentOperationAsync();
        var contract = await queries.Query(_unitOfWork.LeaseContracts).FirstOrDefaultAsync(c => c.ContractID == id);
=======
        var contract = await _unitOfWork.LeaseContracts.GetByIdAsync(id);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        if (contract == null || contract.IsDeleted)
            return ApiResponse<bool>.Fail("العقد غير موجود");

        if (dto.EndDate <= dto.StartDate)
            return ApiResponse<bool>.Fail("تاريخ النهاية يجب أن يكون بعد تاريخ البداية");

<<<<<<< HEAD
        if (!await queries.Query(_unitOfWork.PropertyUnits).AnyAsync(u => u.UnitID == dto.UnitID && !u.IsDeleted))
            return ApiResponse<bool>.Fail("الوحدة غير موجودة");
        if (!await CanReferenceTenantAsync(queries, dto.TenantID))
            return ApiResponse<bool>.Fail("المستأجر غير موجود");

=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        contract.UnitID = dto.UnitID;
        contract.TenantID = dto.TenantID;
        contract.StartDate = dto.StartDate;
        contract.EndDate = dto.EndDate;
        contract.TotalContractValue = dto.TotalContractValue;
        contract.PaymentFrequency = dto.PaymentFrequency;
        contract.RenewalTerms = dto.RenewalTerms;
        contract.SignedContractPath = dto.SignedContractPath;
        contract.Status = dto.Status;
        contract.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.LeaseContracts.Update(contract);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.Success(true, "تم تحديث عقد الإيجار بنجاح");
    }

    public async Task<ApiResponse<bool>> DeleteLeaseContractAsync(int id, int userId)
    {
<<<<<<< HEAD
        var queries = await _access.ForCurrentOperationAsync();
        var contract = await queries.Query(_unitOfWork.LeaseContracts).FirstOrDefaultAsync(c => c.ContractID == id);
=======
        var contract = await _unitOfWork.LeaseContracts.GetByIdAsync(id);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        if (contract == null || contract.IsDeleted)
            return ApiResponse<bool>.Fail("العقد غير موجود");

        contract.IsDeleted = true;
        contract.DeletedAt = DateTime.UtcNow;
        contract.DeletedByUserID = userId;

        _unitOfWork.LeaseContracts.Update(contract);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.Success(true, "تم حذف العقد بنجاح");
    }

    public async Task<ApiResponse<LeaseContractDto>> GetLeaseContractByIdAsync(int id)
    {
<<<<<<< HEAD
        var queries = await _access.ForCurrentOperationAsync();
        var contract = await queries.Query(_unitOfWork.LeaseContracts)
            .AsNoTracking()
            .Where(c => c.ContractID == id && !c.IsDeleted)
            .Select(c => new LeaseContractDto
            {
                ContractID = c.ContractID,
                UnitID = c.UnitID,
                UnitNumber = c.Unit != null ? c.Unit.UnitNumber : string.Empty,
                PropertyName = c.Unit != null && c.Unit.Property != null ? c.Unit.Property.PropertyName : string.Empty,
                TenantID = c.TenantID,
                TenantName = c.Tenant != null ? c.Tenant.FullName : string.Empty,
                StartDate = c.StartDate.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
                EndDate = c.EndDate.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
                TotalContractValue = c.TotalContractValue,
                PaymentFrequency = c.PaymentFrequency,
                Status = c.Status,
                SignedContractPath = c.SignedContractPath
            }).FirstOrDefaultAsync();
=======
        var contract = await _unitOfWork.LeaseContracts.Query()
            .Include(c => c.Unit).ThenInclude(u => u.Property)
            .Include(c => c.Tenant)
            .FirstOrDefaultAsync(c => c.ContractID == id && !c.IsDeleted);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

        if (contract == null)
            return ApiResponse<LeaseContractDto>.Fail("العقد غير موجود");

<<<<<<< HEAD
        return ApiResponse<LeaseContractDto>.Success(contract);
=======
        var dto = new LeaseContractDto
        {
            ContractID = contract.ContractID,
            UnitID = contract.UnitID,
            UnitNumber = contract.Unit?.UnitNumber ?? string.Empty,
            PropertyName = contract.Unit?.Property?.PropertyName ?? string.Empty,
            TenantID = contract.TenantID,
            TenantName = contract.Tenant?.FullName ?? string.Empty,
            StartDate = contract.StartDate.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
            EndDate = contract.EndDate.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
            TotalContractValue = contract.TotalContractValue,
            PaymentFrequency = contract.PaymentFrequency,
            Status = contract.Status,
            SignedContractPath = contract.SignedContractPath
        };

        return ApiResponse<LeaseContractDto>.Success(dto);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    }

    public async Task<ApiResponse<List<LeaseContractDto>>> GetLeasesByPropertyAsync(int propertyId)
    {
<<<<<<< HEAD
        var queries = await _access.ForCurrentOperationAsync();
        var contracts = await queries.Query(_unitOfWork.LeaseContracts)
            .AsNoTracking()
=======
        var contracts = await _unitOfWork.LeaseContracts.Query()
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            .Include(c => c.Unit).ThenInclude(u => u.Property)
            .Include(c => c.Tenant)
            .Where(c => c.Unit.PropertyID == propertyId && !c.IsDeleted)
            .Select(c => new LeaseContractDto
            {
                ContractID = c.ContractID,
                UnitID = c.UnitID,
                UnitNumber = c.Unit.UnitNumber,
                PropertyName = c.Unit.Property.PropertyName,
                TenantID = c.TenantID,
                TenantName = c.Tenant != null ? c.Tenant.FullName : string.Empty,
                StartDate = c.StartDate.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
                EndDate = c.EndDate.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
                TotalContractValue = c.TotalContractValue,
                PaymentFrequency = c.PaymentFrequency,
                Status = c.Status,
                SignedContractPath = c.SignedContractPath
            })
            .ToListAsync();

        return ApiResponse<List<LeaseContractDto>>.Success(contracts);
    }

    public async Task<ApiResponse<List<LeaseContractDto>>> GetAllLeasesAsync()
    {
<<<<<<< HEAD
        var queries = await _access.ForCurrentOperationAsync();
        var contracts = await queries.Query(_unitOfWork.LeaseContracts)
            .AsNoTracking()
=======
        var contracts = await _unitOfWork.LeaseContracts.Query()
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            .Include(c => c.Unit).ThenInclude(u => u.Property)
            .Include(c => c.Tenant)
            .Where(c => !c.IsDeleted)
            .Select(c => new LeaseContractDto
            {
                ContractID = c.ContractID,
                UnitID = c.UnitID,
                UnitNumber = c.Unit.UnitNumber,
                PropertyName = c.Unit.Property.PropertyName,
                TenantID = c.TenantID,
                TenantName = c.Tenant != null ? c.Tenant.FullName : string.Empty,
                StartDate = c.StartDate.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
                EndDate = c.EndDate.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
                TotalContractValue = c.TotalContractValue,
                PaymentFrequency = c.PaymentFrequency,
                Status = c.Status,
                SignedContractPath = c.SignedContractPath
            })
            .ToListAsync();

        return ApiResponse<List<LeaseContractDto>>.Success(contracts);
    }

    public async Task<ApiResponse<bool>> UpdateLeaseContractFilePathAsync(int id, string filePath, int userId)
    {
<<<<<<< HEAD
        var queries = await _access.ForCurrentOperationAsync();
        var contract = await queries.Query(_unitOfWork.LeaseContracts).FirstOrDefaultAsync(c => c.ContractID == id);
=======
        var contract = await _unitOfWork.LeaseContracts.GetByIdAsync(id);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        if (contract == null || contract.IsDeleted)
            return ApiResponse<bool>.Fail("العقد غير موجود");

        contract.SignedContractPath = filePath;
        contract.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.LeaseContracts.Update(contract);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.Success(true, "تم تحديث مسار العقد بنجاح");
    }

    // ----------------------------------------------------------------------
    // PAYMENT SCHEDULES
    // ----------------------------------------------------------------------

    public async Task<ApiResponse<int>> CreatePaymentScheduleAsync(CreatePaymentScheduleDto dto, int userId)
    {
<<<<<<< HEAD
        var queries = await _access.ForCurrentOperationAsync();
        var contract = await queries.Query(_unitOfWork.LeaseContracts).FirstOrDefaultAsync(c => c.ContractID == dto.ContractID);
=======
        var contract = await _unitOfWork.LeaseContracts.GetByIdAsync(dto.ContractID);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        if (contract == null || contract.IsDeleted)
            return ApiResponse<int>.Fail("العقد غير موجود");

        var schedule = new PaymentSchedule
        {
            ContractID = dto.ContractID,
            DueDate = dto.DueDate,
            ExpectedAmount = dto.ExpectedAmount,
            PaidAmount = 0,
            Status = LookupConstants.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.PaymentSchedules.AddAsync(schedule);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<int>.Success(schedule.ScheduleID, "تم إنشاء دفعة الإيجار بنجاح");
    }

    public async Task<ApiResponse<bool>> UpdatePaymentStatusAsync(int scheduleId, UpdatePaymentStatusDto dto, int userId)
    {
<<<<<<< HEAD
        var queries = await _access.ForCurrentOperationAsync();
        var schedule = await queries.Query(_unitOfWork.PaymentSchedules).FirstOrDefaultAsync(s => s.ScheduleID == scheduleId);
=======
        var schedule = await _unitOfWork.PaymentSchedules.GetByIdAsync(scheduleId);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        if (schedule == null || schedule.IsDeleted)
            return ApiResponse<bool>.Fail("الدفعة غير موجودة");

        schedule.PaidAmount = dto.PaidAmount;
        schedule.Status = dto.Status;
        schedule.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.PaymentSchedules.Update(schedule);
        await _unitOfWork.SaveChangesAsync();



        return ApiResponse<bool>.Success(true, "تم تحديث حالة الدفعة بنجاح");
    }

    public async Task<ApiResponse<List<PaymentScheduleDto>>> GetSchedulesByContractAsync(int contractId)
    {
<<<<<<< HEAD
        var queries = await _access.ForCurrentOperationAsync();
        var schedules = await queries.Query(_unitOfWork.PaymentSchedules)
            .AsNoTracking()
=======
        var schedules = await _unitOfWork.PaymentSchedules.Query()
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            .Where(s => s.ContractID == contractId && !s.IsDeleted)
            .OrderBy(s => s.DueDate)
            .Select(s => new PaymentScheduleDto
            {
                ScheduleID = s.ScheduleID,
                ContractID = s.ContractID,
                DueDate = s.DueDate.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
                ExpectedAmount = s.ExpectedAmount,
                PaidAmount = s.PaidAmount,
                Status = s.Status
            })
            .ToListAsync();

        return ApiResponse<List<PaymentScheduleDto>>.Success(schedules);
    }

    public async Task<ApiResponse<List<PaymentScheduleDto>>> GetAllSchedulesAsync()
    {
<<<<<<< HEAD
        var queries = await _access.ForCurrentOperationAsync();
        var schedules = await queries.Query(_unitOfWork.PaymentSchedules)
            .AsNoTracking()
=======
        var schedules = await _unitOfWork.PaymentSchedules.Query()
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            .Where(s => !s.IsDeleted)
            .OrderByDescending(s => s.DueDate)
            .Select(s => new PaymentScheduleDto
            {
                ScheduleID = s.ScheduleID,
                ContractID = s.ContractID,
                DueDate = s.DueDate.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
                ExpectedAmount = s.ExpectedAmount,
                PaidAmount = s.PaidAmount,
                Status = s.Status
            })
            .ToListAsync();

        return ApiResponse<List<PaymentScheduleDto>>.Success(schedules);
    }
<<<<<<< HEAD

    private IQueryable<Tenant> TenantProfiles(IPensionDataQuery queries)
    {
        var scopedIds = queries.Query(_unitOfWork.Tenants).Select(t => t.TenantID);
        // No branch/creator owner exists for unlinked tenants. Preserve that workflow pending a business decision.
        // Linked shared profiles retain the existing all-associated-branches scope rule.
        return _unitOfWork.Tenants.Query().Where(t => !t.LeaseContracts.Any() || scopedIds.Contains(t.TenantID));
    }

    private Task<bool> CanReferenceTenantAsync(IPensionDataQuery queries, int tenantId)
    {
        var contracts = queries.Query(_unitOfWork.LeaseContracts);
        // Referencing a party on an authorized lease does not grant access to its other leases or shared profile.
        return _unitOfWork.Tenants.Query().AnyAsync(t => t.TenantID == tenantId && !t.IsDeleted &&
            (!t.LeaseContracts.Any() || contracts.Any(c => c.TenantID == t.TenantID)));
    }
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
}
