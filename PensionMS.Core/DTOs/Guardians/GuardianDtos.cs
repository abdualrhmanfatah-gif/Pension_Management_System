using PensionMS.Core.Interfaces;

namespace PensionMS.Core.DTOs.Guardians;

public class AssignGuardianDto
{
    public int FileID { get; set; }
    public List<int> EntitledIDs { get; set; } = new();
    public string FullName { get; set; } = string.Empty;
    public string NationalID { get; set; } = string.Empty;
    public IRemoteFile? PersonalPhoto { get; set; }
    public IRemoteFile? NationalIDDocument { get; set; }
    public IRemoteFile? NationalIDBackDocument { get; set; }
    public IRemoteFile? GuardianshipDocument { get; set; }
    public string? Phone { get; set; }
    public string GuardianshipDocNumber { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
}

public class UpdateGuardianPhotoDto
{
    public IRemoteFile? PersonalPhoto { get; set; }
}

public class UpdateGuardianDto
{
    public string FullName { get; set; } = string.Empty;
    public string NationalID { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? GuardianshipDocNumber { get; set; }
    public IRemoteFile? PersonalPhoto { get; set; }
    public IRemoteFile? NationalIDDocument { get; set; }
    public IRemoteFile? NationalIDBackDocument { get; set; }
    public IRemoteFile? GuardianshipDocument { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class EndGuardianshipDto
{
    public string Reason { get; set; } = string.Empty;
}

public class AddMinorDto
{
    public int EntitledID { get; set; }
    public DateTime StartDate { get; set; }
}

public class GuardianMinorDto
{
    public int EntitledID { get; set; }
    public string EntitledPersonName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class GuardianDto
{
    public int GuardianID { get; set; }
    public int FileID { get; set; }
    public List<GuardianMinorDto> Minors { get; set; } = new();
    public string FullName { get; set; } = string.Empty;
    public string NationalID { get; set; } = string.Empty;
    public string? PersonalPhotoPath { get; set; }
    public string? Phone { get; set; }
    public string GuardianshipDocNumber { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; }
    public int PendingReviewStatus { get; set; }
}

// ─── Search DTO ─────────────────────────────────────────────
public class GuardianSearchResultDto
{
    public int GuardianID { get; set; }
    public int FileID { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string NationalID { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string GuardianshipDocNumber { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? PersonalPhotoPath { get; set; }
    public string FileNumber { get; set; } = string.Empty;
    public string? RetireeMilitaryNumber { get; set; }
}
