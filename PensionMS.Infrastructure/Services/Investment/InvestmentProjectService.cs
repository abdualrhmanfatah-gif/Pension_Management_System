using Microsoft.EntityFrameworkCore;
using PensionMS.Core.DTOs.Common;
using PensionMS.Core.DTOs.Investment;
using PensionMS.Core.Entities.Investment;
using PensionMS.Core.Interfaces;

namespace PensionMS.Infrastructure.Services.Investment;

public class InvestmentProjectService : IInvestmentProjectService
{
    private readonly IUnitOfWork _unitOfWork;
<<<<<<< HEAD
    private readonly IAccessiblePensionRepository _access;
    public InvestmentProjectService(IUnitOfWork unitOfWork, IAccessiblePensionRepository access)
    {
        _unitOfWork = unitOfWork;
        _access = access;
=======
    public InvestmentProjectService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
    }

    public async Task<ApiResponse<int>> CreateProjectAsync(CreateProjectDto dto, int userId)
    {
<<<<<<< HEAD
        var queries = await _access.ForCurrentOperationAsync();
        queries.ResolveBranch(dto.PensionBranchID);
        var portfolio = await queries.Query(_unitOfWork.InvestmentPortfolios)
            .FirstOrDefaultAsync(p => p.PortfolioID == dto.PortfolioID);
=======
        var portfolio = await _unitOfWork.InvestmentPortfolios.GetByIdAsync(dto.PortfolioID);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        if (portfolio == null || portfolio.IsDeleted)
            return ApiResponse<int>.Fail("المحفظة الاستثمارية غير موجودة");

        var type = await _unitOfWork.InvestmentTypes.GetByIdAsync(dto.TypeID);
        if (type == null || type.IsDeleted)
            return ApiResponse<int>.Fail("نوع الاستثمار غير موجود");

        // Unique constraint check: Project Name must be unique within a Portfolio
<<<<<<< HEAD
        var exists = await queries.Query(_unitOfWork.InvestmentProjects)
=======
        var exists = await _unitOfWork.InvestmentProjects.Query()
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            .AnyAsync(p => p.PortfolioID == dto.PortfolioID && p.Name == dto.Name && !p.IsDeleted);
        if (exists)
            return ApiResponse<int>.Fail($"يوجد مشروع استثماري بنفس الاسم '{dto.Name}' في هذه المحفظة الاستثمارية");

        var project = new InvestmentProject
        {
            PortfolioID = dto.PortfolioID,
            TypeID = dto.TypeID,
            PensionBranchID = dto.PensionBranchID,
            Name = dto.Name,
            FeasibilityStudyPath = dto.FeasibilityStudyPath,
            BoardApprovalDocPath = dto.BoardApprovalDocPath,
            Status = dto.Status,
            PlannedBudget = dto.PlannedBudget,
            ExpectedROI = dto.ExpectedROI,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.InvestmentProjects.AddAsync(project);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<int>.Success(project.ProjectID, "تم إنشاء المشروع بنجاح");
    }

    public async Task<ApiResponse<bool>> UpdateProjectAsync(int id, UpdateProjectDto dto, int userId)
    {
<<<<<<< HEAD
        var queries = await _access.ForCurrentOperationAsync();
        var project = await queries.Query(_unitOfWork.InvestmentProjects)
            .FirstOrDefaultAsync(p => p.ProjectID == id);
        if (project == null || project.IsDeleted)
            return ApiResponse<bool>.Fail("المشروع غير موجود");

        queries.ResolveBranch(dto.PensionBranchID);
        var portfolio = await queries.Query(_unitOfWork.InvestmentPortfolios)
            .FirstOrDefaultAsync(p => p.PortfolioID == dto.PortfolioID);
=======
        var project = await _unitOfWork.InvestmentProjects.GetByIdAsync(id);
        if (project == null || project.IsDeleted)
            return ApiResponse<bool>.Fail("المشروع غير موجود");

        var portfolio = await _unitOfWork.InvestmentPortfolios.GetByIdAsync(dto.PortfolioID);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        if (portfolio == null || portfolio.IsDeleted)
            return ApiResponse<bool>.Fail("المحفظة الاستثمارية غير موجودة");

        var type = await _unitOfWork.InvestmentTypes.GetByIdAsync(dto.TypeID);
        if (type == null || type.IsDeleted)
            return ApiResponse<bool>.Fail("نوع الاستثمار غير موجود");

        // Unique constraint check: Project Name must be unique within a Portfolio
<<<<<<< HEAD
        var exists = await queries.Query(_unitOfWork.InvestmentProjects)
=======
        var exists = await _unitOfWork.InvestmentProjects.Query()
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            .AnyAsync(p => p.PortfolioID == dto.PortfolioID && p.Name == dto.Name && p.ProjectID != id && !p.IsDeleted);
        if (exists)
            return ApiResponse<bool>.Fail($"اسم المشروع '{dto.Name}' مستخدم بالفعل في هذه المحفظة");

        project.PortfolioID = dto.PortfolioID;
        project.TypeID = dto.TypeID;
        project.PensionBranchID = dto.PensionBranchID;
        project.Name = dto.Name;
        project.FeasibilityStudyPath = dto.FeasibilityStudyPath;
        project.BoardApprovalDocPath = dto.BoardApprovalDocPath;
        project.Status = dto.Status;
        project.PlannedBudget = dto.PlannedBudget;
        project.ExpectedROI = dto.ExpectedROI;
        project.StartDate = dto.StartDate;
        project.EndDate = dto.EndDate;
        var oldStatus = project.Status;
        project.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.InvestmentProjects.Update(project);
        await _unitOfWork.SaveChangesAsync();



        return ApiResponse<bool>.Success(true, "تم تحديث المشروع بنجاح");
    }

    public async Task<ApiResponse<bool>> DeleteProjectAsync(int id, int userId)
    {
<<<<<<< HEAD
        var queries = await _access.ForCurrentOperationAsync();
        var project = await queries.Query(_unitOfWork.InvestmentProjects)
            .FirstOrDefaultAsync(p => p.ProjectID == id);
=======
        var project = await _unitOfWork.InvestmentProjects.GetByIdAsync(id);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        if (project == null || project.IsDeleted)
            return ApiResponse<bool>.Fail("المشروع غير موجود");

        project.IsDeleted = true;
        project.DeletedAt = DateTime.UtcNow;
        project.DeletedByUserID = userId;

        _unitOfWork.InvestmentProjects.Update(project);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.Success(true, "تم حذف المشروع بنجاح");
    }

    public async Task<ApiResponse<ProjectDto>> GetProjectByIdAsync(int id)
    {
<<<<<<< HEAD
        var queries = await _access.ForCurrentOperationAsync();
        var project = await queries.Query(_unitOfWork.InvestmentProjects)
            .AsNoTracking()
=======
        var project = await _unitOfWork.InvestmentProjects.Query()
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            .Include(p => p.Portfolio)
            .Include(p => p.InvestmentType)
            .FirstOrDefaultAsync(p => p.ProjectID == id && !p.IsDeleted);

        if (project == null)
            return ApiResponse<ProjectDto>.Fail("المشروع غير موجود");

        var dto = new ProjectDto
        {
            ProjectID = project.ProjectID,
            PortfolioID = project.PortfolioID,
            PortfolioName = project.Portfolio?.Name ?? string.Empty,
            TypeID = project.TypeID,
            TypeName = project.InvestmentType?.TypeName ?? string.Empty,
            Name = project.Name,
            FeasibilityStudyPath = project.FeasibilityStudyPath,
            BoardApprovalDocPath = project.BoardApprovalDocPath,
            Status = project.Status,
            PlannedBudget = project.PlannedBudget,
            ExpectedROI = project.ExpectedROI,
            StartDate = project.StartDate.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
            EndDate = project.EndDate?.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture)
        };

        return ApiResponse<ProjectDto>.Success(dto);
    }

