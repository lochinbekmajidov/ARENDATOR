using ARENDATOR.Web.Data;
using ARENDATOR.Web.Models;
using ARENDATOR.Web.Services;
using ARENDATOR.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ARENDATOR.Web.Controllers;

[Authorize]
public class BookingsController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly BookingService _bookingService;
    private readonly PaymentService _paymentService;

    public BookingsController(UserManager<ApplicationUser> userManager, ApplicationDbContext context, BookingService bookingService, PaymentService paymentService)
    {
        _userManager = userManager;
        _context = context;
        _bookingService = bookingService;
        _paymentService = paymentService;
    }

    public async Task<IActionResult> Create(int propertyId)
    {
        var property = await _context.Properties.FirstOrDefaultAsync(p => p.Id == propertyId);
        if (property == null) return NotFound();

        var vm = new BookingCreateViewModel
        {
            PropertyId = property.Id,
            PropertyTitle = property.Title,
            PricePerDay = property.PricePerDay,
            StartDate = DateTime.Today.AddDays(1),
            EndDate = DateTime.Today.AddDays(4)
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BookingCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var property = await _context.Properties.FirstOrDefaultAsync(p => p.Id == model.PropertyId);
        if (property == null) return NotFound();

        if (!await _bookingService.IsPropertyAvailableAsync(model.PropertyId, model.StartDate, model.EndDate))
        {
            ModelState.AddModelError(string.Empty, "This property is not available for the selected dates.");
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Challenge();
        }

        var booking = await _bookingService.CreateBookingAsync(property.Id, user.Id, model.StartDate, model.EndDate);
        var payment = await _paymentService.CreateForBookingAsync(booking);

        TempData["SuccessMessage"] = $"Booking request submitted successfully. Payment reference: {payment.PaymentReference}";
        return RedirectToAction(nameof(History));
    }

    public async Task<IActionResult> History()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Challenge();
        }

        var bookings = await _bookingService.GetUserBookingsAsync(user.Id);
        return View(bookings);
    }

    [Authorize(Roles = "Owner,Admin")]
    public async Task<IActionResult> OwnerQueue()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Challenge();
        }

        var bookings = await _bookingService.GetOwnerBookingsAsync(user.Id);
        return View(bookings);
    }

    [HttpPost]
    [Authorize(Roles = "Owner,Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        var booking = await _context.Bookings.Include(b => b.Property).FirstOrDefaultAsync(b => b.Id == id);
        if (booking == null) return NotFound();

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Challenge();
        }

        if (!User.IsInRole("Admin") && booking.Property.OwnerId != user.Id)
        {
            return Forbid();
        }

        await _bookingService.UpdateStatusAsync(id, BookingStatus.Approved);
        TempData["SuccessMessage"] = "Booking approved.";
        return RedirectToAction(nameof(OwnerQueue));
    }

    [HttpPost]
    [Authorize(Roles = "Owner,Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id)
    {
        var booking = await _context.Bookings.Include(b => b.Property).FirstOrDefaultAsync(b => b.Id == id);
        if (booking == null) return NotFound();

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Challenge();
        }

        if (!User.IsInRole("Admin") && booking.Property.OwnerId != user.Id)
        {
            return Forbid();
        }

        await _bookingService.UpdateStatusAsync(id, BookingStatus.Rejected);
        TempData["SuccessMessage"] = "Booking rejected.";
        return RedirectToAction(nameof(OwnerQueue));
    }
}
