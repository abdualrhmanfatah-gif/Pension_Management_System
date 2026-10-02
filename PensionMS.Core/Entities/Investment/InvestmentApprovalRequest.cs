using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PensionMS.Core.Interfaces;
using PensionMS.Core.Entities;

namespace PensionMS.Core.Entities.Investment;

/// <summary>
/// طلبات الاعتماد الاستثماري
/// </summary>
public class InvestmentApprovalRequest : ISoftDeletable
{
    [Key]
    public int ApprovalID { get; set; }

    [Required]
    [MaxLength(100)]
    public string EntityType { get; set; } = string.Empty; // Project, Property, Contract, Expense

    public int EntityID { get; set; }

    public int RequestedByUserID { get; set; }
    [ForeignKey(nameof(RequestedByUserID))]
    public virtual User RequestedBy { get; set; } = null!;

    [MaxLength(100)]
    public string? WorkflowStage { get; set; }

    public string? DecisionHistory { get; set; } // JSON list of decisions

    [MaxLength(50)]
    public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected, MoreInfo

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserID { get; set; }

    public byte[]? RowVersion { get; set; }
}
