using PensionMS.Core.DTOs.HR;
using PensionMS.Core.DTOs.Common;

namespace PensionMS.Core.Interfaces;

public interface IHrReferenceDataService
{
    Task<ApiResponse<List<DepartmentDto>>> GetDepartmentsAsync();
    Task<ApiResponse<DepartmentDto>> GetDepartmentByIdAsync(int id);
    Task<ApiResponse<DepartmentDto>> CreateDepartmentAsync(CreateDepartmentDto dto, int userId);
    Task<ApiResponse<DepartmentDto>> UpdateDepartmentAsync(int id, UpdateDepartmentDto dto, int userId);
    Task<ApiResponse<bool>> DeleteDepartmentAsync(int id, int userId);

    Task<ApiResponse<List<SectionDto>>> GetSectionsAsync(int? departmentId = null);
    Task<ApiResponse<SectionDto>> GetSectionByIdAsync(int id);
    Task<ApiResponse<SectionDto>> CreateSectionAsync(CreateSectionDto dto, int userId);
    Task<ApiResponse<SectionDto>> UpdateSectionAsync(int id, UpdateSectionDto dto, int userId);
    Task<ApiResponse<bool>> DeleteSectionAsync(int id, int userId);

    Task<ApiResponse<List<JobTitleDto>>> GetJobTitlesAsync();
    Task<ApiResponse<JobTitleDto>> GetJobTitleByIdAsync(int id);
    Task<ApiResponse<JobTitleDto>> CreateJobTitleAsync(CreateJobTitleDto dto, int userId);
    Task<ApiResponse<JobTitleDto>> UpdateJobTitleAsync(int id, UpdateJobTitleDto dto, int userId);
    Task<ApiResponse<bool>> DeleteJobTitleAsync(int id, int userId);

    Task<ApiResponse<List<EmploymentStatusDto>>> GetEmploymentStatusesAsync();
    Task<ApiResponse<List<LeaveTypeDto>>> GetLeaveTypesAsync();
    Task<ApiResponse<List<RankDto>>> GetRanksAsync();
}
