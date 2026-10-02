using System;
using System.Collections.Generic;

namespace PensionMS.Core.DTOs.Correspondence;

/// <summary>
/// عنصر في قائمة المراسلات (سجل الصادر والوارد)
/// </summary>
public class CorrespondenceListItemDto
{
    public int CorrespondenceID { get; set; }
    public byte Direction { get; set; }          // 0=Outgoing, 1=Incoming
    public string TypeName { get; set; } = string.Empty;
    public int SequentialNumber { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public DateTime CorrespondenceDate { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string? PageNumber { get; set; }
    public string? ExternalPartyName { get; set; }
    public string? EmployeeName { get; set; }
    public string CreatedByUserName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// تفاصيل المراسلة الكاملة
/// </summary>
public class CorrespondenceDetailsDto : CorrespondenceListItemDto
{
    public int CorrespondenceTypeID { get; set; }
    public int? ExternalPartyID { get; set; }
    public int? EmployeeID { get; set; }
    public string? Notes { get; set; }
    public int DocumentCount { get; set; }
}

/// <summary>
/// بيانات إنشاء مراسلة جديدة
/// </summary>
public class CreateCorrespondenceDto
{
    public byte Direction { get; set; }
    public int CorrespondenceTypeID { get; set; }
    public int SequentialNumber { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public DateTime CorrespondenceDate { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string? PageNumber { get; set; }
    public int? ExternalPartyID { get; set; }
    public int? EmployeeID { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// بيانات تحديث مراسلة
/// </summary>
public class UpdateCorrespondenceDto : CreateCorrespondenceDto { }

/// <summary>
/// بيانات جهة خارجية
/// </summary>
public class ExternalPartyDto
{
    public int ExternalPartyID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PartyType { get; set; } = string.Empty;
    public string? ContactInfo { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// بيانات إنشاء جهة خارجية
/// </summary>
public class CreateExternalPartyDto
{
    public string Name { get; set; } = string.Empty;
    public string PartyType { get; set; } = string.Empty;
    public string? ContactInfo { get; set; }
}

/// <summary>
/// بيانات تحديث جهة خارجية
/// </summary>
public class UpdateExternalPartyDto : CreateExternalPartyDto { }

/// <summary>
/// بيانات نوع المراسلة (مرجع)
/// </summary>
public class CorrespondenceTypeDto
{
    public int CorrespondenceTypeID { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
