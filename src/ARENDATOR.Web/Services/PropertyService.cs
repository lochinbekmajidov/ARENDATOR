using ARENDATOR.Web.Data;
using ARENDATOR.Web.Models;
using ARENDATOR.Web.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ARENDATOR.Web.Services;

public class PropertyService
{
    private readonly ApplicationDbContext _dbContext;

    public PropertyService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Property>> GetFeaturedPropertiesAsync()
    {
        return await _dbContext.Properties
            .Include(p => p.Images)
            .Include(p => p.Owner)
            .Where(p => p.Featured)
            .OrderByDescending(p => p.CreatedAt)
            .Take(6)
            .ToListAsync();
    }

    public async Task<List<Property>> GetRecentPropertiesAsync(int count = 6)
    {
        return await _dbContext.Properties
            .Include(p => p.Images)
            .Include(p => p.Owner)
            .OrderByDescending(p => p.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<Property>> SearchAsync(PropertyFilterViewModel filter)
    {
        var query = _dbContext.Properties
            .Include(p => p.Images)
            .Include(p => p.Owner)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim();
            query = query.Where(p =>
                p.Title.Contains(term) ||
                p.Description.Contains(term) ||
                p.City.Contains(term) ||
                p.Address.Contains(term) ||
                (p.Brand != null && p.Brand.Contains(term)) ||
                (p.Model != null && p.Model.Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(filter.City))
        {
            query = query.Where(p => p.City.Contains(filter.City));
        }

        if (filter.Category.HasValue)
        {
            query = query.Where(p => p.Category == filter.Category.Value);
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(p => p.Status == filter.Status.Value);
        }

        if (filter.OnlyAvailable)
        {
            query = query.Where(p => p.Status == PropertyStatus.Available);
        }

        var properties = await query.ToListAsync();

        return filter.SortBy switch
        {
            "PriceAsc" => properties.OrderBy(p => p.PricePerDay).ToList(),
            "PriceDesc" => properties.OrderByDescending(p => p.PricePerDay).ToList(),
            "NameAsc" => properties.OrderBy(p => p.Title).ToList(),
            "NameDesc" => properties.OrderByDescending(p => p.Title).ToList(),
            _ => properties.OrderByDescending(p => p.CreatedAt).ToList()
        };
    }

    public async Task<List<Property>> GetPropertiesByCategoryAsync(PropertyCategory category)
    {
        var properties = await _dbContext.Properties
            .Include(p => p.Images)
            .Include(p => p.Owner)
            .Where(p => p.Category == category)
            .ToListAsync();

        return properties
            .OrderByDescending(p => p.PricePerDay)
            .ToList();
    }

    public async Task<Property?> GetByIdAsync(int id)
    {
        return await _dbContext.Properties
            .Include(p => p.Images)
            .Include(p => p.Owner)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<List<Property>> GetAvailablePropertiesAsync()
    {
        return await _dbContext.Properties
            .Include(p => p.Images)
            .Where(p => p.Status == PropertyStatus.Available)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Property>> GetRentedPropertiesAsync()
    {
        return await _dbContext.Properties
            .Include(p => p.Images)
            .Where(p => p.Status == PropertyStatus.Rented)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }
}
