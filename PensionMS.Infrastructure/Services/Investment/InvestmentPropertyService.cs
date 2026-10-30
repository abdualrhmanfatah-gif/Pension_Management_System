using Microsoft.EntityFrameworkCore;
using PensionMS.Core.Constants;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Investment;
using PensionMS.Core.Entities.Investment;
using PensionMS.Core.Interfaces;
using System.Globalization;

namespace PensionMS.Infrastructure.Services.Investment;

public class InvestmentPropertyService : IInvestmentPropertyService
{
    private readonly IUnitOfWork _unitOfWork;
<<<<<<< HEAD
    private readonly IAccessiblePensionRepository _access;

    public InvestmentPropertyService(IUnitOfWork unitOfWork, IAccessiblePensionRepository access)
    {
        _unitOfWork = unitOfWork;
        _access = access;
=======

    public InvestmentPropertyService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    }

    // ----------------------------------------------------------------------
    // PROPERTIES
    // ----------------------------------------------------------------------

    public async Task<ApiResponse<int>> CreatePropertyAsync(CreatePropertyDto dto, int userId)
    {
<<<<<<< HEAD
        var queries = await _access.ForCurrentOperationAsync();
        if (dto.ProjectID.HasValue)
        {
            var project = await queries.Query(_unitOfWork.InvestmentProjects)
                .FirstOrDefaultAsync(p => p.ProjectID == dto.ProjectID.Value);
=======
        if (dto.ProjectID.HasValue)
        {
            var project = await _unitOfWork.InvestmentProjects.GetByIdAsync(dto.ProjectID.Value);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            if (project == null || project.IsDeleted)
                return ApiResponse<int>.Fail("المشروع غير موجود");
        }

        var type = await _unitOfWork.PropertyTypes.GetByIdAsync(dto.PropertyTypeID);
        if (type == null || type.IsDeleted)
            return ApiResponse<int>.Fail("نوع العقار غير موجود");

        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        int branchId = dto.PensionBranchID > 0 ? dto.PensionBranchID : (user?.BranchID ?? 1);
<<<<<<< HEAD
        queries.ResolveBranch(branchId);
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1

        var property = new InvestmentProperty
        {
            ProjectID = dto.ProjectID,
            PensionBranchID = branchId,
            PropertyTypeID = dto.PropertyTypeID,
            PropertyName = dto.PropertyName,
            Address = dto.Address,
            LocationCity = dto.LocationCity,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            GoogleMapsUrl = dto.GoogleMapsUrl,
            MainPhotoPath = dto.MainPhotoPath,
            PhotosJson = dto.PhotosJson,
            OwnershipDeedPath = dto.OwnershipDeedPath,
            TotalArea = dto.TotalArea,
            AcquisitionDate = dto.AcquisitionDate,
            InitialValue = dto.InitialValue,
            TitleDeedNumber = dto.TitleDeedNumber,
            Status = dto.Status ?? "Ready",
            Description = dto.Description,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.InvestmentProperties.AddAsync(property);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<int>.Success(property.PropertyID, "تم إنشاء العقار بنجاح");
    }

    public async Task<ApiResponse<bool>> UpdatePropertyAsync(int id, UpdatePropertyDto dto, int userId)
    {
<<<<<<< HEAD
        var queries = await _access.ForCurrentOperationAsync();
        var property = await queries.Query(_unitOfWork.InvestmentProperties)
            .FirstOrDefaultAsync(p => p.PropertyID == id);
        if (property == null || property.IsDeleted)
            return ApiResponse<bool>.Fail("العقار غير موجود");

        if (dto.PensionBranchID > 0) queries.ResolveBranch(dto.PensionBranchID);

        if (dto.ProjectID.HasValue)
        {
            var project = await queries.Query(_unitOfWork.InvestmentProjects)
                .FirstOrDefaultAsync(p => p.ProjectID == dto.ProjectID.Value);
=======
        var property = await _unitOfWork.InvestmentProperties.GetByIdAsync(id);
        if (property == null || property.IsDeleted)
            return ApiResponse<bool>.Fail("العقار غير موجود");

        if (dto.ProjectID.HasValue)
        {
            var project = await _unitOfWork.InvestmentProjects.GetByIdAsync(dto.ProjectID.Value);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            if (project == null || project.IsDeleted)
                return ApiResponse<bool>.Fail("المشروع غير موجود");
        }

        var type = await _unitOfWork.PropertyTypes.GetByIdAsync(dto.PropertyTypeID);
        if (type == null || type.IsDeleted)
            return ApiResponse<bool>.Fail("نوع العقار غير موجود");

        property.ProjectID = dto.ProjectID;
        property.PropertyTypeID = dto.PropertyTypeID;
        property.PropertyName = dto.PropertyName;
        property.Address = dto.Address;
        property.LocationCity = dto.LocationCity;
        property.Latitude = dto.Latitude;
        property.Longitude = dto.Longitude;
        property.GoogleMapsUrl = dto.GoogleMapsUrl;
        property.MainPhotoPath = dto.MainPhotoPath;
        property.PhotosJson = dto.PhotosJson;
        property.OwnershipDeedPath = dto.OwnershipDeedPath;
        property.TotalArea = dto.TotalArea;
        property.AcquisitionDate = dto.AcquisitionDate;
        property.InitialValue = dto.InitialValue;
        property.TitleDeedNumber = dto.TitleDeedNumber;
        property.Status = dto.Status ?? InvestmentPropertyStatus.Ready;
        property.Description = dto.Description;
        property.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.InvestmentProperties.Update(property);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.Success(true, "تم تحديث العقار بنجاح");
    }

    public async Task<ApiResponse<bool>> DeletePropertyAsync(int id, int userId)
    {
<<<<<<< HEAD
        var queries = await _access.ForCurrentOperationAsync();
        var property = await queries.Query(_unitOfWork.InvestmentProperties)
            .FirstOrDefaultAsync(p => p.PropertyID == id);
=======
        var property = await _unitOfWork.InvestmentProperties.GetByIdAsync(id);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        if (property == null || property.IsDeleted)
            return ApiResponse<bool>.Fail("العقار غير موجود");

        property.IsDeleted = true;
        property.DeletedAt = DateTime.UtcNow;
        property.DeletedByUserID = userId;

        _unitOfWork.InvestmentProperties.Update(property);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.Success(true, "تم حذف العقار بنجاح");
    }

    public async Task<ApiResponse<PropertyDetailDto>> GetPropertyByIdAsync(int id)
    {
<<<<<<< HEAD
        var queries = await _access.ForCurrentOperationAsync();
        var property = await queries.Query(_unitOfWork.InvestmentProperties)
            .AsNoTracking()
=======
        var property = await _unitOfWork.InvestmentProperties.Query()
            .Include(p => p.Project)
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            .Include(p => p.PropertyType)
            .Include(p => p.Units.Where(u => !u.IsDeleted))
                .ThenInclude(u => u.UnitType)
            .Include(p => p.Valuations.Where(v => !v.IsDeleted))
            .FirstOrDefaultAsync(p => p.PropertyID == id && !p.IsDeleted);

        if (property == null)
            return ApiResponse<PropertyDetailDto>.Fail("العقار غير موجود");

<<<<<<< HEAD
        var projectNames = await LoadProjectNamesAsync(queries, [property]);
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        var latestValuation = property.Valuations
            .OrderByDescending(v => v.ValuationDate)
            .ThenByDescending(v => v.ValuationID)
            .FirstOrDefault();

        var dto = new PropertyDetailDto
        {
            PropertyID = property.PropertyID,
            ProjectID = property.ProjectID,
<<<<<<< HEAD
            ProjectName = property.ProjectID.HasValue ? projectNames.GetValueOrDefault(property.ProjectID.Value) : null,
=======
            ProjectName = property.Project?.Name,
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            PropertyTypeID = property.PropertyTypeID,
            PropertyTypeName = property.PropertyType?.TypeName ?? string.Empty,
            PropertyName = property.PropertyName,
            LocationCity = property.LocationCity,
            MainPhotoPath = property.MainPhotoPath,
            TotalArea = property.TotalArea,
            InitialValue = property.InitialValue,
            CurrentMarketValue = latestValuation?.MarketValue ?? property.InitialValue,
            Address = property.Address,
            Latitude = property.Latitude,
            Longitude = property.Longitude,
            GoogleMapsUrl = property.GoogleMapsUrl,
            PhotosJson = property.PhotosJson,
            OwnershipDeedPath = property.OwnershipDeedPath,
            AcquisitionDate = property.AcquisitionDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            Status = property.Status,
            TitleDeedNumber = property.TitleDeedNumber,
            Description = property.Description,
            Units = property.Units.Select(u => new PropertyUnitDto
            {
                UnitID = u.UnitID,
                PropertyID = u.PropertyID,
                PropertyName = property.PropertyName,
                UnitTypeID = u.UnitTypeID,
                UnitTypeName = u.UnitType?.TypeName ?? string.Empty,
                UnitNumber = u.UnitNumber,
                Floor = u.Floor,
                Area = u.Area,
                Status = u.Status,
                MainPhotoPath = u.MainPhotoPath
            }).ToList(),
            Valuations = property.Valuations
                .OrderByDescending(v => v.ValuationDate)
                .ThenByDescending(v => v.ValuationID)
                .Select(v => new ValuationDto
                {
                    ValuationID = v.ValuationID,
                    PropertyID = v.PropertyID,
                    ValuationDate = v.ValuationDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    MarketValue = v.MarketValue,
                    ValuedBy = v.ValuedBy,
                    ValuationReportPath = v.ValuationReportPath
                }).ToList()
        };

        return ApiResponse<PropertyDetailDto>.Success(dto);
    }

    public async Task<ApiResponse<List<PropertyDto>>> GetAllPropertiesAsync(int page = 1, int pageSize = 50)
    {
<<<<<<< HEAD
        var queries = await _access.ForCurrentOperationAsync();
        var properties = await queries.Query(_unitOfWork.InvestmentProperties)
            .AsNoTracking()
            .Include(p => p.PropertyType)
            .Include(p => p.Valuations.Where(v => !v.IsDeleted))
            .Where(p => !p.IsDeleted)
            .OrderBy(p => p.PropertyID)
=======
        var properties = await _unitOfWork.InvestmentProperties.Query()
            .Include(p => p.Project)
            .Include(p => p.PropertyType)
            .Include(p => p.Valuations.Where(v => !v.IsDeleted))
            .Where(p => !p.IsDeleted)
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

<<<<<<< HEAD
        var projectNames = await LoadProjectNamesAsync(queries, properties);
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        var result = properties.Select(p => {
            var latestValuation = p.Valuations
                .OrderByDescending(v => v.ValuationDate)
                .ThenByDescending(v => v.ValuationID)
                .FirstOrDefault();
            return new PropertyDto
            {
                PropertyID = p.PropertyID,
                ProjectID = p.ProjectID,
<<<<<<< HEAD
                ProjectName = p.ProjectID.HasValue ? projectNames.GetValueOrDefault(p.ProjectID.Value) : null,
=======
                ProjectName = p.Project?.Name,
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                PropertyTypeID = p.PropertyTypeID,
                PropertyTypeName = p.PropertyType?.TypeName ?? string.Empty,
                PropertyName = p.PropertyName,
                LocationCity = p.LocationCity,
                MainPhotoPath = p.MainPhotoPath,
                TotalArea = p.TotalArea,
                InitialValue = p.InitialValue,
                CurrentMarketValue = latestValuation?.MarketValue ?? p.InitialValue,
                Status = p.Status,
                TitleDeedNumber = p.TitleDeedNumber,
                GoogleMapsUrl = p.GoogleMapsUrl
            };
        }).ToList();

        return ApiResponse<List<PropertyDto>>.Success(result);
    }

    public async Task<ApiResponse<List<PropertyDto>>> GetPropertiesByProjectAsync(int projectId)
    {
<<<<<<< HEAD
        var queries = await _access.ForCurrentOperationAsync();
        if (!await queries.Query(_unitOfWork.InvestmentProjects).AnyAsync(p => p.ProjectID == projectId && !p.IsDeleted))
            return ApiResponse<List<PropertyDto>>.Success(new List<PropertyDto>());

        var properties = await queries.Query(_unitOfWork.InvestmentProperties)
            .AsNoTracking()
=======
        var properties = await _unitOfWork.InvestmentProperties.Query()
            .Include(p => p.Project)
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            .Include(p => p.PropertyType)
            .Include(p => p.Valuations.Where(v => !v.IsDeleted))
            .Where(p => p.ProjectID == projectId && !p.IsDeleted)
            .ToListAsync();

<<<<<<< HEAD
        var projectNames = await LoadProjectNamesAsync(queries, properties);
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        var result = properties.Select(p => {
            var latestValuation = p.Valuations
                .OrderByDescending(v => v.ValuationDate)
                .ThenByDescending(v => v.ValuationID)
                .FirstOrDefault();
            return new PropertyDto
            {
                PropertyID = p.PropertyID,
                ProjectID = p.ProjectID,
<<<<<<< HEAD
                ProjectName = p.ProjectID.HasValue ? projectNames.GetValueOrDefault(p.ProjectID.Value) : null,
=======
                ProjectName = p.Project?.Name,
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
                PropertyTypeID = p.PropertyTypeID,
                PropertyTypeName = p.PropertyType?.TypeName ?? string.Empty,
                PropertyName = p.PropertyName,
                LocationCity = p.LocationCity,
                MainPhotoPath = p.MainPhotoPath,
                TotalArea = p.TotalArea,
                InitialValue = p.InitialValue,
                CurrentMarketValue = latestValuation?.MarketValue ?? p.InitialValue,
                Status = p.Status,
                TitleDeedNumber = p.TitleDeedNumber,
                GoogleMapsUrl = p.GoogleMapsUrl
            };
        }).ToList();

        return ApiResponse<List<PropertyDto>>.Success(result);
    }

    public async Task<ApiResponse<bool>> UpdatePropertyMainPhotoPathAsync(int id, string photoPath, int userId)
    {
<<<<<<< HEAD
        var queries = await _access.ForCurrentOperationAsync();
        var property = await queries.Query(_unitOfWork.InvestmentProperties)
            .FirstOrDefaultAsync(p => p.PropertyID == id);
=======
        var property = await _unitOfWork.InvestmentProperties.GetByIdAsync(id);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        if (property == null || property.IsDeleted)
            return ApiResponse<bool>.Fail("العقار غير موجود");

        property.MainPhotoPath = photoPath;
        property.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.InvestmentProperties.Update(property);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.Success(true, "تم تحديث مسار الصورة بنجاح");
    }

    // ----------------------------------------------------------------------
    // UNITS
    // ----------------------------------------------------------------------

    public async Task<ApiResponse<int>> CreateUnitAsync(CreateUnitDto dto, int userId)
    {
<<<<<<< HEAD
        var queries = await _access.ForCurrentOperationAsync();
        var property = await queries.Query(_unitOfWork.InvestmentProperties)
            .FirstOrDefaultAsync(p => p.PropertyID == dto.PropertyID);
=======
        var property = await _unitOfWork.InvestmentProperties.GetByIdAsync(dto.PropertyID);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        if (property == null || property.IsDeleted)
            return ApiResponse<int>.Fail("العقار غير موجود");

        var type = await _unitOfWork.UnitTypes.GetByIdAsync(dto.UnitTypeID);
        if (type == null || type.IsDeleted)
            return ApiResponse<int>.Fail("نوع الوحدة غير موجود");

        var unit = new PropertyUnit
        {
            PropertyID = dto.PropertyID,
            UnitTypeID = dto.UnitTypeID,
            UnitNumber = dto.UnitNumber,
            Floor = dto.Floor,
            Area = dto.Area,
            MainPhotoPath = dto.MainPhotoPath,
            PhotosJson = dto.PhotosJson,
            LayoutPlanPath = dto.LayoutPlanPath,
            Status = dto.Status,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.PropertyUnits.AddAsync(unit);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<int>.Success(unit.UnitID, "تم إنشاء الوحدة بنجاح");
    }

    public async Task<ApiResponse<bool>> UpdateUnitAsync(int id, UpdateUnitDto dto, int userId)
    {
<<<<<<< HEAD
        var queries = await _access.ForCurrentOperationAsync();
        var unit = await queries.Query(_unitOfWork.PropertyUnits)
            .FirstOrDefaultAsync(u => u.UnitID == id);
        if (unit == null || unit.IsDeleted)
            return ApiResponse<bool>.Fail("الوحدة غير موجودة");

        var property = await queries.Query(_unitOfWork.InvestmentProperties)
            .FirstOrDefaultAsync(p => p.PropertyID == dto.PropertyID);
=======
        var unit = await _unitOfWork.PropertyUnits.GetByIdAsync(id);
        if (unit == null || unit.IsDeleted)
            return ApiResponse<bool>.Fail("الوحدة غير موجودة");

        var property = await _unitOfWork.InvestmentProperties.GetByIdAsync(dto.PropertyID);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        if (property == null || property.IsDeleted)
            return ApiResponse<bool>.Fail("العقار غير موجود");

        var type = await _unitOfWork.UnitTypes.GetByIdAsync(dto.UnitTypeID);
        if (type == null || type.IsDeleted)
            return ApiResponse<bool>.Fail("نوع الوحدة غير موجود");

        unit.PropertyID = dto.PropertyID;
        unit.UnitTypeID = dto.UnitTypeID;
        unit.UnitNumber = dto.UnitNumber;
        unit.Floor = dto.Floor;
        unit.Area = dto.Area;
        unit.MainPhotoPath = dto.MainPhotoPath;
        unit.PhotosJson = dto.PhotosJson;
        unit.LayoutPlanPath = dto.LayoutPlanPath;
        unit.Status = dto.Status;
        unit.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.PropertyUnits.Update(unit);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.Success(true, "تم تحديث الوحدة بنجاح");
    }

    public async Task<ApiResponse<bool>> DeleteUnitAsync(int id, int userId)
    {
<<<<<<< HEAD
        var queries = await _access.ForCurrentOperationAsync();
        var unit = await queries.Query(_unitOfWork.PropertyUnits)
            .FirstOrDefaultAsync(u => u.UnitID == id);
=======
        var unit = await _unitOfWork.PropertyUnits.GetByIdAsync(id);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        if (unit == null || unit.IsDeleted)
            return ApiResponse<bool>.Fail("الوحدة غير موجودة");

        unit.IsDeleted = true;
        unit.DeletedAt = DateTime.UtcNow;
        unit.DeletedByUserID = userId;

        _unitOfWork.PropertyUnits.Update(unit);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.Success(true, "تم حذف الوحدة بنجاح");
    }

    public async Task<ApiResponse<List<PropertyUnitDto>>> GetUnitsByPropertyAsync(int propertyId)
    {
<<<<<<< HEAD
        var queries = await _access.ForCurrentOperationAsync();
        var units = await queries.Query(_unitOfWork.PropertyUnits)
            .AsNoTracking()
=======
        var units = await _unitOfWork.PropertyUnits.Query()
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            .Include(u => u.Property)
            .Include(u => u.UnitType)
            .Where(u => u.PropertyID == propertyId && !u.IsDeleted)
            .Select(u => new PropertyUnitDto
            {
                UnitID = u.UnitID,
                PropertyID = u.PropertyID,
                PropertyName = u.Property != null ? u.Property.PropertyName : string.Empty,
                UnitTypeID = u.UnitTypeID,
                UnitTypeName = u.UnitType != null ? u.UnitType.TypeName : string.Empty,
                UnitNumber = u.UnitNumber,
                Floor = u.Floor,
                Area = u.Area,
                Status = u.Status,
                MainPhotoPath = u.MainPhotoPath
            })
            .ToListAsync();

        return ApiResponse<List<PropertyUnitDto>>.Success(units);
    }

    public async Task<ApiResponse<List<PropertyUnitDto>>> GetAllUnitsAsync()
    {
<<<<<<< HEAD
        var queries = await _access.ForCurrentOperationAsync();
        var units = await queries.Query(_unitOfWork.PropertyUnits)
            .AsNoTracking()
=======
        var units = await _unitOfWork.PropertyUnits.Query()
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            .Include(u => u.Property)
            .Include(u => u.UnitType)
            .Where(u => !u.IsDeleted)
            .Select(u => new PropertyUnitDto
            {
                UnitID = u.UnitID,
                PropertyID = u.PropertyID,
                PropertyName = u.Property != null ? u.Property.PropertyName : string.Empty,
                UnitTypeID = u.UnitTypeID,
                UnitTypeName = u.UnitType != null ? u.UnitType.TypeName : string.Empty,
                UnitNumber = u.UnitNumber,
                Floor = u.Floor,
                Area = u.Area,
                Status = u.Status,
                MainPhotoPath = u.MainPhotoPath
            })
            .ToListAsync();

        return ApiResponse<List<PropertyUnitDto>>.Success(units);
    }

    // ----------------------------------------------------------------------
    // VALUATIONS & DEPRECIATION
    // ----------------------------------------------------------------------

    public async Task<ApiResponse<int>> CreateValuationAsync(CreateValuationDto dto, int userId)
    {
<<<<<<< HEAD
        var queries = await _access.ForCurrentOperationAsync();
        var property = await queries.Query(_unitOfWork.InvestmentProperties)
            .FirstOrDefaultAsync(p => p.PropertyID == dto.PropertyID);
=======
        var property = await _unitOfWork.InvestmentProperties.GetByIdAsync(dto.PropertyID);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        if (property == null || property.IsDeleted)
            return ApiResponse<int>.Fail("العقار غير موجود");

        var valuation = new AssetValuation
        {
            PropertyID = dto.PropertyID,
            ValuationDate = dto.ValuationDate,
            MarketValue = dto.MarketValue,
            ValuedBy = dto.ValuedBy,
            ValuationReportPath = dto.ValuationReportPath,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.AssetValuations.AddAsync(valuation);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<int>.Success(valuation.ValuationID, "تم تسجيل التقييم بنجاح");
    }

    public async Task<ApiResponse<List<ValuationDto>>> GetValuationsByPropertyAsync(int propertyId)
    {
<<<<<<< HEAD
        var queries = await _access.ForCurrentOperationAsync();
        var valuations = await queries.Query(_unitOfWork.AssetValuations)
            .AsNoTracking()
=======
        var valuations = await _unitOfWork.AssetValuations.Query()
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            .Where(v => v.PropertyID == propertyId && !v.IsDeleted)
            .OrderByDescending(v => v.ValuationDate)
            .Select(v => new ValuationDto
            {
                ValuationID = v.ValuationID,
                PropertyID = v.PropertyID,
                ValuationDate = v.ValuationDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                MarketValue = v.MarketValue,
                ValuedBy = v.ValuedBy,
                ValuationReportPath = v.ValuationReportPath
            })
            .ToListAsync();

        return ApiResponse<List<ValuationDto>>.Success(valuations);
    }

    public async Task<ApiResponse<bool>> DeleteValuationAsync(int id, int userId)
    {
<<<<<<< HEAD
        var queries = await _access.ForCurrentOperationAsync();
        var valuation = await queries.Query(_unitOfWork.AssetValuations)
            .FirstOrDefaultAsync(v => v.ValuationID == id);
=======
        var valuation = await _unitOfWork.AssetValuations.GetByIdAsync(id);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        if (valuation == null || valuation.IsDeleted)
            return ApiResponse<bool>.Fail("التقييم غير موجود");

        valuation.IsDeleted = true;
        valuation.DeletedAt = DateTime.UtcNow;
        valuation.DeletedByUserID = userId;

        _unitOfWork.AssetValuations.Update(valuation);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.Success(true, "تم حذف التقييم بنجاح");
    }

    // ----------------------------------------------------------------------
    // DEPRECIATION
    // ----------------------------------------------------------------------

    public async Task<ApiResponse<int>> CreateDepreciationAsync(CreateDepreciationDto dto, int userId)
    {
<<<<<<< HEAD
        var queries = await _access.ForCurrentOperationAsync();
        var property = await queries.Query(_unitOfWork.InvestmentProperties)
            .FirstOrDefaultAsync(p => p.PropertyID == dto.PropertyID);
=======
        var property = await _unitOfWork.InvestmentProperties.GetByIdAsync(dto.PropertyID);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        if (property == null || property.IsDeleted)
            return ApiResponse<int>.Fail("العقار غير موجود");

        // Note: FiscalYear is in OB_ namespace, so we assume it's queried via another repo if needed
        // but here we just check if it's set correctly if there's a fiscal year repo.
        // Assuming we just trust it or rely on DB constraint.

        var depreciation = new DepreciationRecord
        {
            PropertyID = dto.PropertyID,
            FiscalYearID = dto.FiscalYearID,
            DepreciationRate = dto.DepreciationRate,
            DepreciationAmount = dto.DepreciationAmount,
            NetBookValue = dto.NetBookValue,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.DepreciationRecords.AddAsync(depreciation);
        await _unitOfWork.SaveChangesAsync();

        // No longer auto-posting to accounting.

        return ApiResponse<int>.Success(depreciation.DeprecID, "تم تسجيل الإهلاك بنجاح");
    }

    public async Task<ApiResponse<List<DepreciationRecordDto>>> GetDepreciationByPropertyAsync(int propertyId)
    {
<<<<<<< HEAD
        var queries = await _access.ForCurrentOperationAsync();
        var records = await queries.Query(_unitOfWork.DepreciationRecords)
            .AsNoTracking()
=======
        var records = await _unitOfWork.DepreciationRecords.Query()
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            .Include(d => d.FiscalYear)
            .Where(d => d.PropertyID == propertyId && !d.IsDeleted)
            .OrderByDescending(d => d.CreatedAt)
            .Select(d => new DepreciationRecordDto
            {
                DeprecID = d.DeprecID,
                PropertyID = d.PropertyID,
                FiscalYearID = d.FiscalYearID,
                YearName = d.FiscalYear != null ? d.FiscalYear.YearName : string.Empty,
                DepreciationRate = d.DepreciationRate,
                DepreciationAmount = d.DepreciationAmount,
                NetBookValue = d.NetBookValue
            })
            .ToListAsync();

        return ApiResponse<List<DepreciationRecordDto>>.Success(records);
    }

    public async Task<ApiResponse<bool>> DeleteDepreciationAsync(int id, int userId)
    {
<<<<<<< HEAD
        var queries = await _access.ForCurrentOperationAsync();
        var record = await queries.Query(_unitOfWork.DepreciationRecords)
            .FirstOrDefaultAsync(d => d.DeprecID == id);
=======
        var record = await _unitOfWork.DepreciationRecords.GetByIdAsync(id);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        if (record == null || record.IsDeleted)
            return ApiResponse<bool>.Fail("سجل الإهلاك غير موجود");

        record.IsDeleted = true;
        record.DeletedAt = DateTime.UtcNow;
        record.DeletedByUserID = userId;

        _unitOfWork.DepreciationRecords.Update(record);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.Success(true, "تم حذف سجل الإهلاك بنجاح");
    }
<<<<<<< HEAD

    private async Task<Dictionary<int, string>> LoadProjectNamesAsync(IPensionDataQuery queries,
        IEnumerable<InvestmentProperty> properties)
    {
        var projectIds = properties.Where(p => p.ProjectID.HasValue)
            .Select(p => p.ProjectID!.Value).Distinct().ToArray();
        if (projectIds.Length == 0) return new Dictionary<int, string>();

        return await queries.Query(_unitOfWork.InvestmentProjects).AsNoTracking()
            .Where(p => projectIds.Contains(p.ProjectID) && !p.IsDeleted)
            .ToDictionaryAsync(p => p.ProjectID, p => p.Name);
    }
=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
}
