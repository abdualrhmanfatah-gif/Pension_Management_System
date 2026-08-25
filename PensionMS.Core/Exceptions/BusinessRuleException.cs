namespace PensionMS.Core.Exceptions;

/// <summary>
/// استثناء قواعد الأعمال — يُربط بـ 400 Bad Request
/// </summary>
public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message) { }
    public BusinessRuleException(string message, Exception innerException) : base(message, innerException) { }
}
