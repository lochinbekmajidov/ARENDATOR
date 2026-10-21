namespace ARENDATOR.Web.Models;

public class NotificationLog
{
    public int Id { get; set; }
    public int? PaymentId { get; set; }
    public string RecipientType { get; set; } = string.Empty;
    public string RecipientPhone { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Status { get; set; } = "Queued";
    public string? ProviderResponse { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }
}
