namespace PensionMS.Core.DTOs.Common;

/// <summary>
/// عنصر بيانات مرجعية (Lookup)
/// </summary>
public class LookupDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}
