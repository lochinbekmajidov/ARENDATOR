using System.Diagnostics;
using ARENDATOR.Web.Models;
using ARENDATOR.Web.Services;
using ARENDATOR.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ARENDATOR.Web.Controllers;

public class HomeController : Controller
{
    private readonly PropertyService _propertyService;
    private readonly ILogger<HomeController> _logger;

    public HomeController(PropertyService propertyService, ILogger<HomeController> logger)
    {
        _propertyService = propertyService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var featured = await _propertyService.GetFeaturedPropertiesAsync();
        var recent = await _propertyService.GetRecentPropertiesAsync(4);

        var model = new HomeViewModel
        {
            FeaturedProperties = featured,
            RecentlyAdded = recent
        };

        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
