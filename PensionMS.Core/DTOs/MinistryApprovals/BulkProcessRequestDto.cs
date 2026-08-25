using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PensionMS.Core.DTOs.MinistryApprovals;

public class BulkProcessRequestDto
{
    [Required]
    [MinLength(1, ErrorMessage = "يجب تحديد قيد واحد على الأقل للمعالجة")]
    public List<int> ApprovalIds { get; set; } = new List<int>();
}
