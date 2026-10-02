using PensionMS.Core.DTOs.Documents;

namespace PensionMS.Core.Exceptions;

/// <summary>
/// تكرار رفع وثيقة (بصمة محتوى + نطاق مالك) — تحمل الوثيقة المطابقة لإرجاعها في استجابة 409
/// </summary>
public class DuplicateDocumentException : Exception
{
    public DocumentDto Document { get; }

    public DuplicateDocumentException(DocumentDto document)
        : base("DUPLICATE_DOCUMENT")
    {
        Document = document;
    }
}
