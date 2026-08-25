using PensionMS.Core.Interfaces;

namespace PensionMS.Core.DTOs.Agents;

public class CreateAgentDto
{
    public int FileID { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string NationalID { get; set; } = string.Empty;
    public IRemoteFile? PersonalPhoto { get; set; }
    public IRemoteFile? NationalIDDocument { get; set; }
    public IRemoteFile? NationalIDBackDocument { get; set; }
    public string? Phone { get; set; }
    public string? AgencyNumber { get; set; }
    public DateTime? AgencyDate { get; set; }
    public DateTime? AgencyEndDate { get; set; }
    public IRemoteFile? AgencyDocument { get; set; }
    public string? Notes { get; set; }
}

public class UpdateAgentDto
{
    public string FullName { get; set; } = string.Empty;
    public string NationalID { get; set; } = string.Empty;
    public IRemoteFile? PersonalPhoto { get; set; }
    public IRemoteFile? NationalIDDocument { get; set; }
    public IRemoteFile? NationalIDBackDocument { get; set; }
    public string? Phone { get; set; }
    public string? AgencyNumber { get; set; }
    public DateTime? AgencyDate { get; set; }
    public DateTime? AgencyEndDate { get; set; }
    public IRemoteFile? AgencyDocument { get; set; }
    public string? Notes { get; set; }

    /// <summary>
    /// آخر تاريخ تعديل معروف للعميل — للتحقق من عدم التعارض
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}

public class AgentDto
{
    public int AgentID { get; set; }
    public int FileID { get; set; }
    public string FileNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string NationalID { get; set; } = string.Empty;
    public string? PersonalPhotoPath { get; set; }
    public string? Phone { get; set; }
    public string? AgencyNumber { get; set; }
    public DateTime? AgencyDate { get; set; }
    public DateTime? AgencyEndDate { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int PendingReviewStatus { get; set; }
}

public class AgentSearchResultDto
{
    public int AgentID { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string NationalID { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? AgencyNumber { get; set; }
    public DateTime? AgencyDate { get; set; }
    public DateTime? AgencyEndDate { get; set; }
    public string? Notes { get; set; }
    public string? PersonalPhotoPath { get; set; }
    public string? NationalIDDocumentPath { get; set; }
    public string? AgencyDocumentPath { get; set; }
    public int FileID { get; set; }
    public string FileNumber { get; set; } = string.Empty;
    public string? RetireeMilitaryNumber { get; set; }
}
