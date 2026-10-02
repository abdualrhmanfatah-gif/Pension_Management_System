namespace PensionMS.Core.Exceptions;

/// <summary>
/// استثناء تعارض التزامن — يُربط بـ 409 Conflict
/// </summary>
public class ConcurrencyConflictException : Exception
{
    public ConcurrencyConflictException()
        : base("تعارض في البيانات — تم تعديل السجل بواسطة مستخدم آخر") { }

    public ConcurrencyConflictException(string message) : base(message) { }

    public ConcurrencyConflictException(string message, Exception innerException)
        : base(message, innerException) { }
}
