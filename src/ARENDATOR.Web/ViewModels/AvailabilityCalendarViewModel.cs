using ARENDATOR.Web.Models;

namespace ARENDATOR.Web.ViewModels;

public class AvailabilityCalendarViewModel
{
    public DateTime ReferenceDate { get; set; } = DateTime.UtcNow;
    public List<Property> Properties { get; set; } = new();
    public List<AvailabilityPeriod> AvailabilityPeriods { get; set; } = new();
}
