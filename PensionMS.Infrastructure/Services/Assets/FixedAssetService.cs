using Microsoft.EntityFrameworkCore;
using PensionMS.Core.DTOs.Assets;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Accounting;
using PensionMS.Core.Entities.Assets;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Interfaces.Accounting;
using PensionMS.Core.Interfaces.Services;
using PensionMS.Infrastructure.Data;
using PensionMS.Infrastructure.Data.Seeders.Assets;

namespace PensionMS.Infrastructure.Services.Assets;

public class FixedAssetService : IFixedAssetService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IImmutableAuditService _auditService;
    private readonly IJournalEntryService _journalEntry;
    private readonly PensionDbContext _dbContext;

    public FixedAssetService(IUnitOfWork unitOfWork, IImmutableAuditService auditService, IJournalEntryService journalEntry, PensionDbContext dbContext)
    {
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _journalEntry = journalEntry;
        _dbContext = dbContext;
    }

    private async Task<int> GetJournalEntryTypeIDAsync(string code)
    {
        var entryType = await _unitOfWork.JournalEntryTypes.Query()
            .FirstOrDefaultAsync(t => t.Code == code && !t.IsDeleted);
        return entryType?.JournalEntryTypeID ?? 1;
    }

    public async Task<ApiResponse<List<FixedAssetDto>>> GetAllAsync()
    {
        var assets = await _unitOfWork.FixedAssets.Query()
            .AsNoTracking()
            .Include(a => a.Category)
            .Include(a => a.AssetType)
            .Include(a => a.CustodianEmployee)
            .Include(a => a.AssetAccount)
            .Include(a => a.ITDetail)
            .Include(a => a.VehicleDetail)
            .Include(a => a.BuildingDetail)
            .Include(a => a.LandDetail)
            .Include(a => a.FurnitureDetail)
            .OrderBy(a => a.AssetNumber)
            .Select(a => MapToDto(a))
            .ToListAsync();

        return ApiResponse<List<FixedAssetDto>>.Success(assets);
    }

    public async Task<ApiResponse<FixedAssetDto>> GetByIdAsync(int id)
    {
        var asset = await _unitOfWork.FixedAssets.Query()
            .AsNoTracking()
            .Include(a => a.Category)
            .Include(a => a.AssetType)
            .Include(a => a.CustodianEmployee)
            .Include(a => a.AssetAccount)
            .Include(a => a.ITDetail)
            .Include(a => a.VehicleDetail)
            .Include(a => a.BuildingDetail)
            .Include(a => a.LandDetail)
            .Include(a => a.FurnitureDetail)
            .FirstOrDefaultAsync(a => a.AssetID == id);

        if (asset == null)
            return ApiResponse<FixedAssetDto>.Fail("الأصل غير موجود");

        return ApiResponse<FixedAssetDto>.Success(MapToDto(asset));
    }

    public async Task<ApiResponse<List<FixedAssetDto>>> GetByTypeAsync(int typeId)
    {
        var assets = await _unitOfWork.FixedAssets.Query()
            .AsNoTracking()
            .Include(a => a.Category)
            .Include(a => a.AssetType)
            .Include(a => a.CustodianEmployee)
            .Include(a => a.AssetAccount)
            .Include(a => a.ITDetail)
            .Include(a => a.VehicleDetail)
            .Include(a => a.BuildingDetail)
            .Include(a => a.LandDetail)
            .Include(a => a.FurnitureDetail)
            .Where(a => a.AssetTypeID == typeId)
            .OrderBy(a => a.AssetNumber)
            .Select(a => MapToDto(a))
            .ToListAsync();

        return ApiResponse<List<FixedAssetDto>>.Success(assets);
    }

    public async Task<ApiResponse<List<AssetTypeDto>>> GetTypesAsync()
    {
        var types = await _dbContext.FixedAssetTypes
            .AsNoTracking()
            .OrderBy(t => t.TypeName)
            .Select(t => new AssetTypeDto
            {
                AssetTypeID = t.AssetTypeID,
                TypeName = t.TypeName,
                TypeCode = t.TypeCode,
                Description = t.Description,
                IsActive = t.IsActive,
                AssetCount = t.Assets.Count
            })
            .ToListAsync();

        return ApiResponse<List<AssetTypeDto>>.Success(types);
    }

    public async Task<ApiResponse<List<AssetCategoryDto>>> GetCategoriesAsync(int? typeId = null)
    {
        var query = _dbContext.FixedAssetCategories
            .AsNoTracking()
            .Include(c => c.AssetType)
            .AsQueryable();

        if (typeId.HasValue)
            query = query.Where(c => c.AssetTypeID == typeId.Value);

        var categories = await query
            .OrderBy(c => c.CategoryName)
            .Select(c => new AssetCategoryDto
            {
                CategoryID = c.CategoryID,
                CategoryName = c.CategoryName,
                CategoryCode = c.CategoryCode,
                Description = c.Description,
                AssetTypeID = c.AssetTypeID,
                AssetTypeName = c.AssetType.TypeName,
                DepreciationRate = c.DepreciationRate,
                SalvagePercent = c.SalvagePercent,
                AssetAccountID = c.AssetAccountID,
                AssetAccountName = c.AssetAccount != null ? c.AssetAccount.AccountName : null,
                AssetAccountCode = c.AssetAccount != null ? c.AssetAccount.AccountCode : null,
                AccumDeprAccountID = c.AccumDeprAccountID,
                AccumDeprAccountName = c.AccumDeprAccount != null ? c.AccumDeprAccount.AccountName : null,
                DeprExpenseAccountID = c.DeprExpenseAccountID,
                DeprExpenseAccountName = c.DeprExpenseAccount != null ? c.DeprExpenseAccount.AccountName : null,
                IsActive = c.IsActive,
                AssetCount = c.Assets.Count
            })
            .ToListAsync();

        return ApiResponse<List<AssetCategoryDto>>.Success(categories);
    }

<<<<<<< HEAD
    public Task<ApiResponse<FixedAssetDto>> CreateAsync(CreateFixedAssetDto dto, int userId) =>
        UnitOfWorkTransaction.ExecuteAsync(_unitOfWork, () => CreateCoreAsync(dto, userId));

    private async Task<ApiResponse<FixedAssetDto>> CreateCoreAsync(CreateFixedAssetDto dto, int userId)
