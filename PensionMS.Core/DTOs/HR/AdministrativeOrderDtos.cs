namespace PensionMS.Core.DTOs.HR;

public class AdministrativeOrderListItemDto
{
    public int AdministrativeOrderID { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string EmployeeFullName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class AdministrativeOrderDetailsDto : AdministrativeOrderListItemDto
{
    public int EmployeeID { get; set; }
    public int AdministrativeOrderTypeID { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public string? Notes { get; set; }
    public string? ApprovedByUserName { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateAdministrativeOrderDto
{
    public int EmployeeID { get; set; }
    public int AdministrativeOrderTypeID { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class IssueAdministrativeOrderDto
{
    public int IssuedByUserID { get; set; }
}
