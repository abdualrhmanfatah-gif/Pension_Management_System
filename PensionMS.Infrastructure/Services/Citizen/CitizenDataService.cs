using Microsoft.EntityFrameworkCore;
using PensionMS.Core.DTOs.Citizen;
using PensionMS.Core.Interfaces;

namespace PensionMS.Infrastructure.Services.Citizen;

public class CitizenDataService : ICitizenDataService
{
    private readonly IUnitOfWork _unitOfWork;

    public CitizenDataService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CitizenProfileDto?> GetProfileAsync(int retireeId)
    {
        var retiree = await _unitOfWork.Retirees.GetByIdAsync(retireeId);
        if (retiree == null) return null;

        return new CitizenProfileDto
        {
            FullName = retiree.FullName,
            NationalID = retiree.NationalID,
            Phone = retiree.Phone,
            Address = retiree.Address,
            City = retiree.City,
            Governorate = retiree.Governorate,
            PersonalPhotoPath = retiree.PersonalPhotoPath,
            MilitaryNumber = retiree.MilitaryNumber,
            BirthDate = retiree.BirthDate
        };
    }

    public async Task<bool> UpdateProfileAsync(int retireeId, UpdateCitizenProfileDto dto)
    {
        var retiree = await _unitOfWork.Retirees.GetByIdAsync(retireeId);
        if (retiree == null) return false;

        if (!string.IsNullOrEmpty(dto.Phone)) retiree.Phone = dto.Phone;
        if (!string.IsNullOrEmpty(dto.Address)) retiree.Address = dto.Address;

        _unitOfWork.Retirees.Update(retiree);
        return await _unitOfWork.SaveChangesAsync() > 0;
    }

    public async Task<CitizenFileDto?> GetFileAsync(int retireeId)
    {
        var file = await _unitOfWork.RetirementFiles.Query()
            .Include(f => f.PensionBranch)
            .Include(f => f.Status)
            .FirstOrDefaultAsync(f => f.RetireeID == retireeId);

        if (file == null) return null;

        return new CitizenFileDto
        {
            FileID = file.FileID,
            FileNumber = file.FileNumber,
            Status = file.Status?.StatusName ?? "غير محدد",
            BranchName = file.PensionBranch?.BranchName ?? "غير محدد",
            OpeningDate = file.FileOpenDate
        };
    }

    public async Task<IEnumerable<CitizenEntitledPersonDto>> GetEntitledPersonsAsync(int retireeId)
    {
        var file = await _unitOfWork.RetirementFiles.FirstOrDefaultAsync(f => f.RetireeID == retireeId);
        if (file == null) return Enumerable.Empty<CitizenEntitledPersonDto>();

        var persons = await _unitOfWork.EntitledPersons.Query()
            .Include(p => p.Relationship)
            .Where(p => p.FileID == file.FileID)
            .ToListAsync();

        return persons.Select(p => new CitizenEntitledPersonDto
        {
            FullName = p.FullName,
            Relationship = p.Relationship?.RelationshipName ?? "غير محدد",
            Status = p.IsActive ? "فعال" : "موقوف"
        });
    }

    public async Task<IEnumerable<CitizenPaymentDto>> GetPaymentsAsync(int retireeId, int page, int pageSize)
    {
        var items = await _unitOfWork.SalaryPaymentItems.Query()
            .Include(i => i.Batch)
            .Include(i => i.Status)
            .Where(i => i.RetireeID == retireeId)
            .OrderByDescending(i => i.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return items.Select(i => new CitizenPaymentDto
        {
            Amount = i.Amount,
            PaymentDate = i.CreatedAt,
            Status = i.Status?.StatusName ?? "",
            Month = i.Batch?.BatchMonth.Month.ToString() ?? "",
            Year = i.Batch?.BatchMonth.Year ?? 0
        });
    }

    public async Task<CitizenPaymentSummaryDto> GetPaymentSummaryAsync(int retireeId)
    {
        var currentYear = DateTime.UtcNow.Year;
        var payments = await _unitOfWork.SalaryPaymentItems.Query()
            .Include(i => i.Batch)
            .Include(i => i.Status)
            .Where(i => i.RetireeID == retireeId && i.Status != null && i.Status.StatusName == "صرف ناجح")
            .ToListAsync();

        var lastPayment = payments.OrderByDescending(i => i.CreatedAt).FirstOrDefault();
        var entitlement = await _unitOfWork.FinancialEntitlements.Query()
            .Where(e => e.RetireeID == retireeId && (e.EndDate == null || e.EndDate > DateTime.UtcNow))
            .OrderByDescending(e => e.EffectiveDate)
            .FirstOrDefaultAsync();

        return new CitizenPaymentSummaryDto
        {
            TotalThisYear = payments.Where(p => p.Batch?.BatchMonth.Year == currentYear).Sum(p => p.Amount),
            LastPaymentAmount = lastPayment?.Amount ?? 0,
            LastPaymentDate = lastPayment?.CreatedAt,
            CurrentMonthlyEntitlement = entitlement?.Amount ?? 0
        };
    }
}