=======
    public async Task<ApiResponse<FixedAssetDto>> CreateAsync(CreateFixedAssetDto dto, int userId)
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    {
        // توليد رقم الأصل
        var assetNumberResponse = await GenerateAssetNumberAsync(dto.AssetTypeID);
        var assetNumber = assetNumberResponse.Data ?? "AST-00001";

        // حساب القيمة الدفترية الأولية
        var netBookValue = dto.AcquiredCost;

        // تعبئة الحسابات المحاسبية من التصنيف إذا لم تكن محددة
        var assetAccountId = dto.AssetAccountID;
        var accumDeprAccountId = dto.AccumDeprAccountID;
        var deprExpAccountId = dto.DeprExpenseAccountID;

        if (assetAccountId == 0 || accumDeprAccountId == 0 || deprExpAccountId == 0)
        {
            var category = await _dbContext.FixedAssetCategories.FindAsync(dto.CategoryID);
            if (category != null)
            {
                if (assetAccountId == 0) assetAccountId = category.AssetAccountID ?? 0;
                if (accumDeprAccountId == 0) accumDeprAccountId = category.AccumDeprAccountID ?? 0;
                if (deprExpAccountId == 0) deprExpAccountId = category.DeprExpenseAccountID ?? 0;
            }
        }

        // حساب التكلفة الإجمالية تلقائياً
        var acquiredCost = dto.UnitPrice * dto.Quantity;

        var asset = new FixedAsset
        {
            AssetNumber = assetNumber,
            AssetName = dto.AssetName,
            Description = dto.Description,
            CategoryID = dto.CategoryID,
            AssetTypeID = dto.AssetTypeID,
            SerialNumber = dto.SerialNumber,
            PurchaseDate = dto.PurchaseDate,
            InServiceDate = dto.InServiceDate,
            UnitPrice = dto.UnitPrice,
            AcquiredCost = acquiredCost,
            SalvageValue = dto.SalvageValue,
            CurrencyCode = dto.CurrencyCode,
            Quantity = dto.Quantity,
            DepreciationRate = dto.DepreciationRate,
            ValuationMethod = dto.ValuationMethod,
            ValuationNotes = dto.ValuationNotes,
            AccumulatedDepreciation = dto.InitialAccumulatedDepreciation,
            NetBookValue = acquiredCost - dto.InitialAccumulatedDepreciation,
            Location = dto.Location,
            CustodianEmployeeID = dto.CustodianEmployeeID,
            AssetAccountID = assetAccountId,
            AccumDeprAccountID = accumDeprAccountId,
            DeprExpenseAccountID = deprExpAccountId,
            Status = AssetStatus.Active,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserID = userId
        };

        await _unitOfWork.FixedAssets.AddAsync(asset);
        await _unitOfWork.SaveChangesAsync();

        // إضافة التفاصيل حسب النوع
        if (dto.ITDetail != null && dto.AssetTypeID == 0) // IT
        {
            var d = dto.ITDetail;
            _dbContext.AssetITDetails.Add(new AssetITDetail
            {
                AssetID = asset.AssetID, Model = d.Model, Manufacturer = d.Manufacturer,
                ProcessorType = d.ProcessorType, ProcessorSpeed = d.ProcessorSpeed,
                RamSize = d.RamSize, HardDiskType = d.HardDiskType, HardDiskSize = d.HardDiskSize,
                GpuModel = d.GpuModel, ScreenSize = d.ScreenSize, ScreenResolution = d.ScreenResolution,
                OperatingSystem = d.OperatingSystem, HasWebcam = d.HasWebcam, HasWifi = d.HasWifi,
                HasBluetooth = d.HasBluetooth, IpAddress = d.IpAddress, MacAddress = d.MacAddress,
                WarrantyExpiry = d.WarrantyExpiry, Notes = d.Notes
            });
        }
        if (dto.VehicleDetail != null && dto.AssetTypeID == 1) // Vehicle
        {
            var d = dto.VehicleDetail;
            _dbContext.AssetVehicleDetails.Add(new AssetVehicleDetail
            {
                AssetID = asset.AssetID, PlateNumber = d.PlateNumber,
                PlateLetter1 = d.PlateLetter1, PlateLetter2 = d.PlateLetter2, PlateLetter3 = d.PlateLetter3,
                EngineNumber = d.EngineNumber, ChassisNumber = d.ChassisNumber,
                TransmissionType = d.TransmissionType, FuelType = d.FuelType,
                CylinderCount = d.CylinderCount, EngineCapacity = d.EngineCapacity,
                HorsePower = d.HorsePower, WheelCount = d.WheelCount, TireSize = d.TireSize,
                Color = d.Color, ModelYear = d.ModelYear, RegistrationExpiry = d.RegistrationExpiry,
                InsuranceExpiry = d.InsuranceExpiry, Mileage = d.Mileage, HasGps = d.HasGps, Notes = d.Notes
            });
        }
        if (dto.BuildingDetail != null && dto.AssetTypeID == 2) // Building
        {
            var d = dto.BuildingDetail;
            _dbContext.AssetBuildingDetails.Add(new AssetBuildingDetail
            {
                AssetID = asset.AssetID, AreaSqm = d.AreaSqm, BuiltAreaSqm = d.BuiltAreaSqm,
                FloorCount = d.FloorCount, OwnershipDocNumber = d.OwnershipDocNumber,
                LicenseNumber = d.LicenseNumber, Notes = d.Notes
            });
        }
        if (dto.LandDetail != null && dto.AssetTypeID == 3) // Land
        {
            var d = dto.LandDetail;
            _dbContext.AssetLandDetails.Add(new AssetLandDetail
            {
                AssetID = asset.AssetID, PlotNumber = d.PlotNumber, LandAreaSqm = d.LandAreaSqm,
                OwnershipDocNumber = d.OwnershipDocNumber, OwnershipDocDate = d.OwnershipDocDate, Notes = d.Notes
            });
        }
        if (dto.FurnitureDetail != null && dto.AssetTypeID == 4) // Furniture
        {
            var d = dto.FurnitureDetail;
            _dbContext.AssetFurnitureDetails.Add(new AssetFurnitureDetail
            {
                AssetID = asset.AssetID, Material = d.Material, Color = d.Color, Notes = d.Notes
            });
        }

        await _dbContext.SaveChangesAsync();

<<<<<<< HEAD
=======
        await _auditService.LogAsync(userId, "", "Create", "FixedAsset", asset.AssetID, null, System.Text.Json.JsonSerializer.Serialize(dto));
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

        // إنشاء القيد المحاسبي: مدين حساب الأصل / دائن الخزينة
        if (!dto.SkipJournalEntry && assetAccountId > 0 && acquiredCost > 0)
        {
            var periodId = await _journalEntry.GetPeriodIdForDateAsync(dto.PurchaseDate);
            if (periodId <= 0)
            {
<<<<<<< HEAD
=======
                // حذف الأصل المُنشأ لأن القيد لا يمكن إنشاؤه
                _dbContext.FixedAssets.Remove(asset);
                await _dbContext.SaveChangesAsync();
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                return ApiResponse<FixedAssetDto>.Fail($"لا توجد فترة مالية مفتوحة بتاريخ الشراء ({dto.PurchaseDate:yyyy-MM-dd}). أنشئ السنة المالية أولاً.");
            }

            // حساب الخزينة الرئيسية (1811)
            var treasuryAccount = await _unitOfWork.ChartOfAccounts.Query()
                .FirstOrDefaultAsync(a => a.AccountCode == "1811" && !a.IsDeleted);

            if (treasuryAccount == null)
            {
<<<<<<< HEAD
=======
                _dbContext.FixedAssets.Remove(asset);
                await _dbContext.SaveChangesAsync();
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                return ApiResponse<FixedAssetDto>.Fail("حساب الخزينة الرئيسية (1811) غير موجود في الدليل المحاسبي.");
            }

            var fixedAssetTypeId = await GetJournalEntryTypeIDAsync("AST");

            var journalResult = await _journalEntry.CreateJournalEntryAsync(new CreateJournalEntryDto
            {
                EntryDate = dto.PurchaseDate,
                PeriodID = periodId,
                JournalEntryTypeID = fixedAssetTypeId,
                SourceReferenceID = asset.AssetID,
                Description = $"شراء أصل ثابت: {dto.AssetName} — رقم {assetNumber}",
                Lines = new List<CreateJournalEntryLineDto>
                {
                    new() { AccountID = assetAccountId, Debit = acquiredCost, Credit = 0, Description = dto.AssetName },
                    new() { AccountID = treasuryAccount.AccountID, Debit = 0, Credit = acquiredCost, Description = "الخزينة الرئيسية" }
                }
            }, userId);
<<<<<<< HEAD
            if (!journalResult.IsSuccess)
                return ApiResponse<FixedAssetDto>.Fail(journalResult.Message ?? "فشل إنشاء قيد الأصل");
        }

        await _auditService.LogAsync(userId, "", "Create", "FixedAsset", asset.AssetID, null, System.Text.Json.JsonSerializer.Serialize(dto));
=======
        }

