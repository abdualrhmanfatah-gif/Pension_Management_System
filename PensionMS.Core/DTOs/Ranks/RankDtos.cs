using System.ComponentModel.DataAnnotations;

namespace PensionMS.Core.DTOs.Ranks;

public class RankDto
{
    public int RankID { get; set; }
    public string RankName { get; set; } = string.Empty;
    public int RankLevel { get; set; }
    public decimal BaseSalary { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateRankDto
{
    [Required(ErrorMessage = "اسم الرتبة مطلوب")]
    public string RankName { get; set; } = string.Empty;

    [Required(ErrorMessage = "المستوى مطلوب")]
    public int RankLevel { get; set; }

    [Required(ErrorMessage = "المعاش الأساسي مطلوب")]
    public decimal BaseSalary { get; set; }
}

public class UpdateRankDto
{
    public string? RankName { get; set; }
    public int? RankLevel { get; set; }
    public decimal? BaseSalary { get; set; }
}
