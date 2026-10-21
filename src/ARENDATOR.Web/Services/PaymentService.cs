using ARENDATOR.Web.Data;
using ARENDATOR.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace ARENDATOR.Web.Services;

public class PaymentService
{
    private readonly ApplicationDbContext _dbContext;

    public PaymentService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Payment> CreateForBookingAsync(Booking booking)
    {
        var property = await _dbContext.Properties
            .FirstOrDefaultAsync(p => p.Id == booking.PropertyId)
            ?? throw new InvalidOperationException("Property not found for payment generation.");

        var payment = new Payment
        {
            BookingId = booking.Id,
            PropertyId = booking.PropertyId,
            RenterId = booking.RenterId,
            OwnerId = property.OwnerId,
            Amount = booking.TotalPrice,
            DueDate = booking.EndDate.AddDays(1),
            Status = PaymentStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            PaymentReference = $"AR-{booking.Id}-{DateTime.UtcNow:yyyyMMddHHmmss}"
        };

        _dbContext.Payments.Add(payment);
        await _dbContext.SaveChangesAsync();
        return payment;
    }

    public async Task<List<Payment>> GetAllPendingAsync()
    {
        return await _dbContext.Payments
            .Include(p => p.Property)
            .Include(p => p.Booking)
            .Include(p => p.Renter)
            .Where(p => p.Status == PaymentStatus.Pending)
            .OrderBy(p => p.DueDate)
            .ToListAsync();
    }

    public async Task<DashboardStats> GetDashboardStatsAsync()
    {
        var totalCars = await _dbContext.Properties.CountAsync(p => p.Category == PropertyCategory.Car);
        var totalApartments = await _dbContext.Properties.CountAsync(p => p.Category == PropertyCategory.Apartment);
        var available = await _dbContext.Properties.CountAsync(p => p.Status == PropertyStatus.Available);
        var occupied = await _dbContext.Properties.CountAsync(p => p.Status == PropertyStatus.Occupied || p.Status == PropertyStatus.Rented);
        var maintenance = await _dbContext.Properties.CountAsync(p => p.Status == PropertyStatus.Maintenance);
        var activeBookings = await _dbContext.Bookings.CountAsync(b => b.Status == BookingStatus.Approved || b.Status == BookingStatus.Active);
        var pendingPayments = await _dbContext.Payments.CountAsync(p => p.Status == PaymentStatus.Pending);
        var paidPayments = await _dbContext.Payments.CountAsync(p => p.Status == PaymentStatus.Paid);
        var overduePayments = await _dbContext.Payments.CountAsync(p => p.Status == PaymentStatus.Overdue);

        return new DashboardStats
        {
            TotalCars = totalCars,
            TotalApartments = totalApartments,
            AvailableProperties = available,
            OccupiedProperties = occupied,
            UnderMaintenanceProperties = maintenance,
            ActiveBookings = activeBookings,
            PendingPayments = pendingPayments,
            CompletedPayments = paidPayments,
            OverduePayments = overduePayments
        };
    }
}

public class DashboardStats
{
    public int TotalCars { get; set; }
    public int TotalApartments { get; set; }
    public int AvailableProperties { get; set; }
    public int OccupiedProperties { get; set; }
    public int UnderMaintenanceProperties { get; set; }
    public int ActiveBookings { get; set; }
    public int PendingPayments { get; set; }
    public int CompletedPayments { get; set; }
    public int OverduePayments { get; set; }
    public int RentedProperties => OccupiedProperties;
}
