namespace PensionMS.Core.Interfaces;

public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAt { get; set; }
    int? DeletedByUserID { get; set; }
}