>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        return await GetByIdAsync(asset.AssetID);
    }

    public async Task<ApiResponse<FixedAssetDto>> UpdateAsync(int id, UpdateFixedAssetDto dto, int userId)
    {
        var asset = await _unitOfWork.FixedAssets.GetByIdAsync(id);
        if (asset == null)
            return ApiResponse<FixedAssetDto>.Fail("الأصل غير موجود");

        var oldValues = System.Text.Json.JsonSerializer.Serialize(asset);

        if (dto.AssetName != null) asset.AssetName = dto.AssetName;
        if (dto.Description != null) asset.Description = dto.Description;
        if (dto.CategoryID.HasValue) asset.CategoryID = dto.CategoryID.Value;
        if (dto.AssetTypeID.HasValue) asset.AssetTypeID = dto.AssetTypeID.Value;
        if (dto.SerialNumber != null) asset.SerialNumber = dto.SerialNumber;
        if (dto.PurchaseDate.HasValue) asset.PurchaseDate = dto.PurchaseDate.Value;
        if (dto.InServiceDate.HasValue) asset.InServiceDate = dto.InServiceDate.Value;
        if (dto.SalvageValue.HasValue) asset.SalvageValue = dto.SalvageValue.Value;
        if (dto.DepreciationRate.HasValue) asset.DepreciationRate = dto.DepreciationRate.Value;
        if (dto.ValuationMethod.HasValue) asset.ValuationMethod = dto.ValuationMethod.Value;
        if (dto.ValuationNotes != null) asset.ValuationNotes = dto.ValuationNotes;
        if (dto.Location != null) asset.Location = dto.Location;
        if (dto.CustodianEmployeeID.HasValue) asset.CustodianEmployeeID = dto.CustodianEmployeeID;
        if (dto.AssetAccountID.HasValue) asset.AssetAccountID = dto.AssetAccountID.Value;
        if (dto.AccumDeprAccountID.HasValue) asset.AccumDeprAccountID = dto.AccumDeprAccountID.Value;
        if (dto.DeprExpenseAccountID.HasValue) asset.DeprExpenseAccountID = dto.DeprExpenseAccountID.Value;

        // تحديث سعر الوحدة والكمية وإعادة حساب التكلفة
        if (dto.UnitPrice.HasValue) asset.UnitPrice = dto.UnitPrice.Value;
        if (dto.Quantity.HasValue) asset.Quantity = dto.Quantity.Value;
        if (dto.UnitPrice.HasValue || dto.Quantity.HasValue)
        {
            asset.AcquiredCost = asset.UnitPrice * asset.Quantity;
            asset.NetBookValue = asset.AcquiredCost - asset.AccumulatedDepreciation;
        }
        if (dto.AcquiredCost.HasValue)
        {
            asset.AcquiredCost = dto.AcquiredCost.Value;
            asset.NetBookValue = dto.AcquiredCost.Value - asset.AccumulatedDepreciation;
        }
        if (dto.Status.HasValue)
        {
            asset.Status = dto.Status.Value;
            if (dto.Status.Value == AssetStatus.Disposed)
            {
                asset.DisposalDate = DateTime.UtcNow;
                asset.DisposalReason = dto.DisposalReason;
            }
        }

        asset.UpdatedAt = DateTime.UtcNow;
        asset.UpdatedByUserID = userId;

<<<<<<< HEAD
=======
        _unitOfWork.FixedAssets.Update(asset);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        await _unitOfWork.SaveChangesAsync();

        // تحديث التفاصيل
        await UpdateDetailsAsync(asset.AssetID, asset.AssetTypeID, dto);

        await _auditService.LogAsync(userId, "", "Update", "FixedAsset", id, oldValues, System.Text.Json.JsonSerializer.Serialize(dto));

        return await GetByIdAsync(id);
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id, int userId)
    {
        var asset = await _unitOfWork.FixedAssets.GetByIdAsync(id);
        if (asset == null)
            return ApiResponse<bool>.Fail("الأصل غير موجود");

        // التحقق من عدم وجود هلاك مسجل
        var hasDepreciation = await _dbContext.DepreciationSchedules.AnyAsync(d => d.AssetID == id);
        if (hasDepreciation)
            return ApiResponse<bool>.Fail("لا يمكن حذف أصل مسجل له هلاك");

        // التحقق من عدم وجود عهد نشطة مرتبطة
        var hasActiveCustody = await _dbContext.InventoryCustodies
            .AnyAsync(c => c.AssetID == id && c.Status == Core.Entities.Inventory.CustodyStatus.Active && !c.IsDeleted);
        if (hasActiveCustody)
            return ApiResponse<bool>.Fail("لا يمكن حذف الأصل لوجود عهد نشطة مرتبطة به");

<<<<<<< HEAD
        asset.IsDeleted = true;
        asset.DeletedAt = DateTime.UtcNow;
        asset.DeletedByUserID = userId;
=======
        _unitOfWork.FixedAssets.Delete(asset);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        await _unitOfWork.SaveChangesAsync();

        await _auditService.LogAsync(userId, "", "Delete", "FixedAsset", id, null, null);

        return ApiResponse<bool>.Success(true, "تم حذف الأصل بنجاح");
    }

    public async Task<ApiResponse<string>> GenerateAssetNumberAsync(int assetTypeId)
    {
        var type = await _dbContext.FixedAssetTypes.FindAsync(assetTypeId);
        var prefix = type?.TypeCode ?? "AST";

        var lastNumber = await _dbContext.FixedAssets
            .Where(a => a.AssetNumber.StartsWith(prefix))
            .OrderByDescending(a => a.AssetNumber)
            .Select(a => a.AssetNumber)
            .FirstOrDefaultAsync();

        int seq = 1;
        if (!string.IsNullOrEmpty(lastNumber))
        {
            var parts = lastNumber.Split('-');
            if (parts.Length > 1 && int.TryParse(parts[^1], out var last))
                seq = last + 1;
        }

        return ApiResponse<string>.Success($"{prefix}-{seq:D5}", $"Generated asset number: {prefix}-{seq:D5}");
    }

    public async Task<ApiResponse<List<DepreciationScheduleDto>>> GetScheduleAsync(int assetId)
    {
        var schedule = await _dbContext.DepreciationSchedules
            .AsNoTracking()
            .Where(d => d.AssetID == assetId)
            .OrderBy(d => d.PeriodYear).ThenBy(d => d.PeriodMonth)
            .Select(d => new DepreciationScheduleDto
            {
                ScheduleID = d.ScheduleID,
                AssetID = d.AssetID,
                PeriodYear = d.PeriodYear,
                PeriodMonth = d.PeriodMonth,
                PeriodStartDate = d.PeriodStartDate,
                PeriodEndDate = d.PeriodEndDate,
                DepreciationAmount = d.DepreciationAmount,
                AccumulatedDepreciation = d.AccumulatedDepreciation,
                NetBookValue = d.NetBookValue,
                IsPosted = d.IsPosted,
                PostedDate = d.PostedDate,
                CreatedAt = d.CreatedAt
            })
            .ToListAsync();

        return ApiResponse<List<DepreciationScheduleDto>>.Success(schedule);
    }

    public async Task<ApiResponse<bool>> RunYearlyDepreciationAsync(int year, int userId)
    {
<<<<<<< HEAD
        if (year is < 1 or > 9999)
            return ApiResponse<bool>.Fail("السنة غير صالحة");
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        var assets = await _dbContext.FixedAssets
            .Where(a => a.Status == AssetStatus.Active
                && a.DepreciationRate > 0)
            .ToListAsync();

<<<<<<< HEAD
        var existingPeriods = (await _dbContext.DepreciationSchedules
            .AsNoTracking()
            .Where(d => d.PeriodYear == year)
            .Select(d => new { d.AssetID, d.PeriodMonth })
            .ToListAsync()).Select(d => (d.AssetID, d.PeriodMonth)).ToHashSet();

=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        int totalRecords = 0;
        int monthsProcessed = 0;

        for (int month = 1; month <= 12; month++)
        {
            int monthCount = 0;

            foreach (var asset in assets)
            {
<<<<<<< HEAD
                if (year < asset.InServiceDate.Year
                    || year == asset.InServiceDate.Year && month < asset.InServiceDate.Month)
                    continue;
                if (existingPeriods.Contains((asset.AssetID, month))) continue;
=======
                // التحقق من عدم وجود هلاك لهذا الشهر مسبقاً
                var exists = await _dbContext.DepreciationSchedules
                    .AnyAsync(d => d.AssetID == asset.AssetID && d.PeriodYear == year && d.PeriodMonth == month);
                if (exists) continue;
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

                // حساب الهلاك الشهري = (التكلفة - القيمة المتبقية) × (النسبة ÷ 100) ÷ 12
                var depreciableAmount = asset.AcquiredCost - asset.SalvageValue;
                var annualDepreciation = depreciableAmount * (asset.DepreciationRate / 100m);
                decimal monthlyDepreciation = annualDepreciation / 12m;

                // تطبيق الإهلاك الجزئي للشهر الأول إذا كان شهر التشغيل
                if (asset.InServiceDate.Year == year && asset.InServiceDate.Month == month)
                {
                    var daysInMonth = DateTime.DaysInMonth(year, month);
                    var daysInService = daysInMonth - asset.InServiceDate.Day + 1;
                    monthlyDepreciation = monthlyDepreciation * daysInService / daysInMonth;
                }

                // التحقق من عدم تجاوز الهلاك المتراكم للقيمة المتبقية
                var maxDepreciation = asset.AcquiredCost - asset.SalvageValue - asset.AccumulatedDepreciation;
                if (monthlyDepreciation > maxDepreciation)
                    monthlyDepreciation = maxDepreciation;

                if (monthlyDepreciation <= 0) continue;

                // تحديث الأصل
                asset.AccumulatedDepreciation += monthlyDepreciation;
                asset.NetBookValue -= monthlyDepreciation;
                asset.LastDepreciationDate = new DateTime(year, month, 1);

                // إنشاء سجل الهلاك
                var schedule = new DepreciationSchedule
                {
                    AssetID = asset.AssetID,
                    PeriodYear = year,
                    PeriodMonth = month,
                    PeriodStartDate = new DateTime(year, month, 1),
                    PeriodEndDate = new DateTime(year, month, DateTime.DaysInMonth(year, month)),
                    DepreciationAmount = monthlyDepreciation,
                    AccumulatedDepreciation = asset.AccumulatedDepreciation,
                    NetBookValue = asset.NetBookValue,
                    IsPosted = false,
                    CreatedAt = DateTime.UtcNow
                };

                _dbContext.DepreciationSchedules.Add(schedule);
                monthCount++;
            }

            if (monthCount > 0)
            {
                monthsProcessed++;
                totalRecords += monthCount;
            }
        }

        await _dbContext.SaveChangesAsync();

        return ApiResponse<bool>.Success(true, $"تم حساب إهلاك {assets.Count} أصل لـ {monthsProcessed} شهر بنجاح ({totalRecords} سجل)");
    }

    public async Task<ApiResponse<bool>> PostYearlyDepreciationAsync(int year, int userId)
    {
        // جلب كل سجلات الإهلاك غير المُرحّلة للسنة كاملة
        var schedules = await _dbContext.DepreciationSchedules
            .Where(d => d.PeriodYear == year && !d.IsPosted)
            .Include(d => d.Asset)
            .ToListAsync();

        if (!schedules.Any())
        {
            // هل يوجد سجلات مُرحّلة مسبقاً؟
            var alreadyPosted = await _dbContext.DepreciationSchedules
                .Where(d => d.PeriodYear == year && d.IsPosted)
                .CountAsync();

            if (alreadyPosted > 0)
                return ApiResponse<bool>.Fail(
                    $"تم ترحيل الإهلاك لهذه السنة مسبقاً ({alreadyPosted} سجل مُرحّل). لا يمكن إعادة الترحيل.");

            // هل يوجد أصول نشطة أصلاً؟
            var activeAssets = await _dbContext.FixedAssets
                .Where(a => a.Status == Core.Entities.Assets.AssetStatus.Active && a.DepreciationRate > 0)
                .CountAsync();

            if (activeAssets == 0)
                return ApiResponse<bool>.Fail(
                    "لا توجد أصول نشطة مؤهلة للإهلاك. تأكد من وجود أصول بحالة 'نشطة' ونسبة إهلاك أكبر من صفر.");

            return ApiResponse<bool>.Fail(
                $"لم يتم حساب الإهلاك للسنة {year} بعد. يجب تشغيل 'حساب الإهلاك' أولاً.");
        }

        // استخدام آخر شهر في السنة للفترة المالية
        var lastMonth = schedules.Max(d => d.PeriodMonth);
        var periodId = await _journalEntry.GetPeriodIdForDateAsync(new DateTime(year, lastMonth, 1));
        if (periodId <= 0)
            return ApiResponse<bool>.Fail("لا توجد فترة مالية مفتوحة لهذه السنة");

        // تجميع الإهلاك حسب حساب المصروف (للسنة كاملة)
        var groupedByExpense = schedules
            .Where(s => s.Asset.DeprExpenseAccountID > 0)
            .GroupBy(s => s.Asset.DeprExpenseAccountID)
            .ToList();

        if (!groupedByExpense.Any())
            return ApiResponse<bool>.Fail("لا توجد أصول مهيأة بحسابات الإهلاك. تأكد من تعبئة حساب مصروف الإهلاك وحساب مخصص الإهلاك في الأصول.");

        var errors = new List<string>();
        int postedCount = 0;

        foreach (var group in groupedByExpense)
        {
            var totalAmount = group.Sum(s => s.DepreciationAmount);
            var expenseAccountId = group.Key;

            // العثور على حساب مخصص الإهلاك المقابل
            var accumAccountId = group.First().Asset.AccumDeprAccountID;

            if (accumAccountId <= 0)
            {
                errors.Add($"حساب مخصص الإهلاك غير مُعيّن للأصول: {string.Join(", ", group.Select(s => s.Asset.AssetNumber))}");
                continue;
            }

            // إنشاء القيد المحاسبي الواحد للسنة كاملة
            try
            {
                var depreciationTypeId = await GetJournalEntryTypeIDAsync("DEP");

                var journalResult = await _journalEntry.CreateJournalEntryAsync(new CreateJournalEntryDto
                {
                    EntryDate = new DateTime(year, 12, 31),
                    PeriodID = periodId,
                    JournalEntryTypeID = depreciationTypeId,
                    SourceReferenceID = 0,
                    Description = $"قيد إهلاك الأصول - السنة المالية {year}",
                    Lines = new List<CreateJournalEntryLineDto>
                    {
                        new() { AccountID = expenseAccountId, Debit = totalAmount, Credit = 0, Description = "مصروف الإهلاك السنوي" },
                        new() { AccountID = accumAccountId, Debit = 0, Credit = totalAmount, Description = "مخصص الإهلاك المتراكم" }
                    }
                }, userId);

                if (!journalResult.IsSuccess)
                {
                    var assetNumbers = string.Join(", ", group.Select(s => s.Asset.AssetNumber));
                    errors.Add($"فشل ترحيل إهلاك الأصول [{assetNumbers}]: {journalResult.Message}");
                    continue;
                }

                postedCount++;
            }
            catch (Exception ex)
            {
                var assetNumbers = string.Join(", ", group.Select(s => s.Asset.AssetNumber));
                errors.Add($"خطأ في ترحيل إهلاك الأصول [{assetNumbers}]: {ex.Message}");
            }
        }

        // تحديث حالة الترحيل للسجلات الناجحة فقط
        var postedSchedules = schedules
            .Where(s => groupedByExpense.Any(g => g.Key == s.Asset.DeprExpenseAccountID && g.First().Asset.AccumDeprAccountID > 0))
            .ToList();

        foreach (var schedule in postedSchedules)
        {
            schedule.IsPosted = true;
            schedule.PostedDate = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync();

        if (errors.Any() && postedCount == 0)
            return ApiResponse<bool>.Fail(string.Join("\n", errors));

        if (errors.Any())
            return ApiResponse<bool>.Success(true, $"تم ترحيل {postedCount} قيد محاسبي بنجاح. ملاحظات:\n{string.Join("\n", errors)}");

        return ApiResponse<bool>.Success(true, $"تم ترحيل {postedCount} قيد محاسبي للسنة المالية {year} بنجاح");
    }

    private async Task UpdateDetailsAsync(int assetId, int assetTypeId, UpdateFixedAssetDto dto)
    {
        switch (assetTypeId)
        {
            case 0 when dto.ITDetail != null: // IT
                var itDetail = await _dbContext.AssetITDetails.FindAsync(assetId);
                if (itDetail == null)
                {
                    itDetail = new AssetITDetail { AssetID = assetId };
                    _dbContext.AssetITDetails.Add(itDetail);
                }
                UpdateITDetail(itDetail, dto.ITDetail);
                break;

            case 1 when dto.VehicleDetail != null: // Vehicle
                var vDetail = await _dbContext.AssetVehicleDetails.FindAsync(assetId);
                if (vDetail == null)
                {
                    vDetail = new AssetVehicleDetail { AssetID = assetId };
                    _dbContext.AssetVehicleDetails.Add(vDetail);
                }
                UpdateVehicleDetail(vDetail, dto.VehicleDetail);
                break;

            case 2 when dto.BuildingDetail != null: // Building
                var bDetail = await _dbContext.AssetBuildingDetails.FindAsync(assetId);
                if (bDetail == null)
                {
                    bDetail = new AssetBuildingDetail { AssetID = assetId };
                    _dbContext.AssetBuildingDetails.Add(bDetail);
                }
                UpdateBuildingDetail(bDetail, dto.BuildingDetail);
                break;

            case 3 when dto.LandDetail != null: // Land
                var lDetail = await _dbContext.AssetLandDetails.FindAsync(assetId);
                if (lDetail == null)
                {
                    lDetail = new AssetLandDetail { AssetID = assetId };
                    _dbContext.AssetLandDetails.Add(lDetail);
                }
                UpdateLandDetail(lDetail, dto.LandDetail);
                break;

            case 4 when dto.FurnitureDetail != null: // Furniture
                var fDetail = await _dbContext.AssetFurnitureDetails.FindAsync(assetId);
                if (fDetail == null)
                {
                    fDetail = new AssetFurnitureDetail { AssetID = assetId };
                    _dbContext.AssetFurnitureDetails.Add(fDetail);
                }
                UpdateFurnitureDetail(fDetail, dto.FurnitureDetail);
                break;
        }

        await _dbContext.SaveChangesAsync();
    }

    private void UpdateITDetail(AssetITDetail detail, AssetITDetailDto dto)
    {
        if (dto.Model != null) detail.Model = dto.Model;
        if (dto.Manufacturer != null) detail.Manufacturer = dto.Manufacturer;
        if (dto.ProcessorType != null) detail.ProcessorType = dto.ProcessorType;
        if (dto.ProcessorSpeed != null) detail.ProcessorSpeed = dto.ProcessorSpeed;
        if (dto.RamSize != null) detail.RamSize = dto.RamSize;
        if (dto.HardDiskType != null) detail.HardDiskType = dto.HardDiskType;
        if (dto.HardDiskSize != null) detail.HardDiskSize = dto.HardDiskSize;
        if (dto.GpuModel != null) detail.GpuModel = dto.GpuModel;
        if (dto.ScreenSize != null) detail.ScreenSize = dto.ScreenSize;
        if (dto.ScreenResolution != null) detail.ScreenResolution = dto.ScreenResolution;
        if (dto.OperatingSystem != null) detail.OperatingSystem = dto.OperatingSystem;
        detail.HasWebcam = dto.HasWebcam;
        detail.HasWifi = dto.HasWifi;
        detail.HasBluetooth = dto.HasBluetooth;
        if (dto.IpAddress != null) detail.IpAddress = dto.IpAddress;
        if (dto.MacAddress != null) detail.MacAddress = dto.MacAddress;
        if (dto.WarrantyExpiry.HasValue) detail.WarrantyExpiry = dto.WarrantyExpiry;
        if (dto.Notes != null) detail.Notes = dto.Notes;
    }

    private void UpdateVehicleDetail(AssetVehicleDetail detail, AssetVehicleDetailDto dto)
    {
        if (dto.PlateNumber != null) detail.PlateNumber = dto.PlateNumber;
        if (dto.PlateLetter1 != null) detail.PlateLetter1 = dto.PlateLetter1;
        if (dto.PlateLetter2 != null) detail.PlateLetter2 = dto.PlateLetter2;
        if (dto.PlateLetter3 != null) detail.PlateLetter3 = dto.PlateLetter3;
        if (dto.EngineNumber != null) detail.EngineNumber = dto.EngineNumber;
        if (dto.ChassisNumber != null) detail.ChassisNumber = dto.ChassisNumber;
        if (dto.TransmissionType != null) detail.TransmissionType = dto.TransmissionType;
        if (dto.FuelType != null) detail.FuelType = dto.FuelType;
        if (dto.CylinderCount.HasValue) detail.CylinderCount = dto.CylinderCount;
        if (dto.EngineCapacity != null) detail.EngineCapacity = dto.EngineCapacity;
        if (dto.HorsePower.HasValue) detail.HorsePower = dto.HorsePower;
        detail.WheelCount = dto.WheelCount;
        if (dto.TireSize != null) detail.TireSize = dto.TireSize;
        if (dto.Color != null) detail.Color = dto.Color;
        if (dto.ModelYear.HasValue) detail.ModelYear = dto.ModelYear;
        if (dto.RegistrationExpiry.HasValue) detail.RegistrationExpiry = dto.RegistrationExpiry;
        if (dto.InsuranceExpiry.HasValue) detail.InsuranceExpiry = dto.InsuranceExpiry;
        detail.Mileage = dto.Mileage;
        detail.HasGps = dto.HasGps;
        if (dto.Notes != null) detail.Notes = dto.Notes;
    }

    private void UpdateBuildingDetail(AssetBuildingDetail detail, AssetBuildingDetailDto dto)
    {
        if (dto.AreaSqm.HasValue) detail.AreaSqm = dto.AreaSqm;
        if (dto.BuiltAreaSqm.HasValue) detail.BuiltAreaSqm = dto.BuiltAreaSqm;
        detail.FloorCount = dto.FloorCount;
        if (dto.OwnershipDocNumber != null) detail.OwnershipDocNumber = dto.OwnershipDocNumber;
        if (dto.LicenseNumber != null) detail.LicenseNumber = dto.LicenseNumber;
        if (dto.Notes != null) detail.Notes = dto.Notes;
    }

    private void UpdateLandDetail(AssetLandDetail detail, AssetLandDetailDto dto)
    {
        if (dto.PlotNumber != null) detail.PlotNumber = dto.PlotNumber;
        if (dto.LandAreaSqm.HasValue) detail.LandAreaSqm = dto.LandAreaSqm;
        if (dto.OwnershipDocNumber != null) detail.OwnershipDocNumber = dto.OwnershipDocNumber;
        if (dto.OwnershipDocDate.HasValue) detail.OwnershipDocDate = dto.OwnershipDocDate;
        if (dto.Notes != null) detail.Notes = dto.Notes;
    }

    private void UpdateFurnitureDetail(AssetFurnitureDetail detail, AssetFurnitureDetailDto dto)
    {
        if (dto.Material != null) detail.Material = dto.Material;
        if (dto.Color != null) detail.Color = dto.Color;
        if (dto.Notes != null) detail.Notes = dto.Notes;
    }

    private static FixedAssetDto MapToDto(FixedAsset a)
    {
        return new FixedAssetDto
        {
            AssetID = a.AssetID,
            AssetNumber = a.AssetNumber,
            AssetName = a.AssetName,
            Description = a.Description,
            CategoryID = a.CategoryID,
            CategoryName = a.Category?.CategoryName,
            AssetTypeID = a.AssetTypeID,
            AssetTypeName = a.AssetType?.TypeName,
            SerialNumber = a.SerialNumber,
            PurchaseDate = a.PurchaseDate,
            InServiceDate = a.InServiceDate,
            UnitPrice = a.UnitPrice,
            AcquiredCost = a.AcquiredCost,
            SalvageValue = a.SalvageValue,
            CurrencyCode = a.CurrencyCode,
            Quantity = a.Quantity,
            DepreciationRate = a.DepreciationRate,
            ValuationMethod = a.ValuationMethod,
            ValuationNotes = a.ValuationNotes,
            AccumulatedDepreciation = a.AccumulatedDepreciation,
            NetBookValue = a.NetBookValue,
            LastDepreciationDate = a.LastDepreciationDate,
            Location = a.Location,
            CustodianEmployeeID = a.CustodianEmployeeID,
            CustodianEmployeeName = a.CustodianEmployee != null ? a.CustodianEmployee.FullName : null,
            AssetAccountID = a.AssetAccountID,
            AssetAccountName = a.AssetAccount?.AccountName,
            AssetAccountCode = a.AssetAccount?.AccountCode,
            AccumDeprAccountID = a.AccumDeprAccountID,
            DeprExpenseAccountID = a.DeprExpenseAccountID,
            Status = a.Status,
            DisposalDate = a.DisposalDate,
            DisposalReason = a.DisposalReason,
            CreatedAt = a.CreatedAt,
            UpdatedAt = a.UpdatedAt,
            ITDetail = a.ITDetail != null ? new AssetITDetailDto
            {
                Model = a.ITDetail.Model,
                Manufacturer = a.ITDetail.Manufacturer,
                ProcessorType = a.ITDetail.ProcessorType,
                ProcessorSpeed = a.ITDetail.ProcessorSpeed,
                RamSize = a.ITDetail.RamSize,
                HardDiskType = a.ITDetail.HardDiskType,
                HardDiskSize = a.ITDetail.HardDiskSize,
                GpuModel = a.ITDetail.GpuModel,
                ScreenSize = a.ITDetail.ScreenSize,
                ScreenResolution = a.ITDetail.ScreenResolution,
                OperatingSystem = a.ITDetail.OperatingSystem,
                HasWebcam = a.ITDetail.HasWebcam,
                HasWifi = a.ITDetail.HasWifi,
                HasBluetooth = a.ITDetail.HasBluetooth,
                IpAddress = a.ITDetail.IpAddress,
                MacAddress = a.ITDetail.MacAddress,
                WarrantyExpiry = a.ITDetail.WarrantyExpiry,
                Notes = a.ITDetail.Notes
            } : null,
            VehicleDetail = a.VehicleDetail != null ? new AssetVehicleDetailDto
            {
                PlateNumber = a.VehicleDetail.PlateNumber,
                PlateLetter1 = a.VehicleDetail.PlateLetter1,
                PlateLetter2 = a.VehicleDetail.PlateLetter2,
                PlateLetter3 = a.VehicleDetail.PlateLetter3,
                EngineNumber = a.VehicleDetail.EngineNumber,
                ChassisNumber = a.VehicleDetail.ChassisNumber,
                TransmissionType = a.VehicleDetail.TransmissionType,
                FuelType = a.VehicleDetail.FuelType,
                CylinderCount = a.VehicleDetail.CylinderCount,
                EngineCapacity = a.VehicleDetail.EngineCapacity,
                HorsePower = a.VehicleDetail.HorsePower,
                WheelCount = a.VehicleDetail.WheelCount,
                TireSize = a.VehicleDetail.TireSize,
                Color = a.VehicleDetail.Color,
                ModelYear = a.VehicleDetail.ModelYear,
                RegistrationExpiry = a.VehicleDetail.RegistrationExpiry,
                InsuranceExpiry = a.VehicleDetail.InsuranceExpiry,
                Mileage = a.VehicleDetail.Mileage,
                HasGps = a.VehicleDetail.HasGps,
                Notes = a.VehicleDetail.Notes
            } : null,
            BuildingDetail = a.BuildingDetail != null ? new AssetBuildingDetailDto
            {
                AreaSqm = a.BuildingDetail.AreaSqm,
                BuiltAreaSqm = a.BuildingDetail.BuiltAreaSqm,
                FloorCount = a.BuildingDetail.FloorCount,
                OwnershipDocNumber = a.BuildingDetail.OwnershipDocNumber,
                LicenseNumber = a.BuildingDetail.LicenseNumber,
                Notes = a.BuildingDetail.Notes
            } : null,
            LandDetail = a.LandDetail != null ? new AssetLandDetailDto
            {
                PlotNumber = a.LandDetail.PlotNumber,
                LandAreaSqm = a.LandDetail.LandAreaSqm,
                OwnershipDocNumber = a.LandDetail.OwnershipDocNumber,
                OwnershipDocDate = a.LandDetail.OwnershipDocDate,
                Notes = a.LandDetail.Notes
            } : null,
            FurnitureDetail = a.FurnitureDetail != null ? new AssetFurnitureDetailDto
            {
                Material = a.FurnitureDetail.Material,
                Color = a.FurnitureDetail.Color,
                Notes = a.FurnitureDetail.Notes
            } : null
        };
    }

<<<<<<< HEAD
    public Task<ApiResponse<int>> ResetCategoriesAsync() =>
        UnitOfWorkTransaction.ExecuteAsync(_unitOfWork, async () =>
        {
            // Historical assets retain their category, details and depreciation records.
            if (await _dbContext.FixedAssets.IgnoreQueryFilters().AnyAsync())
                return ApiResponse<int>.Fail("لا يمكن إعادة تهيئة التصنيفات لوجود أصول حالية أو تاريخية؛ عدّل التصنيفات القائمة.");
            var categories = await _dbContext.FixedAssetCategories.ToListAsync();
            _dbContext.FixedAssetCategories.RemoveRange(categories);
            await _dbContext.SaveChangesAsync();
            await FixedAssetCategorySeeder.SeedAsync(_dbContext);
            return ApiResponse<int>.Success(categories.Count, "تمت إعادة تهيئة التصنيفات");
        });
=======
    public async Task<ApiResponse<int>> ResetCategoriesAsync()
    {
        // 1. حذف جداول التفاصيل المرتبطة بالأصول
        var vehicleDetails = await _dbContext.AssetVehicleDetails.ToListAsync();
        _dbContext.AssetVehicleDetails.RemoveRange(vehicleDetails);

        var itDetails = await _dbContext.AssetITDetails.ToListAsync();
        _dbContext.AssetITDetails.RemoveRange(itDetails);

        var buildingDetails = await _dbContext.AssetBuildingDetails.ToListAsync();
        _dbContext.AssetBuildingDetails.RemoveRange(buildingDetails);

        var landDetails = await _dbContext.AssetLandDetails.ToListAsync();
        _dbContext.AssetLandDetails.RemoveRange(landDetails);

        var furnitureDetails = await _dbContext.AssetFurnitureDetails.ToListAsync();
        _dbContext.AssetFurnitureDetails.RemoveRange(furnitureDetails);

        // 2. حذف جدولة الإهلاك
        var schedules = await _dbContext.DepreciationSchedules.ToListAsync();
        _dbContext.DepreciationSchedules.RemoveRange(schedules);

        // 3. حذف عناصر الحراسة المرتبطة بالأصول
        var custodies = await _dbContext.InventoryCustodies.ToListAsync();
        _dbContext.InventoryCustodies.RemoveRange(custodies);

        // 4. حذف الأصول
        var assets = await _dbContext.FixedAssets.ToListAsync();
        _dbContext.FixedAssets.RemoveRange(assets);

        // 5. حذف التصنيفات
        var categories = await _dbContext.FixedAssetCategories.ToListAsync();
        _dbContext.FixedAssetCategories.RemoveRange(categories);

        await _dbContext.SaveChangesAsync();

        // 6. إعادة إنشاء التصنيفات بالحسابات الصحيحة
        await FixedAssetCategorySeeder.SeedAsync(_dbContext);

        return ApiResponse<int>.Success(categories.Count, $"تم حذف {assets.Count} أصل و {categories.Count} تصنيف وإعادة تهيئتها");
    }
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

    public async Task<ApiResponse<AssetCategoryDto>> CreateCategoryAsync(CreateAssetCategoryDto dto, int userId)
    {
        var entity = new FixedAssetCategory
        {
            CategoryName = dto.CategoryName,
            CategoryCode = dto.CategoryCode,
            Description = dto.Description,
            AssetTypeID = dto.AssetTypeID,
            DepreciationRate = dto.DepreciationRate,
            SalvagePercent = dto.SalvagePercent,
            AssetAccountID = dto.AssetAccountID,
            AccumDeprAccountID = dto.AccumDeprAccountID,
            DeprExpenseAccountID = dto.DeprExpenseAccountID,
            IsActive = true,
            CreatedByUserID = userId,
            CreatedAt = DateTime.UtcNow,
        };
        _dbContext.FixedAssetCategories.Add(entity);
        await _dbContext.SaveChangesAsync();

        return await GetCategoryByIdAsync(entity.CategoryID);
    }

    public async Task<ApiResponse<AssetCategoryDto>> UpdateCategoryAsync(int id, UpdateAssetCategoryDto dto, int userId)
    {
        var entity = await _dbContext.FixedAssetCategories.FindAsync(id);
        if (entity == null)
            return ApiResponse<AssetCategoryDto>.Fail("التصنيف غير موجود");

        if (dto.CategoryName != null) entity.CategoryName = dto.CategoryName;
        if (dto.CategoryCode != null) entity.CategoryCode = dto.CategoryCode;
        if (dto.Description != null) entity.Description = dto.Description;
        if (dto.AssetTypeID.HasValue) entity.AssetTypeID = dto.AssetTypeID.Value;
        if (dto.DepreciationRate.HasValue) entity.DepreciationRate = dto.DepreciationRate.Value;
        if (dto.SalvagePercent.HasValue) entity.SalvagePercent = dto.SalvagePercent.Value;
        if (dto.AssetAccountID.HasValue) entity.AssetAccountID = dto.AssetAccountID;
        if (dto.AccumDeprAccountID.HasValue) entity.AccumDeprAccountID = dto.AccumDeprAccountID;
        if (dto.DeprExpenseAccountID.HasValue) entity.DeprExpenseAccountID = dto.DeprExpenseAccountID;
        if (dto.IsActive.HasValue) entity.IsActive = dto.IsActive.Value;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedByUserID = userId;

        await _dbContext.SaveChangesAsync();
        return await GetCategoryByIdAsync(id);
    }

    public async Task<ApiResponse<bool>> DeleteCategoryAsync(int id, int userId)
    {
        var entity = await _dbContext.FixedAssetCategories
            .Include(c => c.Assets)
            .FirstOrDefaultAsync(c => c.CategoryID == id);
        if (entity == null)
            return ApiResponse<bool>.Fail("التصنيف غير موجود");
<<<<<<< HEAD
        if (await _dbContext.FixedAssets.IgnoreQueryFilters().AnyAsync(a => a.CategoryID == id))
=======
        if (entity.Assets.Count > 0)
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            return ApiResponse<bool>.Fail("لا يمكن حذف تصنيف مرتبط بأصول");

        _dbContext.FixedAssetCategories.Remove(entity);
        await _dbContext.SaveChangesAsync();
        await _auditService.LogAsync(userId, "", "Delete", "FixedAssetCategory", id, null, null);
        return ApiResponse<bool>.Success(true, "تم حذف التصنيف بنجاح");
    }

    public async Task<ApiResponse<bool>> ToggleCategoryActiveAsync(int id, int userId)
    {
        var entity = await _dbContext.FixedAssetCategories.FindAsync(id);
        if (entity == null)
            return ApiResponse<bool>.Fail("التصنيف غير موجود");

        entity.IsActive = !entity.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedByUserID = userId;
        await _dbContext.SaveChangesAsync();
        return ApiResponse<bool>.Success(true, entity.IsActive ? "تم تفعيل التصنيف" : "تم تعطيل التصنيف");
    }

    private async Task<ApiResponse<AssetCategoryDto>> GetCategoryByIdAsync(int id)
    {
        var cat = await _dbContext.FixedAssetCategories
            .AsNoTracking()
            .Include(c => c.AssetType)
            .Include(c => c.AssetAccount)
            .Include(c => c.AccumDeprAccount)
            .Include(c => c.DeprExpenseAccount)
            .Where(c => c.CategoryID == id)
            .Select(c => new AssetCategoryDto
            {
                CategoryID = c.CategoryID,
                CategoryName = c.CategoryName,
                CategoryCode = c.CategoryCode,
                Description = c.Description,
                AssetTypeID = c.AssetTypeID,
                AssetTypeName = c.AssetType.TypeName,
                DepreciationRate = c.DepreciationRate,
                SalvagePercent = c.SalvagePercent,
                AssetAccountID = c.AssetAccountID,
                AssetAccountName = c.AssetAccount != null ? c.AssetAccount.AccountName : null,
                AssetAccountCode = c.AssetAccount != null ? c.AssetAccount.AccountCode : null,
                AccumDeprAccountID = c.AccumDeprAccountID,
                AccumDeprAccountName = c.AccumDeprAccount != null ? c.AccumDeprAccount.AccountName : null,
                DeprExpenseAccountID = c.DeprExpenseAccountID,
                DeprExpenseAccountName = c.DeprExpenseAccount != null ? c.DeprExpenseAccount.AccountName : null,
                IsActive = c.IsActive,
                AssetCount = c.Assets.Count
            })
            .FirstOrDefaultAsync();

        if (cat == null)
            return ApiResponse<AssetCategoryDto>.Fail("التصنيف غير موجود");

        return ApiResponse<AssetCategoryDto>.Success(cat);
    }
}
