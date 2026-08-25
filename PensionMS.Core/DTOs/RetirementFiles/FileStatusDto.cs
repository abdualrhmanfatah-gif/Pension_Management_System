namespace PensionMS.Core.DTOs.RetirementFiles;

public class FileStatusDto
{
    public int StatusID { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Color { get; set; }
}
