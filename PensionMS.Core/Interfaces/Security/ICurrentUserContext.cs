namespace PensionMS.Core.Interfaces;

/// <summary>هوية المستخدم المصادق عليه؛ لا يقرر نطاق البيانات أو الصلاحيات.</summary>
public interface ICurrentUserContext
{
    int GetCurrentUserId();
}
