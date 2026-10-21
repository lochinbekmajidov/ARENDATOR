using Microsoft.AspNetCore.Identity;

namespace ARENDATOR.Web.Models;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public bool IsOwner { get; set; }
    public bool IsAdmin { get; set; }

    public ICollection<Property> Properties { get; set; } = new List<Property>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<Payment> RenterPayments { get; set; } = new List<Payment>();
    public ICollection<Payment> OwnerPayments { get; set; } = new List<Payment>();
    public ICollection<NotificationLog> Notifications { get; set; } = new List<NotificationLog>();
}
