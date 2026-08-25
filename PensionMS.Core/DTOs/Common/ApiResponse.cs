namespace PensionMS.Core.DTOs.Common;

/// <summary>
/// استجابة API موحدة
/// </summary>
public class ApiResponse<T>
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }

    public static ApiResponse<T> Success(T? data, string? message = null)
        => new() { IsSuccess = true, Data = data, Message = message };

    public static ApiResponse<T> Fail(string message)
        => new() { IsSuccess = false, Message = message };

    public static ApiResponse<T> Fail(string message, List<string> errors)
        => new() { IsSuccess = false, Message = message, Errors = errors };
}
