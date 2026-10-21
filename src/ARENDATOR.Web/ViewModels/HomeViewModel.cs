using ARENDATOR.Web.Models;

namespace ARENDATOR.Web.ViewModels;

public class HomeViewModel
{
    public List<Property> FeaturedProperties { get; set; } = new();
    public List<Property> RecentlyAdded { get; set; } = new();
}
