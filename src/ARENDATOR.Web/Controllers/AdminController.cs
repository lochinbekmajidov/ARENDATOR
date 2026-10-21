using ARENDATOR.Web.Data;
using ARENDATOR.Web.Models;
using ARENDATOR.Web.Services;
using ARENDATOR.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ARENDATOR.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly PaymentService _paymentService;

    public AdminController(ApplicationDbContext context, PaymentService paymentService)
    {
        _context = context;
        _paymentService = paymentService;
    }

    public async Task<IActionResult> Index()
    {
        var stats = await _paymentService.GetDashboardStatsAsync();
        var properties = await _context.Properties
            .Include(p => p.Images)
            .OrderByDescending(p => p.CreatedAt)
            .Take(10)
            .ToListAsync();

        var availabilityPeriods = await _context.AvailabilityPeriods
            .Include(ap => ap.Property)
            .OrderBy(ap => ap.StartDate)
            .Take(10)
            .ToListAsync();

        var recentBookings = await _context.Bookings
            .Include(b => b.Property)
            .Include(b => b.Renter)
            .OrderByDescending(b => b.CreatedAt)
            .Take(8)
            .ToListAsync();

        var payments = await _context.Payments
            .Include(p => p.Property)
            .Include(p => p.Renter)
            .OrderByDescending(p => p.CreatedAt)
            .Take(8)
            .ToListAsync();

        var model = new AdminDashboardViewModel
        {
            Stats = stats,
            Properties = properties,
            AvailabilityPeriods = availabilityPeriods,
            RecentBookings = recentBookings,
            Payments = payments
        };

        return View(model);
    }

    public async Task<IActionResult> Calendar(DateTime? month)
    {
        var reference = (month ?? DateTime.UtcNow).Date;
        var firstDay = new DateTime(reference.Year, reference.Month, 1);

        var properties = await _context.Properties
            .Include(p => p.Images)
            .Include(p => p.AvailabilityPeriods)
            .OrderBy(p => p.Category)
            .ThenBy(p => p.Title)
            .ToListAsync();

        var periods = await _context.AvailabilityPeriods
            .Include(ap => ap.Property)
            .Where(ap => ap.StartDate <= firstDay.AddMonths(1).AddDays(-1) && ap.EndDate >= firstDay)
            .OrderBy(ap => ap.StartDate)
            .ToListAsync();

        var model = new AvailabilityCalendarViewModel
        {
            ReferenceDate = reference,
            Properties = properties,
            AvailabilityPeriods = periods
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddAvailabilityPeriod(int propertyId, DateTime startDate, DateTime endDate, string notes = "")
    {
        if (endDate < startDate)
        {
            TempData["Error"] = "The end date must be after the start date.";
            return RedirectToAction(nameof(Calendar));
        }

        var property = await _context.Properties.FirstOrDefaultAsync(p => p.Id == propertyId);
        if (property == null)
        {
            return NotFound();
        }

        var overlaps = await _context.AvailabilityPeriods.AnyAsync(ap =>
            ap.PropertyId == propertyId &&
            ap.StartDate <= endDate &&
            ap.EndDate >= startDate);

        if (overlaps)
        {
            TempData["Error"] = "This property already has an overlapping availability period.";
            return RedirectToAction(nameof(Calendar));
        }

        _context.AvailabilityPeriods.Add(new AvailabilityPeriod
        {
            PropertyId = propertyId,
            StartDate = startDate.Date,
            EndDate = endDate.Date,
            IsOccupied = true,
            Notes = notes,
            CreatedAt = DateTime.UtcNow
        });

        property.Status = property.Category == PropertyCategory.Car ? PropertyStatus.Occupied : PropertyStatus.Occupied;
        await _context.SaveChangesAsync();

        TempData["Success"] = "Availability period saved successfully.";
        return RedirectToAction(nameof(Calendar));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdatePropertyStatus(int id, PropertyStatus status)
    {
        var property = await _context.Properties.FirstOrDefaultAsync(p => p.Id == id);
        if (property == null)
        {
            return NotFound();
        }

        property.Status = status;
        await _context.SaveChangesAsync();

        TempData["Success"] = $"{property.Title} status updated to {status}.";
        return RedirectToAction(nameof(Index));
    }
}
