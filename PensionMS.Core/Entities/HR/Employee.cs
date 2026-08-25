using PensionMS.Core.Interfaces;
using PensionMS.Core.Entities;

namespace PensionMS.Core.Entities.HR;

public class Employee : ISoftDeletable
{
    public int EmployeeID { get; set; }

    public int DepartmentID { get; set; }
    public virtual Department Department { get; set; } = null!;

    public int? SectionID { get; set; }
    public virtual Section? Section { get; set; }

    public int JobTitleID { get; set; }
    public virtual JobTitle JobTitle { get; set; } = null!;

    public int EmploymentStatusID { get; set; }
    public virtual EmploymentStatus EmploymentStatus { get; set; } = null!;

    public string EmployeeNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string NationalID { get; set; } = string.Empty;
    public string? BirthPlace { get; set; }
    public string? MaritalStatus { get; set; }
    public string? MilitaryNumber { get; set; }
    public int? RankID { get; set; }
    public virtual Rank? Rank { get; set; }
    public DateTime? MilitaryServiceDate { get; set; }
    public string? JobPositions { get; set; }
    public string? AcademicQualifications { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Notes { get; set; }
    public string? ProfilePhotoPath { get; set; }
    public string? BloodGroup { get; set; }
    public string? NationalIDIssuePlace { get; set; }
    public DateTime? NationalIDIssueDate { get; set; }
    public DateTime? NationalIDExpiryDate { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public byte Gender { get; set; }
    public DateTime HireDate { get; set; }
    public DateTime? BirthDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserID { get; set; }

    public byte[]? RowVersion { get; set; }

    public virtual ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
    public virtual ICollection<Correspondence> Correspondences { get; set; } = new List<Correspondence>();
    public virtual ICollection<AttendanceEvent> AttendanceEvents { get; set; } = new List<AttendanceEvent>();
    public virtual ICollection<ServiceHistoryEvent> ServiceHistoryEvents { get; set; } = new List<ServiceHistoryEvent>();
    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
    public virtual ICollection<AchievementObstacle> AchievementObstacles { get; set; } = new List<AchievementObstacle>();
}
