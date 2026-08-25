using System;

namespace PensionMS.Core.DTOs.HR;

public class HrGlobalDocumentDto
{
    public int DocumentID { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string? FileExtension { get; set; }
    public long FileSize { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public int DocumentTypeID { get; set; }
    public string DocumentTypeName { get; set; } = string.Empty;
    
    public string? UploadedByUser { get; set; }
    
    // Employee Data Context
    public int EmployeeID { get; set; }
    public string EmployeeFullName { get; set; } = string.Empty;
    public string EmployeeNumber { get; set; } = string.Empty;
}
