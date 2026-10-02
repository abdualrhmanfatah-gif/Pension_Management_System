using Microsoft.EntityFrameworkCore;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.Entities.HR;
using PensionMS.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PensionMS.Core.DTOs.HR;

namespace PensionMS.Infrastructure.Services.HR;

public class HrReferenceDataService : IHrReferenceDataService
{
    private readonly IUnitOfWork _unitOfWork;

    public HrReferenceDataService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // ── Departments ─────────────────────────────────────

    public async Task<ApiResponse<List<DepartmentDto>>> GetDepartmentsAsync()
    {
        var result = await _unitOfWork.Departments.Query()
            .AsNoTracking()
            .Where(d => !d.IsDeleted)
            .OrderBy(d => d.Name)
            .Select(d => new DepartmentDto
            {
                DepartmentID = d.DepartmentID,
                Code = d.Code,
                Name = d.Name,
                Description = d.Description,
                IsActive = d.IsActive,
                SectionsCount = d.Sections.Count(s => !s.IsDeleted),
                EmployeesCount = d.Employees.Count(e => !e.IsDeleted)
            })
            .ToListAsync();

        return ApiResponse<List<DepartmentDto>>.Success(result);
    }

    public async Task<ApiResponse<DepartmentDto>> GetDepartmentByIdAsync(int id)
    {
        var d = await _unitOfWork.Departments.Query()
            .AsNoTracking()
            .Where(x => x.DepartmentID == id && !x.IsDeleted)
            .Select(d => new DepartmentDto
            {
                DepartmentID = d.DepartmentID,
                Code = d.Code,
                Name = d.Name,
                Description = d.Description,
                IsActive = d.IsActive,
                SectionsCount = d.Sections.Count(s => !s.IsDeleted),
                EmployeesCount = d.Employees.Count(e => !e.IsDeleted)
            })
            .FirstOrDefaultAsync();

        if (d == null) return ApiResponse<DepartmentDto>.Fail("الإدارة غير موجودة");

        return ApiResponse<DepartmentDto>.Success(d);
    }

