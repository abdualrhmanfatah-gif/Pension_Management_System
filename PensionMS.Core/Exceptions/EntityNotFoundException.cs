namespace PensionMS.Core.Exceptions;

/// <summary>
/// استثناء كيان غير موجود — يُربط بـ 404 Not Found
/// </summary>
public class EntityNotFoundException : Exception
{
    public string EntityName { get; }
    public object? EntityId { get; }

    public EntityNotFoundException(string entityName, object? entityId = null)
        : base($"{entityName} غير موجود")
    {
        EntityName = entityName;
        EntityId = entityId;
    }

    public EntityNotFoundException(string entityName, object? entityId, Exception innerException)
        : base($"{entityName} غير موجود", innerException)
    {
        EntityName = entityName;
        EntityId = entityId;
    }
}
