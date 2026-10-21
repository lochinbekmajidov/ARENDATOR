namespace ARENDATOR.Web.Models;

public enum PaymentStatus
{
    Pending,
    Paid,
    Overdue,
    Failed
}

public class Payment
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public Booking Booking { get; set; } = default!;

    public int PropertyId { get; set; }
    public Property Property { get; set; } = default!;

    public string RenterId { get; set; } = string.Empty;
    public ApplicationUser Renter { get; set; } = default!;

    public string OwnerId { get; set; } = string.Empty;
    public ApplicationUser Owner { get; set; } = default!;

    public decimal Amount { get; set; }
    public DateTime DueDate { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PaidAt { get; set; }
    public string? PaymentReference { get; set; }
    public bool SmsReminderSent { get; set; }
    public DateTime? LastSmsReminderAt { get; set; }
}
