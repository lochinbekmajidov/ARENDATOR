using ARENDATOR.Web.Data;
using ARENDATOR.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace ARENDATOR.Web.Services;

public class BookingService
{
    private readonly ApplicationDbContext _dbContext;

    public BookingService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> IsPropertyAvailableAsync(int propertyId, DateTime startDate, DateTime endDate, int? excludeBookingId = null)
    {
        if (endDate <= startDate)
        {
            return false;
        }

        var overlap = await _dbContext.Bookings
            .Where(b => b.PropertyId == propertyId &&
                        b.Status != BookingStatus.Rejected &&
                        b.Status != BookingStatus.Cancelled &&
                        b.Id != (excludeBookingId ?? -1) &&
                        b.StartDate < endDate &&
                        b.EndDate > startDate)
            .AnyAsync();

        return !overlap;
    }

    public async Task<Booking> CreateBookingAsync(int propertyId, string renterId, DateTime startDate, DateTime endDate, string? notes = null)
    {
        var property = await _dbContext.Properties
            .FirstOrDefaultAsync(p => p.Id == propertyId)
            ?? throw new InvalidOperationException("Property not found.");

        if (!await IsPropertyAvailableAsync(propertyId, startDate, endDate))
        {
            throw new InvalidOperationException("The selected dates overlap with an existing booking.");
        }

        var totalDays = Math.Max(1, (endDate - startDate).Days);
        var booking = new Booking
        {
            PropertyId = propertyId,
            RenterId = renterId,
            StartDate = startDate,
            EndDate = endDate,
            TotalPrice = property.PricePerDay * totalDays,
            Status = BookingStatus.Pending,
            Notes = notes,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Bookings.Add(booking);
        await _dbContext.SaveChangesAsync();

        return booking;
    }

    public async Task<List<Booking>> GetUserBookingsAsync(string renterId)
    {
        return await _dbContext.Bookings
            .Include(b => b.Property)
            .Where(b => b.RenterId == renterId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Booking>> GetOwnerBookingsAsync(string ownerId)
    {
        return await _dbContext.Bookings
            .Include(b => b.Property)
            .Include(b => b.Renter)
            .Where(b => b.Property.OwnerId == ownerId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    public async Task<Booking?> GetByIdAsync(int id)
    {
        return await _dbContext.Bookings
            .Include(b => b.Property)
            .Include(b => b.Renter)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<Booking> UpdateStatusAsync(int bookingId, BookingStatus status)
    {
        var booking = await _dbContext.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId)
            ?? throw new InvalidOperationException("Booking not found.");

        booking.Status = status;
        booking.ApprovedAt = status == BookingStatus.Approved ? DateTime.UtcNow : booking.ApprovedAt;

        if (status == BookingStatus.Approved)
        {
            var property = await _dbContext.Properties.FirstOrDefaultAsync(p => p.Id == booking.PropertyId);
            if (property != null)
            {
                property.Status = PropertyStatus.Rented;
            }
        }

        if (status == BookingStatus.Completed || status == BookingStatus.Cancelled || status == BookingStatus.Rejected)
        {
            var property = await _dbContext.Properties.FirstOrDefaultAsync(p => p.Id == booking.PropertyId);
            if (property != null && property.Status == PropertyStatus.Rented)
            {
                property.Status = PropertyStatus.Available;
            }
        }

        await _dbContext.SaveChangesAsync();
        return booking;
    }
}
