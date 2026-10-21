namespace ARENDATOR.Web.Models;

public enum BookingStatus
{
    Pending,
    Approved,
    Rejected,
    Active,
    Completed,
    Cancelled
}

public class Booking
{
    public int Id { get; set; }
    public int PropertyId { get; set; }
    public Property Property { get; set; } = default!;

    public string RenterId { get; set; } = string.Empty;
    public ApplicationUser Renter { get; set; } = default!;

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalPrice { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ApprovedAt { get; set; }
    public DateTime? SmsReminderSentAt { get; set; }
    public bool SmsReminderSent { get; set; }
    public string? Notes { get; set; }

    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
