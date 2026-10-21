using ARENDATOR.Web.Data;
using ARENDATOR.Web.Models;
using ARENDATOR.Web.Services;
using ARENDATOR.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ARENDATOR.Web.Controllers;

public class PropertiesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly PropertyService _propertyService;
    private readonly UserManager<ApplicationUser> _userManager;

    public PropertiesController(ApplicationDbContext context, PropertyService propertyService, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _propertyService = propertyService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(PropertyFilterViewModel filter)
    {
        var properties = await _propertyService.SearchAsync(filter);
        ViewBag.Filter = filter;
        return View(properties);
    }

    public async Task<IActionResult> Available()
    {
        var properties = await _propertyService.GetAvailablePropertiesAsync();
        ViewBag.Filter = new PropertyFilterViewModel { OnlyAvailable = true };
        return View("Index", properties);
    }

    public async Task<IActionResult> Rented()
    {
        var properties = await _propertyService.GetRentedPropertiesAsync();
        ViewBag.Filter = new PropertyFilterViewModel { Status = PropertyStatus.Rented, OnlyAvailable = false };
        return View("Index", properties);
    }

    public async Task<IActionResult> Maintenance()
    {
        var properties = await _propertyService.SearchAsync(new PropertyFilterViewModel
        {
            Status = PropertyStatus.Maintenance,
            OnlyAvailable = false
        });
        ViewBag.Filter = new PropertyFilterViewModel { Status = PropertyStatus.Maintenance, OnlyAvailable = false };
        return View("Index", properties);
    }

    public async Task<IActionResult> Apartments(string searchTerm, string city, string status, string sortBy = "PriceDesc")
    {
        var filter = new PropertyFilterViewModel
        {
            SearchTerm = searchTerm ?? string.Empty,
            City = city ?? string.Empty,
            Category = PropertyCategory.Apartment,
            OnlyAvailable = false,
            SortBy = sortBy
        };

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<PropertyStatus>(status, true, out var parsedStatus))
        {
            filter.Status = parsedStatus;
        }

        var properties = await _propertyService.SearchAsync(filter);
        ViewBag.Filter = filter;
        return View(properties);
    }

    public async Task<IActionResult> Cars(string searchTerm, string city, string status, string sortBy = "PriceDesc")
    {
        var filter = new PropertyFilterViewModel
        {
            SearchTerm = searchTerm ?? string.Empty,
            City = city ?? string.Empty,
            Category = PropertyCategory.Car,
            OnlyAvailable = false,
            SortBy = sortBy
        };

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<PropertyStatus>(status, true, out var parsedStatus))
        {
            filter.Status = parsedStatus;
        }

        var properties = await _propertyService.SearchAsync(filter);
        ViewBag.Filter = filter;
        return View(properties);
    }

    public async Task<IActionResult> Details(int id)
    {
        var property = await _propertyService.GetByIdAsync(id);
        if (property == null)
        {
            return NotFound();
        }

        return View(property);
    }

    [Authorize(Roles = "Owner,Admin")]
    public IActionResult Create(PropertyCategory? category = null)
    {
        var model = new Property
        {
            Category = category ?? PropertyCategory.Apartment,
            Status = PropertyStatus.Available,
            Country = "Tajikistan",
            ContactName = User.Identity?.Name ?? string.Empty
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Owner,Admin")]
    public async Task<IActionResult> Create(Property propertyInput)
    {
        if (!ModelState.IsValid)
        {
            return View(propertyInput);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Challenge();
        }

        propertyInput.OwnerId = user.Id;
        propertyInput.Slug = GenerateSlug(propertyInput.Title);
        propertyInput.CreatedAt = DateTime.UtcNow;
        propertyInput.ContactName = string.IsNullOrWhiteSpace(propertyInput.ContactName) ? user.UserName ?? user.Email ?? "Owner" : propertyInput.ContactName;
        propertyInput.ContactPhone = string.IsNullOrWhiteSpace(propertyInput.ContactPhone) ? user.PhoneNumber ?? string.Empty : propertyInput.ContactPhone;
        propertyInput.ContactEmail = string.IsNullOrWhiteSpace(propertyInput.ContactEmail) ? user.Email ?? string.Empty : propertyInput.ContactEmail;

        if (propertyInput.Category == PropertyCategory.Car && string.IsNullOrWhiteSpace(propertyInput.Brand))
        {
            ModelState.AddModelError(nameof(propertyInput.Brand), "Brand is required for cars.");
            return View(propertyInput);
        }

        if (propertyInput.Category == PropertyCategory.Apartment && !propertyInput.Bedrooms.HasValue)
        {
            ModelState.AddModelError(nameof(propertyInput.Bedrooms), "Number of bedrooms is required for apartments.");
            return View(propertyInput);
        }

        var parsedUrls = ParseImageUrls(propertyInput.ImageUrls, propertyInput.CoverImageUrl);
        if (parsedUrls.Count > 0)
        {
            propertyInput.CoverImageUrl ??= parsedUrls[0];
            propertyInput.Images = parsedUrls
                .Select((url, index) => new PropertyImage
                {
                    ImageUrl = url,
                    IsPrimary = index == 0,
                    Caption = index == 0 ? "Main photo" : $"Photo {index + 1}"
                })
                .ToList();
        }

        if (string.IsNullOrWhiteSpace(propertyInput.CoverImageUrl) && propertyInput.Images.Any())
        {
            propertyInput.CoverImageUrl = propertyInput.Images.First().ImageUrl;
        }

        _context.Properties.Add(propertyInput);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Property created successfully.";
        return RedirectToAction(nameof(Details), new { id = propertyInput.Id });
    }

    [Authorize(Roles = "Owner,Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var property = await _context.Properties
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (property == null) return NotFound();

        var user = await _userManager.GetUserAsync(User);
        if (!User.IsInRole("Admin") && property.OwnerId != user!.Id)
        {
            return Forbid();
        }

        return View(property);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Owner,Admin")]
    public async Task<IActionResult> Edit(int id, Property propertyInput)
    {
        var property = await _context.Properties
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (property == null) return NotFound();

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Challenge();
        }

        if (!User.IsInRole("Admin") && property.OwnerId != user.Id)
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            return View(propertyInput);
        }

        property.Title = propertyInput.Title;
        property.Description = propertyInput.Description;
        property.Category = propertyInput.Category;
        property.Status = propertyInput.Status;
        property.PricePerDay = propertyInput.PricePerDay;
        property.City = propertyInput.City;
        property.Address = propertyInput.Address;
        property.Country = propertyInput.Country;
        property.Brand = propertyInput.Brand;
        property.Model = propertyInput.Model;
        property.LicensePlate = propertyInput.LicensePlate;
        property.Year = propertyInput.Year;
        property.Transmission = propertyInput.Transmission;
        property.FuelType = propertyInput.FuelType;
        property.Mileage = propertyInput.Mileage;
        property.BodyType = propertyInput.BodyType;
        property.Bedrooms = propertyInput.Bedrooms;
        property.Bathrooms = propertyInput.Bathrooms;
        property.MaxGuests = propertyInput.MaxGuests;
        property.AreaSqMeters = propertyInput.AreaSqMeters;
        property.Seats = propertyInput.Seats;
        property.ApartmentType = propertyInput.ApartmentType;
        property.HasParking = propertyInput.HasParking;
        property.HasPool = propertyInput.HasPool;
        property.Featured = propertyInput.Featured;
        property.ContactName = propertyInput.ContactName;
        property.ContactPhone = propertyInput.ContactPhone;
        property.ContactEmail = propertyInput.ContactEmail;
        property.Slug = GenerateSlug(propertyInput.Title);

        var parsedUrls = ParseImageUrls(propertyInput.ImageUrls, propertyInput.CoverImageUrl);
        if (parsedUrls.Count > 0)
        {
            property.CoverImageUrl = parsedUrls[0];

            _context.PropertyImages.RemoveRange(property.Images);
            property.Images = parsedUrls
                .Select((url, index) => new PropertyImage
                {
                    PropertyId = property.Id,
                    ImageUrl = url,
                    IsPrimary = index == 0,
                    Caption = index == 0 ? "Main photo" : $"Photo {index + 1}"
                })
                .ToList();
        }
        else if (!string.IsNullOrWhiteSpace(propertyInput.CoverImageUrl))
        {
            property.CoverImageUrl = propertyInput.CoverImageUrl;
        }

        await _context.SaveChangesAsync();
        TempData["Success"] = "Property updated successfully.";
        return RedirectToAction(nameof(Details), new { id = property.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Owner,Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var property = await _context.Properties.FirstOrDefaultAsync(p => p.Id == id);
        if (property == null) return NotFound();

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Challenge();
        }

        if (!User.IsInRole("Admin") && property.OwnerId != user.Id)
        {
            return Forbid();
        }

        _context.Properties.Remove(property);
        await _context.SaveChangesAsync();
        TempData["Success"] = property.Category == PropertyCategory.Apartment ? "Apartment deleted successfully." : "Car deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    private static List<string> ParseImageUrls(string? imageUrls, string? coverImageUrl)
    {
        var allUrls = new List<string>();

        if (!string.IsNullOrWhiteSpace(coverImageUrl))
        {
            allUrls.Add(coverImageUrl);
        }

        if (!string.IsNullOrWhiteSpace(imageUrls))
        {
            allUrls.AddRange(imageUrls
                .Split(new[] { ',', ';', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(url => !string.IsNullOrWhiteSpace(url))
                .Distinct());
        }

        return allUrls
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string GenerateSlug(string title)
    {
        var slug = title.Trim();
        slug = slug.Replace(" ", "-");
        slug = slug.ToLowerInvariant();
        return slug;
    }
}