    public async Task<ApiResponse<List<ProjectDto>>> GetAllProjectsAsync()
    {
<<<<<<< HEAD
        var queries = await _access.ForCurrentOperationAsync();
        var projects = await queries.Query(_unitOfWork.InvestmentProjects)
            .AsNoTracking()
=======
        var projects = await _unitOfWork.InvestmentProjects.Query()
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            .Include(p => p.Portfolio)
            .Include(p => p.InvestmentType)
            .Where(p => !p.IsDeleted)
            .Select(p => new ProjectDto
            {
                ProjectID = p.ProjectID,
                PortfolioID = p.PortfolioID,
                PortfolioName = p.Portfolio != null ? p.Portfolio.Name : string.Empty,
                TypeID = p.TypeID,
                TypeName = p.InvestmentType != null ? p.InvestmentType.TypeName : string.Empty,
                Name = p.Name,
                FeasibilityStudyPath = p.FeasibilityStudyPath,
                BoardApprovalDocPath = p.BoardApprovalDocPath,
                Status = p.Status,
                PlannedBudget = p.PlannedBudget,
                ExpectedROI = p.ExpectedROI,
                StartDate = p.StartDate.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
                EndDate = p.EndDate.HasValue ? p.EndDate.Value.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture) : null
            })
            .ToListAsync();

