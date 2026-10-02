using Microsoft.EntityFrameworkCore;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.HR;
using PensionMS.Core.Entities.HR;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Constants;

namespace PensionMS.Infrastructure.Services.HR;

public class HrEmployeeService : IHrEmployeeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IImmutableAuditService _auditService;
    private readonly IFileStorageService _fileStorageService;

    public HrEmployeeService(IUnitOfWork unitOfWork, IImmutableAuditService auditService, IFileStorageService fileStorageService)
    {
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _fileStorageService = fileStorageService;
    }

    public async Task<ApiResponse<PagedResult<EmployeeListItemDto>>> GetAllAsync(int page = 1, int pageSize = 20, string? search = null, int? cursor = null)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 20 : pageSize;

        var query = _unitOfWork.Employees.Query()
            .AsNoTracking()
            .Where(e => !e.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(e =>
                e.FullName.Contains(term) ||
                e.EmployeeNumber.Contains(term) ||
                e.NationalID.Contains(term));
        }

        if (cursor.HasValue)
        {
            var cursorItems = await query
                .Where(e => e.EmployeeID < cursor.Value)
                .OrderByDescending(e => e.EmployeeID)
                .Take(pageSize + 1)
                .Select(e => new EmployeeListItemDto
                {
                    EmployeeID = e.EmployeeID,
                    EmployeeNumber = e.EmployeeNumber,
                    FullName = e.FullName,
                    NationalID = e.NationalID,
                    DepartmentName = e.Department != null ? e.Department.Name : "غير محدد",
                    SectionName = e.Section != null ? e.Section.Name : null,
                    JobTitleID = e.JobTitleID,
                    JobTitleName = e.JobTitle != null ? e.JobTitle.Name : "غير محدد",
                    EmploymentStatusName = e.EmploymentStatus != null ? e.EmploymentStatus.Name : "غير محدد",
                    ProfilePhotoPath = e.ProfilePhotoPath,
                    HireDate = e.HireDate,
                    IsActive = true
                })
                .ToListAsync();

            bool hasMore = cursorItems.Count > pageSize;
            if (hasMore) cursorItems = cursorItems.Take(pageSize).ToList();

            var cursorResult = new PagedResult<EmployeeListItemDto>
            {
                Items = cursorItems,
                TotalCount = 0,
                Page = page,
                PageSize = pageSize,
                NextCursor = cursorItems.Count > 0 ? cursorItems.Last().EmployeeID : null,
                HasMore = hasMore
            };

            return ApiResponse<PagedResult<EmployeeListItemDto>>.Success(cursorResult);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(e => e.JobTitleID)
            .ThenBy(e => e.FullName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new EmployeeListItemDto
            {
                EmployeeID = e.EmployeeID,
                EmployeeNumber = e.EmployeeNumber,
                FullName = e.FullName,
                NationalID = e.NationalID,
                DepartmentName = e.Department != null ? e.Department.Name : "غير محدد",
                SectionName = e.Section != null ? e.Section.Name : null,
                JobTitleID = e.JobTitleID,
                JobTitleName = e.JobTitle != null ? e.JobTitle.Name : "غير محدد",
                EmploymentStatusName = e.EmploymentStatus != null ? e.EmploymentStatus.Name : "غير محدد",
                ProfilePhotoPath = e.ProfilePhotoPath,
                HireDate = e.HireDate,
                IsActive = true
            })
            .ToListAsync();

        var result = new PagedResult<EmployeeListItemDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
        
        return ApiResponse<PagedResult<EmployeeListItemDto>>.Success(result);
    }

    public async Task<ApiResponse<EmployeeDetailsDto>> GetByIdAsync(int id)
    {
        var result = await _unitOfWork.Employees.Query()
            .AsNoTracking()
            .Where(e => e.EmployeeID == id && !e.IsDeleted)
            .Select(e => new EmployeeDetailsDto
            {
                EmployeeID = e.EmployeeID,
                DepartmentID = e.DepartmentID,
                SectionID = e.SectionID,
                JobTitleID = e.JobTitleID,
                EmploymentStatusID = e.EmploymentStatusID,
                EmployeeNumber = e.EmployeeNumber,
                FullName = e.FullName,
                NationalID = e.NationalID,
                Phone = e.Phone,
                BirthPlace = e.BirthPlace,
                MaritalStatus = e.MaritalStatus,
                MilitaryNumber = e.MilitaryNumber,
                RankID = e.RankID,
                RankName = e.Rank != null ? e.Rank.RankName : null,
                MilitaryServiceDate = e.MilitaryServiceDate,
                JobPositions = e.JobPositions,
                AcademicQualifications = e.AcademicQualifications,
                Address = e.Address,
                Notes = e.Notes,
                BloodGroup = e.BloodGroup,
                NationalIDIssuePlace = e.NationalIDIssuePlace,
                NationalIDIssueDate = e.NationalIDIssueDate,
                NationalIDExpiryDate = e.NationalIDExpiryDate,
                EmergencyContactName = e.EmergencyContactName,
                EmergencyContactPhone = e.EmergencyContactPhone,
                Gender = e.Gender,
                HireDate = e.HireDate,
                BirthDate = e.BirthDate,
                DepartmentName = e.Department != null ? e.Department.Name : "غير محدد",
                SectionName = e.Section != null ? e.Section.Name : null,
                JobTitleName = e.JobTitle != null ? e.JobTitle.Name : "غير محدد",
                EmploymentStatusName = e.EmploymentStatus != null ? e.EmploymentStatus.Name : "غير محدد",
                ProfilePhotoPath = e.ProfilePhotoPath,
                IsInService = true,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt
            })
            .FirstOrDefaultAsync();
            
        if (result == null) return ApiResponse<EmployeeDetailsDto>.Fail("الموظف غير موجود");
        return ApiResponse<EmployeeDetailsDto>.Success(result);
    }

    public async Task<ApiResponse<EmployeeDetailsDto>> CreateAsync(CreateEmployeeDto dto, int userId)
    {
        try 
        {
            await ValidateReferencesAsync(dto.DepartmentID, dto.SectionID, dto.JobTitleID, dto.EmploymentStatusID, dto.RankID);
        }
        catch (InvalidOperationException ex)
        {
            return ApiResponse<EmployeeDetailsDto>.Fail("حدث خطأ أثناء إنشاء الموظف. يرجى المحاولة لاحقاً");
        }

        var employeeNumber = NormalizeOptional(dto.EmployeeNumber);
        var fullName = NormalizeOptional(dto.FullName);
        var nationalId = NormalizeOptional(dto.NationalID) ?? string.Empty;

        if (string.IsNullOrEmpty(employeeNumber)) return ApiResponse<EmployeeDetailsDto>.Fail("رقم الموظف مطلوب");
        if (string.IsNullOrEmpty(fullName)) return ApiResponse<EmployeeDetailsDto>.Fail("الاسم الكامل مطلوب");

        if (await _unitOfWork.Employees.AnyAsync(e => e.EmployeeNumber == employeeNumber && !e.IsDeleted))
            return ApiResponse<EmployeeDetailsDto>.Fail("رقم الموظف موجود مسبقاً");

        var employee = new Employee
        {
            DepartmentID = dto.DepartmentID,
            SectionID = dto.SectionID,
            JobTitleID = dto.JobTitleID,
            EmploymentStatusID = dto.EmploymentStatusID,
            EmployeeNumber = employeeNumber,
            FullName = fullName,
            NationalID = nationalId,
            Phone = NormalizeOptional(dto.Phone),
            BirthPlace = NormalizeOptional(dto.BirthPlace),
            MaritalStatus = NormalizeOptional(dto.MaritalStatus),
            MilitaryNumber = NormalizeOptional(dto.MilitaryNumber),
            RankID = dto.RankID,
            MilitaryServiceDate = dto.MilitaryServiceDate,
            JobPositions = NormalizeOptional(dto.JobPositions),
            AcademicQualifications = NormalizeOptional(dto.AcademicQualifications),
            Address = NormalizeOptional(dto.Address),
            Notes = NormalizeOptional(dto.Notes),
            BloodGroup = NormalizeOptional(dto.BloodGroup),
            NationalIDIssuePlace = NormalizeOptional(dto.NationalIDIssuePlace),
            NationalIDIssueDate = dto.NationalIDIssueDate,
            NationalIDExpiryDate = dto.NationalIDExpiryDate,
            EmergencyContactName = NormalizeOptional(dto.EmergencyContactName),
            EmergencyContactPhone = NormalizeOptional(dto.EmergencyContactPhone),
            Gender = dto.Gender,
            HireDate = dto.HireDate,
            BirthDate = dto.BirthDate,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Employees.AddAsync(employee);
        
        // Log to Service History
        employee.ServiceHistoryEvents.Add(new ServiceHistoryEvent
        {
            EventType = HrEventTypeConstants.AddEmployee,
            EventDate = DateTime.UtcNow,
            Summary = $"تمت إضافة الموظف {fullName} برقم وظيفي {employeeNumber}",
            SourceEntityType = nameof(Employee),
            CreatedAt = DateTime.UtcNow
        });

        await _unitOfWork.SaveChangesAsync();

        await _auditService.LogAsync(userId, "", "Create", "Employee", employee.EmployeeID, null, System.Text.Json.JsonSerializer.Serialize(dto));

        var result = await GetByIdAsync(employee.EmployeeID);
        return ApiResponse<EmployeeDetailsDto>.Success(result.Data!, "تم إنشاء الموظف بنجاح");
    }

    public async Task<ApiResponse<EmployeeDetailsDto>> UpdateAsync(int id, UpdateEmployeeDto dto, int userId)
    {
        var employee = await _unitOfWork.Employees.Query()
            .FirstOrDefaultAsync(e => e.EmployeeID == id && !e.IsDeleted);
        if (employee == null) return ApiResponse<EmployeeDetailsDto>.Fail("الموظف غير موجود");

        var oldValues = System.Text.Json.JsonSerializer.Serialize(employee);

        var departmentId = dto.DepartmentID ?? employee.DepartmentID;
        var sectionId = dto.SectionID.HasValue ? dto.SectionID : employee.SectionID;
        var jobTitleId = dto.JobTitleID ?? employee.JobTitleID;
        var employmentStatusId = dto.EmploymentStatusID ?? employee.EmploymentStatusID;
        var rankId = dto.RankID ?? employee.RankID;

        try 
        {
            await ValidateReferencesAsync(departmentId, sectionId, jobTitleId, employmentStatusId, rankId);
        }
        catch (InvalidOperationException ex)
        {
            return ApiResponse<EmployeeDetailsDto>.Fail("حدث خطأ أثناء تحديث بيانات الموظف. يرجى المحاولة لاحقاً");
        }

        if (dto.DepartmentID.HasValue) employee.DepartmentID = dto.DepartmentID.Value;
        if (dto.SectionID.HasValue) employee.SectionID = dto.SectionID.Value;
        if (dto.JobTitleID.HasValue) employee.JobTitleID = dto.JobTitleID.Value;
        if (dto.EmploymentStatusID.HasValue) employee.EmploymentStatusID = dto.EmploymentStatusID.Value;

        if (dto.FullName != null) employee.FullName = dto.FullName.Trim();
        if (dto.NationalID != null) employee.NationalID = NormalizeOptional(dto.NationalID) ?? string.Empty;
        if (dto.Phone != null) employee.Phone = NormalizeOptional(dto.Phone);
        if (dto.BirthPlace != null) employee.BirthPlace = NormalizeOptional(dto.BirthPlace);
        if (dto.MaritalStatus != null) employee.MaritalStatus = NormalizeOptional(dto.MaritalStatus);
        if (dto.MilitaryNumber != null) employee.MilitaryNumber = NormalizeOptional(dto.MilitaryNumber);
        if (dto.RankID.HasValue) employee.RankID = dto.RankID.Value;
        if (dto.MilitaryServiceDate.HasValue) employee.MilitaryServiceDate = dto.MilitaryServiceDate;
        if (dto.JobPositions != null) employee.JobPositions = NormalizeOptional(dto.JobPositions);
        if (dto.AcademicQualifications != null) employee.AcademicQualifications = NormalizeOptional(dto.AcademicQualifications);
        if (dto.Address != null) employee.Address = NormalizeOptional(dto.Address);
        if (dto.Notes != null) employee.Notes = NormalizeOptional(dto.Notes);
        if (dto.BloodGroup != null) employee.BloodGroup = NormalizeOptional(dto.BloodGroup);
        if (dto.NationalIDIssuePlace != null) employee.NationalIDIssuePlace = NormalizeOptional(dto.NationalIDIssuePlace);
        if (dto.NationalIDIssueDate.HasValue) employee.NationalIDIssueDate = dto.NationalIDIssueDate;
        if (dto.NationalIDExpiryDate.HasValue) employee.NationalIDExpiryDate = dto.NationalIDExpiryDate;
        if (dto.EmergencyContactName != null) employee.EmergencyContactName = NormalizeOptional(dto.EmergencyContactName);
        if (dto.EmergencyContactPhone != null) employee.EmergencyContactPhone = NormalizeOptional(dto.EmergencyContactPhone);
        if (dto.Gender.HasValue) employee.Gender = dto.Gender.Value;
        if (dto.HireDate.HasValue) employee.HireDate = dto.HireDate.Value;
        if (dto.BirthDate.HasValue) employee.BirthDate = dto.BirthDate.Value;

        employee.UpdatedAt = DateTime.UtcNow;

        // Log significant changes to Service History
        employee.ServiceHistoryEvents.Add(new ServiceHistoryEvent
        {
            EventType = HrEventTypeConstants.EditData,
            EventDate = DateTime.UtcNow,
            Summary = "تم تحديث البيانات الأساسية للموظف",
            SourceEntityType = nameof(Employee),
            SourceEntityId = employee.EmployeeID,
            CreatedAt = DateTime.UtcNow
        });

        await _unitOfWork.SaveChangesAsync();

        await _auditService.LogAsync(userId, "", "Update", "Employee", id, oldValues, System.Text.Json.JsonSerializer.Serialize(dto));

        var result = await GetByIdAsync(id);
        return ApiResponse<EmployeeDetailsDto>.Success(result.Data!, "تم تحديث بيانات الموظف بنجاح");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(int id, int userId)
    {
        var employee = await _unitOfWork.Employees.Query()
            .FirstOrDefaultAsync(e => e.EmployeeID == id && !e.IsDeleted);
        if (employee == null) return ApiResponse<bool>.Fail("الموظف غير موجود");

        var oldValues = System.Text.Json.JsonSerializer.Serialize(employee);

        employee.IsDeleted = true;
        employee.DeletedAt = DateTime.UtcNow;
        employee.DeletedByUserID = userId;
        employee.UpdatedAt = DateTime.UtcNow;

        employee.ServiceHistoryEvents.Add(new ServiceHistoryEvent
        {
            EventType = HrEventTypeConstants.DeleteEmployee,
            EventDate = DateTime.UtcNow,
            Summary = "تم وضع علامة الحذف على الموظف (حذف منطقي)",
            SourceEntityType = nameof(Employee),
            SourceEntityId = employee.EmployeeID,
            CreatedAt = DateTime.UtcNow
        });

        await _unitOfWork.SaveChangesAsync();

        await _auditService.LogAsync(userId, "", "Delete", "Employee", id, oldValues, null);

        return ApiResponse<bool>.Success(true, "تم حذف الموظف بنجاح");
    }

    public async Task<ApiResponse<HrStatsDto>> GetStatsAsync()
    {
        var totalEmployees = await _unitOfWork.Employees.Query().Where(e => !e.IsDeleted).CountAsync();
        var pendingLeaves = await _unitOfWork.LeaveRequests.Query().Where(l => !l.IsDeleted && l.Status == LeaveStatusConstants.Pending).CountAsync();

        var now = DateTime.UtcNow;
        var onLeave = await _unitOfWork.LeaveRequests.Query()
            .Where(l => !l.IsDeleted && l.Status == LeaveStatusConstants.Approved && l.StartDate <= now && l.EndDate >= now)            .CountAsync();

        var stats = new HrStatsDto
        {
            TotalEmployees = totalEmployees,
            PendingLeaveRequests = pendingLeaves,
            EmployeesOnLeave = onLeave
        };

        return ApiResponse<HrStatsDto>.Success(stats);
    }

    public async Task<ApiResponse<PagedResult<HrGlobalDocumentDto>>> GetGlobalEmployeeDocumentsAsync(int page = 1, int pageSize = 20, string? search = null)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 20 : pageSize;

        var query = _unitOfWork.Documents.Query()
            .AsNoTracking()
            .Where(d => d.OwnerType == PensionMS.Core.Entities.DocumentOwnerType.Employee && !d.IsDeleted && d.EmployeeID != null);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(d => 
                d.FileName.Contains(term) ||
                (d.Employee != null && d.Employee.FullName.Contains(term)) ||
                (d.Employee != null && d.Employee.EmployeeNumber.Contains(term))
            );
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(d => d.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(d => new HrGlobalDocumentDto
            {
                DocumentID = d.DocumentID,
                FileName = d.FileName,
                FilePath = d.FilePath,
                FileExtension = Path.GetExtension(d.FileName),
                FileSize = d.FileSize ?? 0,
                CreatedAt = d.CreatedAt,
                DocumentTypeID = d.DocumentTypeID,
                DocumentTypeName = d.DocumentType.TypeName,
                UploadedByUser = d.UploadedByUser != null ? d.UploadedByUser.FullName : null,
                EmployeeID = d.EmployeeID ?? 0,
                EmployeeFullName = d.Employee != null ? d.Employee.FullName : "غير متوفر",
                EmployeeNumber = d.Employee != null ? d.Employee.EmployeeNumber : "غير متوفر"
            })
            .ToListAsync();

        var result = new PagedResult<HrGlobalDocumentDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };

        return ApiResponse<PagedResult<HrGlobalDocumentDto>>.Success(result);
    }

    public async Task<ApiResponse<string>> UploadPhotoAsync(int id, Stream fileStream, string fileName, int userId)
    {
        var employee = await _unitOfWork.Employees.Query()
            .FirstOrDefaultAsync(e => e.EmployeeID == id && !e.IsDeleted);
        if (employee == null) return ApiResponse<string>.Fail("الموظف غير موجود");

        // Delete old photo if exists
        if (!string.IsNullOrEmpty(employee.ProfilePhotoPath))
        {
            await _fileStorageService.DeleteFileAsync(employee.ProfilePhotoPath);
        }

        // Save new photo
        var folderName = $"employees/{id}/profile";
        var filePath = await _fileStorageService.SaveFileAsync(fileStream, fileName, folderName);

        employee.ProfilePhotoPath = filePath;
        employee.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        await _auditService.LogAsync(userId, "", "Update", "EmployeePhoto", id, null, filePath);

        return ApiResponse<string>.Success(filePath, "تم رفع الصورة الشخصية بنجاح");
    }

    private async Task ValidateReferencesAsync(int departmentId, int? sectionId, int jobTitleId, int employmentStatusId, int? rankId)
    {
        if (!await _unitOfWork.Departments.AnyAsync(d => d.DepartmentID == departmentId && !d.IsDeleted))
            throw new InvalidOperationException("Department was not found.");

        if (sectionId.HasValue)
        {
            var section = await _unitOfWork.Sections.Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.SectionID == sectionId.Value && !s.IsDeleted);

            if (section == null)
                throw new InvalidOperationException("Section was not found.");

            if (section.DepartmentID != departmentId)
                throw new InvalidOperationException("Section does not belong to the selected department.");
        }

        if (!await _unitOfWork.JobTitles.AnyAsync(j => j.JobTitleID == jobTitleId && !j.IsDeleted))
            throw new InvalidOperationException("Job title was not found.");

        if (!await _unitOfWork.EmploymentStatuses.AnyAsync(s => s.EmploymentStatusID == employmentStatusId && !s.IsDeleted))
            throw new InvalidOperationException("Employment status was not found.");

        if (rankId.HasValue && !await _unitOfWork.Ranks.Query().AsNoTracking().AnyAsync(r => r.RankID == rankId.Value))
            throw new InvalidOperationException("Rank was not found.");
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
