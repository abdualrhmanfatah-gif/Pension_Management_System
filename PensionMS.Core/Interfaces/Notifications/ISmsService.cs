namespace PensionMS.Core.Interfaces;

public interface ISmsService
{
    Task<bool> SendSmsAsync(string phoneNumber, string message);
}