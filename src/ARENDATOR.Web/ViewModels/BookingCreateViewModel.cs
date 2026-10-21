namespace ARENDATOR.Web.ViewModels;

public class BookingCreateViewModel
{
    public int PropertyId { get; set; }
    public string PropertyTitle { get; set; } = string.Empty;
    public decimal PricePerDay { get; set; }
    public DateTime StartDate { get; set; } = DateTime.Today.AddDays(1);
    public DateTime EndDate { get; set; } = DateTime.Today.AddDays(3);
    public int TotalDays => (EndDate - StartDate).Days > 0 ? (EndDate - StartDate).Days : 0;
    public decimal TotalPrice => TotalDays * PricePerDay;
}
