namespace PensionMS.Core.DTOs.Admin;

public class UpdateSettingDto
{
    public string Value { get; set; } = string.Empty;
}

public class CreateSettingDto
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
}
