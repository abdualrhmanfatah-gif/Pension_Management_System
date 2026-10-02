using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PensionMS.Core.Interfaces;

namespace PensionMS.Core.Entities.Investment;

/// <summary>
/// أوامر العمل (الصيانة)
/// </summary>
public class WorkOrder : ISoftDeletable
{
    [Key]
    public int WorkOrderID { get; set; }

    public int RequestID { get; set; }
    [ForeignKey(nameof(RequestID))]
    public virtual MaintenanceRequest MaintenanceRequest { get; set; } = null!;

    [MaxLength(200)]
    public string? AssignedContractor { get; set; }

    public decimal EstimatedCost { get; set; }
    public decimal ActualCost { get; set; }

    public DateTime? CompletionDate { get; set; }

    public string? InvoiceFilePath { get; set; }
    public string? CompletionCertPath { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserID { get; set; }

    public byte[]? RowVersion { get; set; }
}
