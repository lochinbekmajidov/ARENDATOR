namespace ARENDATOR.Web.Models;

public class AvailabilityPeriod
{
    public int Id { get; set; }
    public int PropertyId { get; set; }
    public Property Property { get; set; } = default!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsOccupied { get; set; } = true;
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
