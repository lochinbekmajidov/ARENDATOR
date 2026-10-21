using ARENDATOR.Web.Data;
using ARENDATOR.Web.Models;
using ARENDATOR.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace ARENDATOR.Web.BackgroundJobs;

public class PaymentReminderBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PaymentReminderBackgroundService> _logger;

    public PaymentReminderBackgroundService(IServiceScopeFactory scopeFactory, ILogger<PaymentReminderBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var smsService = scope.ServiceProvider.GetRequiredService<ISmsNotificationService>();

                var reminderDate = DateTime.UtcNow.Date.AddDays(1);

                var bookingsDueTomorrow = await dbContext.Bookings
                    .Include(b => b.Property)
                        .ThenInclude(p => p.Owner)
                    .Include(b => b.Renter)
                    .Where(b => b.Status == BookingStatus.Approved &&
                                b.StartDate.Date == reminderDate &&
                                !b.SmsReminderSent)
                    .ToListAsync(stoppingToken);

                foreach (var booking in bookingsDueTomorrow)
                {
                    var message = $"ARENDATOR reminder: your rental of {booking.Property.Title} begins tomorrow on {booking.StartDate:dd.MM.yyyy}. We look forward to welcoming you.";
                    var ownerMessage = $"ARENDATOR reminder: your guest for {booking.Property.Title} is scheduled to arrive tomorrow on {booking.StartDate:dd.MM.yyyy}.";

                    var renterResult = await smsService.SendReminderAsync(booking.Renter.PhoneNumber ?? string.Empty, message, "BookingRenter", booking.RenterId);
                    var ownerResult = await smsService.SendReminderAsync(booking.Property.Owner.PhoneNumber ?? string.Empty, ownerMessage, "BookingOwner", booking.Property.OwnerId);

                    booking.SmsReminderSent = renterResult.Success || ownerResult.Success;
                    booking.SmsReminderSentAt = DateTime.UtcNow;

                    _logger.LogInformation("Booking reminder for {Property}: {RenterResult}; {OwnerResult}", booking.Property.Title, renterResult.Message, ownerResult.Message);
                }

                var paymentsDueTomorrow = await dbContext.Payments
                    .Include(p => p.Booking)
                    .Include(p => p.Property)
                    .Include(p => p.Renter)
                    .Include(p => p.Owner)
                    .Where(p => p.Status == PaymentStatus.Pending &&
                                p.DueDate.Date == reminderDate &&
                                !p.SmsReminderSent)
                    .ToListAsync(stoppingToken);

                foreach (var payment in paymentsDueTomorrow)
                {
                    var renterMessage = $"ARENDATOR Reminder: Your rental payment for {payment.Property.Title} is due tomorrow. Please complete your payment on time.";
                    var ownerMessage = $"ARENDATOR Reminder: The rental payment for your {payment.Property.Title} is due tomorrow. Please contact the renter if necessary.";

                    var renterResult = await smsService.SendReminderAsync(payment.Renter.PhoneNumber ?? string.Empty, renterMessage, "Renter", payment.RenterId, payment.Id);
                    var ownerResult = await smsService.SendReminderAsync(payment.Owner.PhoneNumber ?? string.Empty, ownerMessage, "Owner", payment.OwnerId, payment.Id);

                    payment.SmsReminderSent = renterResult.Success || ownerResult.Success;
                    payment.LastSmsReminderAt = DateTime.UtcNow;
                    _logger.LogInformation("Payment reminder for {Property}: {RenterResult}; {OwnerResult}", payment.Property.Title, renterResult.Message, ownerResult.Message);
                }

                await dbContext.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Payment reminder background job encountered an error.");
            }

            await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
        }
    }
}