        return ApiResponse<List<ProjectDto>>.Success(projects);
    }

    public async Task<ApiResponse<List<ProjectDto>>> GetProjectsByPortfolioAsync(int portfolioId)
    {
<<<<<<< HEAD
        var queries = await _access.ForCurrentOperationAsync();
        var projects = await queries.Query(_unitOfWork.InvestmentProjects)
            .AsNoTracking()
=======
        var projects = await _unitOfWork.InvestmentProjects.Query()
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            .Include(p => p.Portfolio)
            .Include(p => p.InvestmentType)
            .Where(p => p.PortfolioID == portfolioId && !p.IsDeleted)
            .Select(p => new ProjectDto
            {
                ProjectID = p.ProjectID,
                PortfolioID = p.PortfolioID,
                PortfolioName = p.Portfolio != null ? p.Portfolio.Name : string.Empty,
                TypeID = p.TypeID,
                TypeName = p.InvestmentType != null ? p.InvestmentType.TypeName : string.Empty,
                Name = p.Name,
                FeasibilityStudyPath = p.FeasibilityStudyPath,
                BoardApprovalDocPath = p.BoardApprovalDocPath,
                Status = p.Status,
                PlannedBudget = p.PlannedBudget,
                ExpectedROI = p.ExpectedROI,
                StartDate = p.StartDate.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
                EndDate = p.EndDate.HasValue ? p.EndDate.Value.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture) : null
            })
            .ToListAsync();

        return ApiResponse<List<ProjectDto>>.Success(projects);
    }

    public async Task<ApiResponse<ProjectSummaryDto>> GetProjectSummaryAsync(int id)
    {
<<<<<<< HEAD
        var queries = await _access.ForCurrentOperationAsync();
        var project = await queries.Query(_unitOfWork.InvestmentProjects)
            .AsNoTracking()
=======
        var project = await _unitOfWork.InvestmentProjects.Query()
            .Include(p => p.Properties.Where(pr => !pr.IsDeleted))
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            .FirstOrDefaultAsync(p => p.ProjectID == id && !p.IsDeleted);

        if (project == null)
            return ApiResponse<ProjectSummaryDto>.Fail("المشروع غير موجود");

<<<<<<< HEAD
        var propertyCount = await queries.Query(_unitOfWork.InvestmentProperties)
            .CountAsync(p => p.ProjectID == id && !p.IsDeleted);

=======
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        // Calculate actual spent from real transactions (Expenses only)
        // TODO: In a future phase, integrate with the new BankAccountService 
        // to retrieve actual investment expenses for this project.
        decimal totalSpent = 0; 

        var progress = project.PlannedBudget > 0 ? (totalSpent / project.PlannedBudget) * 100 : 0;

        var summary = new ProjectSummaryDto
        {
            ProjectID = project.ProjectID,
            Name = project.Name,
            Status = project.Status,
            PlannedBudget = project.PlannedBudget,
            TotalActualSpent = totalSpent,
<<<<<<< HEAD
            PropertyCount = propertyCount,
=======
            PropertyCount = project.Properties.Count,
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            ProgressPercentage = Math.Round(progress, 2)
        };

        return ApiResponse<ProjectSummaryDto>.Success(summary);
    }

    // ----------------------------------------------------
    // Investment Plans
    // ----------------------------------------------------

    public async Task<ApiResponse<List<InvestmentPlanDto>>> GetProjectPlansAsync(int projectId)
    {
<<<<<<< HEAD
        var queries = await _access.ForCurrentOperationAsync();
        var plans = await queries.Query(_unitOfWork.InvestmentPlans)
            .AsNoTracking()
=======
        var plans = await _unitOfWork.InvestmentPlans.Query()
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
            .Include(p => p.FiscalYear)
            .Where(p => p.ProjectID == projectId && !p.IsDeleted)
            .Select(p => new InvestmentPlanDto
            {
                PlanID = p.PlanID,
                ProjectID = p.ProjectID,
                FiscalYearID = p.FiscalYearID,
                YearName = p.FiscalYear != null ? p.FiscalYear.YearName : string.Empty,
                PlannedAmount = p.PlannedAmount,
                ActualSpent = p.ActualSpent
            })
            .ToListAsync();

        return ApiResponse<List<InvestmentPlanDto>>.Success(plans);
    }

    public async Task<ApiResponse<int>> CreatePlanAsync(CreateInvestmentPlanDto dto, int userId)
    {
<<<<<<< HEAD
        var queries = await _access.ForCurrentOperationAsync();
        var project = await queries.Query(_unitOfWork.InvestmentProjects)
            .FirstOrDefaultAsync(p => p.ProjectID == dto.ProjectID);
=======
        var project = await _unitOfWork.InvestmentProjects.GetByIdAsync(dto.ProjectID);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        if (project == null || project.IsDeleted)
            return ApiResponse<int>.Fail("المشروع غير موجود");

        var plan = new InvestmentPlan
        {
            ProjectID = dto.ProjectID,
            FiscalYearID = dto.FiscalYearID,
            PlannedAmount = dto.PlannedAmount,
            ActualSpent = dto.ActualSpent,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.InvestmentPlans.AddAsync(plan);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<int>.Success(plan.PlanID, "تمت إضافة خطة الموازنة بنجاح");
    }

    public async Task<ApiResponse<bool>> UpdatePlanAsync(int id, UpdateInvestmentPlanDto dto, int userId)
    {
<<<<<<< HEAD
        var queries = await _access.ForCurrentOperationAsync();
        var plan = await queries.Query(_unitOfWork.InvestmentPlans)
            .FirstOrDefaultAsync(p => p.PlanID == id);
=======
        var plan = await _unitOfWork.InvestmentPlans.GetByIdAsync(id);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        if (plan == null || plan.IsDeleted)
            return ApiResponse<bool>.Fail("الخطة غير موجودة");

        plan.FiscalYearID = dto.FiscalYearID;
        plan.PlannedAmount = dto.PlannedAmount;
        plan.ActualSpent = dto.ActualSpent;
        plan.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.InvestmentPlans.Update(plan);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.Success(true, "تم تحديث خطة الموازنة بنجاح");
    }

    public async Task<ApiResponse<bool>> DeletePlanAsync(int id, int userId)
    {
<<<<<<< HEAD
        var queries = await _access.ForCurrentOperationAsync();
        var plan = await queries.Query(_unitOfWork.InvestmentPlans)
            .FirstOrDefaultAsync(p => p.PlanID == id);
=======
        var plan = await _unitOfWork.InvestmentPlans.GetByIdAsync(id);
>>>>>>> 6248c767575198505e27d690a9dbf7172eff0ad1
        if (plan == null || plan.IsDeleted)
            return ApiResponse<bool>.Fail("الخطة غير موجودة");

        plan.IsDeleted = true;
        plan.DeletedAt = DateTime.UtcNow;
        plan.DeletedByUserID = userId;

        _unitOfWork.InvestmentPlans.Update(plan);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.Success(true, "تم حذف خطة الموازنة بنجاح");
    }
}
