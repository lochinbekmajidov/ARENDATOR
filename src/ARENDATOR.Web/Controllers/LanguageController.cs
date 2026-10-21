using System.Globalization;
using Microsoft.AspNetCore.Mvc;

namespace ARENDATOR.Web.Controllers;

public class LanguageController : Controller
{
    [HttpGet("/set-language")]
    public IActionResult SetLanguage(string culture, string returnUrl = "/")
    {
        var normalizedCulture = culture?.Trim().ToLowerInvariant() switch
        {
            "tg" or "tj" or "tjk" or "tajik" => "tj",
            "uz" or "uzb" or "uzbek" => "uz",
            "en" or "english" => "en",
            "ru" or "rus" or "russian" => "ru",
            _ => "tj"
        };

        Response.Cookies.Append("ARENDATOR_Culture", normalizedCulture,
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true,
                SameSite = SameSiteMode.Lax
            });

        CultureInfo.CurrentCulture = new CultureInfo(normalizedCulture);
        CultureInfo.CurrentUICulture = new CultureInfo(normalizedCulture);

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToAction("Index", "Home");
    }
}
