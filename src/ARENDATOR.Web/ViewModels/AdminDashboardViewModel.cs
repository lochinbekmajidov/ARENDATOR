using ARENDATOR.Web.Models;
using ARENDATOR.Web.Services;

namespace ARENDATOR.Web.ViewModels;

public class AdminDashboardViewModel
{
    public DashboardStats Stats { get; set; } = new();
    public List<Property> Properties { get; set; } = new();
    public List<AvailabilityPeriod> AvailabilityPeriods { get; set; } = new();
    public List<Booking> RecentBookings { get; set; } = new();
    public List<Payment> Payments { get; set; } = new();
}