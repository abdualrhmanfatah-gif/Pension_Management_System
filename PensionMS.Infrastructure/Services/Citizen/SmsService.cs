using Microsoft.Extensions.Logging;
using PensionMS.Core.Interfaces;

namespace PensionMS.Infrastructure.Services.Citizen;

public class SmsService : ISmsService
{
    private readonly ILogger<SmsService> _logger;

    public SmsService(ILogger<SmsService> logger)
    {
        _logger = logger;
    }

    public Task<bool> SendSmsAsync(string phoneNumber, string message)
    {
        // Mock implementation for SMS sending. In production, this would call an external SMS Gateway.
        _logger.LogInformation("Simulating sending SMS to {PhoneNumber}: {Message}", phoneNumber, message);
        return Task.FromResult(true);
    }
}