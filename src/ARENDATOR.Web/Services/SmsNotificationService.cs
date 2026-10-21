using ARENDATOR.Web.Data;
using ARENDATOR.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace ARENDATOR.Web.Services;

public interface ISmsNotificationService
{
    Task<SmsDeliveryResult> SendReminderAsync(string phoneNumber, string message, string recipientType, string? userId = null, int? paymentId = null);
}

public class SmsDeliveryResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string ProviderResponse { get; set; } = string.Empty;
}

public class SmsNotificationService : ISmsNotificationService
{
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<SmsNotificationService> _logger;

    public SmsNotificationService(IConfiguration configuration, ApplicationDbContext dbContext, ILogger<SmsNotificationService> logger)
    {
        _configuration = configuration;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<SmsDeliveryResult> SendReminderAsync(string phoneNumber, string message, string recipientType, string? userId = null, int? paymentId = null)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return new SmsDeliveryResult { Success = false, Message = "Phone number is missing." };
        }

        var mode = _configuration["SmsSettings:Mode"] ?? "Development";
        var log = new NotificationLog
        {
            RecipientType = recipientType,
            RecipientPhone = phoneNumber,
            Message = message,
            Status = "Queued",
            UserId = userId ?? string.Empty,
            PaymentId = paymentId,
            CreatedAt = DateTime.UtcNow
        };

        if (string.Equals(mode, "Development", StringComparison.OrdinalIgnoreCase))
        {
            log.Status = "Delivered (mock)";
            log.ProviderResponse = "Development mode: SMS not sent to real provider.";
            _dbContext.NotificationLogs.Add(log);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("SMS reminder simulated for {PhoneNumber}: {Message}", phoneNumber, message);
            return new SmsDeliveryResult { Success = true, Message = "Development mode simulated successfully.", ProviderResponse = log.ProviderResponse };
        }

        var apiKey = _configuration["SmsSettings:ApiKey"];
        var baseUrl = _configuration["SmsSettings:BaseUrl"];
        if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(apiKey))
        {
            log.Status = "Failed";
            log.ProviderResponse = "Missing SMS provider configuration.";
            _dbContext.NotificationLogs.Add(log);
            await _dbContext.SaveChangesAsync();
            return new SmsDeliveryResult { Success = false, Message = "Missing provider config.", ProviderResponse = log.ProviderResponse };
        }

        try
        {
            using var client = new HttpClient();
            var payload = new
            {
                to = phoneNumber,
                message,
                apiKey
            };

            var response = await client.PostAsJsonAsync(baseUrl, payload);
            var body = await response.Content.ReadAsStringAsync();
            log.Status = response.IsSuccessStatusCode ? "Delivered" : "Failed";
            log.ProviderResponse = body;
            _dbContext.NotificationLogs.Add(log);
            await _dbContext.SaveChangesAsync();

            return new SmsDeliveryResult
            {
                Success = response.IsSuccessStatusCode,
                Message = response.IsSuccessStatusCode ? "SMS sent successfully." : "SMS provider returned an error.",
                ProviderResponse = body
            };
        }
        catch (Exception ex)
        {
            log.Status = "Failed";
            log.ProviderResponse = ex.Message;
            _dbContext.NotificationLogs.Add(log);
            await _dbContext.SaveChangesAsync();
            _logger.LogError(ex, "SMS reminder failed for {PhoneNumber}", phoneNumber);
            return new SmsDeliveryResult { Success = false, Message = ex.Message, ProviderResponse = ex.Message };
        }
    }
}
