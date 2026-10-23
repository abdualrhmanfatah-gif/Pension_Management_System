using PensionMS.Core.DTOs.RetirementFiles;
using PensionMS.Core.Entities;

namespace PensionMS.Infrastructure.Services.Pension.RetirementFile;

/// <summary>ينسخ الحقول إلى استجابة جهزت الخدمات المختصة نتائجها المحسوبة مسبقاً.</summary>
internal static class RetirementFileDtoMapper
{
    internal static RetirementFileDto MapToDto(
        PensionMS.Core.Entities.RetirementFile file, Retiree retiree, RetirementFileDto dto)
    {
        dto.FileID = file.FileID;
        dto.RetireeID = file.RetireeID;
        dto.FileNumber = file.FileNumber;
        dto.RetireeFullName = retiree.FullName;
        dto.NationalID = retiree.NationalID;
        dto.RankName = file.Rank?.RankName;
        dto.BranchName = file.PensionBranch?.BranchName;
        dto.MilitaryNumber = retiree.MilitaryNumber;
        dto.StatusID = file.StatusID;
        dto.PreviousStatusID = file.PreviousStatusID;
        dto.RetirementDate = file.RetirementDate;
        dto.ServiceStartDate = file.ServiceStartDate;
        dto.ServiceEndDate = file.ServiceEndDate;
        dto.FileOpenDate = file.FileOpenDate;
        dto.ClosedDate = file.ClosedDate;
        dto.StatusReason = file.StatusReason;
        dto.Notes = file.Notes;
        dto.CreatedAt = file.CreatedAt;
        dto.UpdatedAt = file.UpdatedAt;
        dto.AssignedEmployeeID = file.AssignedEmployeeID;
        dto.AssignedEmployeeName = file.AssignedEmployee?.FullName;
        dto.BirthDate = retiree.BirthDate;
        dto.Gender = retiree.Gender;
        dto.Address = retiree.Address;
        dto.DeathDate = retiree.DeathDate;
        dto.IsAlive = retiree.IsAlive;
        dto.PersonalPhotoPath = retiree.PersonalPhotoPath;
        dto.ReviewStatus = (int)file.ReviewStatus;
        dto.ReviewRequestedAt = file.ReviewRequestedAt;
        dto.ReviewCompletedAt = file.ReviewCompletedAt;
        dto.ReviewNotes = file.ReviewNotes;
        return dto;
    }
}