    public async Task<ApiResponse<DepartmentDto>> CreateDepartmentAsync(CreateDepartmentDto dto, int userId)
    {
        var dept = new Department
        {
            Code = dto.Code,
            Name = dto.Name,
            Description = dto.Description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Departments.AddAsync(dept);
        await _unitOfWork.SaveChangesAsync();

        return await GetDepartmentByIdAsync(dept.DepartmentID);
    }

    public async Task<ApiResponse<DepartmentDto>> UpdateDepartmentAsync(int id, UpdateDepartmentDto dto, int userId)
    {
        var dept = await _unitOfWork.Departments.GetByIdAsync(id);
        if (dept == null || dept.IsDeleted) return ApiResponse<DepartmentDto>.Fail("الإدارة غير موجودة");

        if (dto.Code != null) dept.Code = dto.Code;
        if (dto.Name != null) dept.Name = dto.Name;
        if (dto.Description != null) dept.Description = dto.Description;
        if (dto.IsActive.HasValue) dept.IsActive = dto.IsActive.Value;
        dept.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Departments.Update(dept);
        await _unitOfWork.SaveChangesAsync();
        return await GetDepartmentByIdAsync(dept.DepartmentID);
    }

    public async Task<ApiResponse<bool>> DeleteDepartmentAsync(int id, int userId)
    {
        var dept = await _unitOfWork.Departments.Query()
            .Include(d => d.Sections)
            .Include(d => d.Employees)
            .FirstOrDefaultAsync(x => x.DepartmentID == id && !x.IsDeleted);
            
        if (dept == null) return ApiResponse<bool>.Fail("الإدارة غير موجودة");

        if (dept.Sections.Any(s => !s.IsDeleted) || dept.Employees.Any(e => !e.IsDeleted))
            return ApiResponse<bool>.Fail("لا يمكن حذف الإدارة لوجود أقسام أو موظفين مرتبطين بها");

        dept.IsDeleted = true;
        dept.DeletedAt = DateTime.UtcNow;
        dept.DeletedByUserID = userId;
        
        _unitOfWork.Departments.Update(dept);
        await _unitOfWork.SaveChangesAsync();
        return ApiResponse<bool>.Success(true);
    }

    // ── Sections ────────────────────────────────────────

    public async Task<ApiResponse<List<SectionDto>>> GetSectionsAsync(int? departmentId = null)
    {
        var query = _unitOfWork.Sections.Query()
            .AsNoTracking()
            .Where(s => !s.IsDeleted);
            
        if (departmentId.HasValue)
            query = query.Where(s => s.DepartmentID == departmentId.Value);

        var result = await query
            .OrderBy(s => s.Name)
            .Select(s => new SectionDto
            {
                SectionID = s.SectionID,
                DepartmentID = s.DepartmentID,
                DepartmentName = s.Department != null ? s.Department.Name : "",
                Code = s.Code,
                Name = s.Name,
                Description = s.Description,
                IsActive = s.IsActive,
                EmployeesCount = s.Employees.Count(e => !e.IsDeleted)
            })
            .ToListAsync();

        return ApiResponse<List<SectionDto>>.Success(result);
    }

    public async Task<ApiResponse<SectionDto>> GetSectionByIdAsync(int id)
    {
        var s = await _unitOfWork.Sections.Query()
            .AsNoTracking()
            .Where(x => x.SectionID == id && !x.IsDeleted)
            .Select(s => new SectionDto
            {
                SectionID = s.SectionID,
                DepartmentID = s.DepartmentID,
                DepartmentName = s.Department != null ? s.Department.Name : "",
                Code = s.Code,
                Name = s.Name,
                Description = s.Description,
                IsActive = s.IsActive,
                EmployeesCount = s.Employees.Count(e => !e.IsDeleted)
            })
            .FirstOrDefaultAsync();

        if (s == null) return ApiResponse<SectionDto>.Fail("القسم غير موجود");

        return ApiResponse<SectionDto>.Success(s);
    }

    public async Task<ApiResponse<SectionDto>> CreateSectionAsync(CreateSectionDto dto, int userId)
    {
        var section = new Section
        {
            DepartmentID = dto.DepartmentID,
            Code = dto.Code,
            Name = dto.Name,
            Description = dto.Description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Sections.AddAsync(section);
        await _unitOfWork.SaveChangesAsync();

        return await GetSectionByIdAsync(section.SectionID);
    }

    public async Task<ApiResponse<SectionDto>> UpdateSectionAsync(int id, UpdateSectionDto dto, int userId)
    {
        var section = await _unitOfWork.Sections.GetByIdAsync(id);
        if (section == null || section.IsDeleted) return ApiResponse<SectionDto>.Fail("القسم غير موجود");

        if (dto.DepartmentID.HasValue) section.DepartmentID = dto.DepartmentID.Value;
        if (dto.Code != null) section.Code = dto.Code;
        if (dto.Name != null) section.Name = dto.Name;
        if (dto.Description != null) section.Description = dto.Description;
        if (dto.IsActive.HasValue) section.IsActive = dto.IsActive.Value;
        section.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Sections.Update(section);
        await _unitOfWork.SaveChangesAsync();
        return await GetSectionByIdAsync(section.SectionID);
    }

    public async Task<ApiResponse<bool>> DeleteSectionAsync(int id, int userId)
    {
        var section = await _unitOfWork.Sections.Query()
            .Include(s => s.Employees)
            .FirstOrDefaultAsync(x => x.SectionID == id && !x.IsDeleted);
            
        if (section == null) return ApiResponse<bool>.Fail("القسم غير موجود");

        if (section.Employees.Any(e => !e.IsDeleted))
            return ApiResponse<bool>.Fail("لا يمكن حذف القسم لوجود موظفين مرتبطين به");

        section.IsDeleted = true;
        section.DeletedAt = DateTime.UtcNow;
        section.DeletedByUserID = userId;
        
        _unitOfWork.Sections.Update(section);
        await _unitOfWork.SaveChangesAsync();
        return ApiResponse<bool>.Success(true);
    }

    // ── Job Titles ──────────────────────────────────────

    public async Task<ApiResponse<List<JobTitleDto>>> GetJobTitlesAsync()
    {
        var result = await _unitOfWork.JobTitles.Query()
            .AsNoTracking()
            .Where(j => !j.IsDeleted)
            .OrderBy(j => j.Name)
            .Select(j => new JobTitleDto
            {
                JobTitleID = j.JobTitleID,
                Code = j.Code,
                Name = j.Name,
                Description = j.Description,
                IsActive = j.IsActive,
                EmployeesCount = j.Employees.Count(e => !e.IsDeleted)
            })
            .ToListAsync();

        return ApiResponse<List<JobTitleDto>>.Success(result);
    }

    public async Task<ApiResponse<JobTitleDto>> GetJobTitleByIdAsync(int id)
    {
        var j = await _unitOfWork.JobTitles.Query()
            .AsNoTracking()
            .Where(x => x.JobTitleID == id && !x.IsDeleted)
            .Select(j => new JobTitleDto
            {
                JobTitleID = j.JobTitleID,
                Code = j.Code,
                Name = j.Name,
                Description = j.Description,
                IsActive = j.IsActive,
                EmployeesCount = j.Employees.Count(e => !e.IsDeleted)
            })
            .FirstOrDefaultAsync();

        if (j == null) return ApiResponse<JobTitleDto>.Fail("المسمى الوظيفي غير موجود");

        return ApiResponse<JobTitleDto>.Success(j);
    }

    public async Task<ApiResponse<JobTitleDto>> CreateJobTitleAsync(CreateJobTitleDto dto, int userId)
    {
        var job = new JobTitle
        {
            Code = dto.Code,
            Name = dto.Name,
            Description = dto.Description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.JobTitles.AddAsync(job);
        await _unitOfWork.SaveChangesAsync();

        return await GetJobTitleByIdAsync(job.JobTitleID);
    }

    public async Task<ApiResponse<JobTitleDto>> UpdateJobTitleAsync(int id, UpdateJobTitleDto dto, int userId)
    {
        var job = await _unitOfWork.JobTitles.GetByIdAsync(id);
        if (job == null || job.IsDeleted) return ApiResponse<JobTitleDto>.Fail("المسمى الوظيفي غير موجود");

        if (dto.Code != null) job.Code = dto.Code;
        if (dto.Name != null) job.Name = dto.Name;
        if (dto.Description != null) job.Description = dto.Description;
        if (dto.IsActive.HasValue) job.IsActive = dto.IsActive.Value;
        job.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.JobTitles.Update(job);
        await _unitOfWork.SaveChangesAsync();
        return await GetJobTitleByIdAsync(job.JobTitleID);
    }

    public async Task<ApiResponse<bool>> DeleteJobTitleAsync(int id, int userId)
    {
        var job = await _unitOfWork.JobTitles.Query()
            .Include(j => j.Employees)
            .FirstOrDefaultAsync(x => x.JobTitleID == id && !x.IsDeleted);
            
        if (job == null) return ApiResponse<bool>.Fail("المسمى الوظيفي غير موجود");

        if (job.Employees.Any(e => !e.IsDeleted))
            return ApiResponse<bool>.Fail("لا يمكن حذف المسمى الوظيفي لوجود موظفين مرتبطين به");

        job.IsDeleted = true;
        job.DeletedAt = DateTime.UtcNow;
        job.DeletedByUserID = userId;
        
        _unitOfWork.JobTitles.Update(job);
        await _unitOfWork.SaveChangesAsync();
        return ApiResponse<bool>.Success(true);
    }

    // ── Enums & Lookups ─────────────────────────────────

    public async Task<ApiResponse<List<EmploymentStatusDto>>> GetEmploymentStatusesAsync()
    {
        var result = await _unitOfWork.EmploymentStatuses.Query()
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .Select(s => new EmploymentStatusDto
            {
                EmploymentStatusID = s.EmploymentStatusID,
                Code = s.Code,
                Name = s.Name,
                Description = s.Description,
                IsActive = s.IsActive,
                IsInService = s.IsInService,
                EmployeesCount = s.Employees.Count(e => !e.IsDeleted)
            })
            .ToListAsync();

        return ApiResponse<List<EmploymentStatusDto>>.Success(result);
    }

    public async Task<ApiResponse<List<LeaveTypeDto>>> GetLeaveTypesAsync()
    {
        var result = await _unitOfWork.LeaveTypes.Query()
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .Select(t => new LeaveTypeDto
            {
                LeaveTypeID = t.LeaveTypeID,
                Code = t.Code,
                Name = t.Name,
                Description = t.Description,
                IsActive = t.IsActive
            })
            .ToListAsync();

        return ApiResponse<List<LeaveTypeDto>>.Success(result);
    }

    public async Task<ApiResponse<List<RankDto>>> GetRanksAsync()
    {
        var result = await _unitOfWork.Ranks.Query()
            .AsNoTracking()
            .OrderBy(r => r.RankLevel)
            .Select(r => new RankDto
            {
                RankID = r.RankID,
                RankName = r.RankName,
                RankLevel = r.RankLevel,
                BaseSalary = r.BaseSalary
            })
            .ToListAsync();

        return ApiResponse<List<RankDto>>.Success(result);
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
