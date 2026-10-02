namespace PensionMS.Core.Entities;

/// <summary>
/// إعدادات النظام
/// </summary>
public class SystemSetting
{
    public int SettingID { get; set; }

    public int? UpdatedByUserID { get; set; }
    public virtual User? UpdatedByUser { get; set; }

    public string SettingKey { get; set; } = string.Empty;
    public string SettingValue { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
