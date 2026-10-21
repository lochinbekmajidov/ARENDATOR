using ARENDATOR.Web.Models;

namespace ARENDATOR.Web.ViewModels;

public class PropertyFilterViewModel
{
    public string SearchTerm { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public PropertyCategory? Category { get; set; }
    public PropertyStatus? Status { get; set; }
    public bool OnlyAvailable { get; set; } = true;
    public string SortBy { get; set; } = "PriceDesc";
}
