using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ARENDATOR.Web.Models;

public enum PropertyCategory
{
    Car,
    Apartment
}

public enum PropertyStatus
{
    Available = 0,
    Occupied = 1,
    Rented = 1,
    Maintenance = 2
}

public class Property
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public PropertyCategory Category { get; set; }
    public PropertyStatus Status { get; set; } = PropertyStatus.Available;
    public decimal PricePerDay { get; set; }
    public string Currency { get; set; } = "USD";
    public string City { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Country { get; set; } = "USA";
    public string? CoverImageUrl { get; set; }
    public string? LicensePlate { get; set; }
    public bool Featured { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Display(Name = "Contact name")]
    public string ContactName { get; set; } = string.Empty;

    [Display(Name = "Contact phone")]
    public string ContactPhone { get; set; } = string.Empty;

    [Display(Name = "Contact email")]
    public string ContactEmail { get; set; } = string.Empty;

    [NotMapped]
    public string? ImageUrls { get; set; }

    public string OwnerId { get; set; } = string.Empty;
    public ApplicationUser Owner { get; set; } = default!;

    public string? Brand { get; set; }
    public string? Model { get; set; }
    public int? Year { get; set; }
    public string? Transmission { get; set; }
    public string? FuelType { get; set; }
    public int? Mileage { get; set; }
    public string? BodyType { get; set; }

    public int? Bedrooms { get; set; }
    public int? Bathrooms { get; set; }
    public int? MaxGuests { get; set; }
    public int? AreaSqMeters { get; set; }
    public int? Seats { get; set; }
    public string? ApartmentType { get; set; }
    public bool? HasParking { get; set; }
    public bool? HasPool { get; set; }

    public ICollection<PropertyImage> Images { get; set; } = new List<PropertyImage>();
    public ICollection<AvailabilityPeriod> AvailabilityPeriods { get; set; } = new List<AvailabilityPeriod>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
