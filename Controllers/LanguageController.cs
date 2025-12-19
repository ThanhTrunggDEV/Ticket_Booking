using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace Ticket_Booking.Controllers
{
    public class LanguageController : Controller
    {
        [HttpGet]
        [HttpPost]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            // Validate culture parameter (whitelist approach)
            var supportedCultures = new[] { "vi", "en" };
            if (!supportedCultures.Contains(culture))
            {
                culture = "vi"; // Default to Vietnamese if invalid
            }

            // Set cookie with culture preference
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    IsEssential = true,
                    SameSite = SameSiteMode.Lax
                }
            );

            // Safe redirect - prevent open redirect attacks
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            // Fallback to referrer or home page
            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer) && Url.IsLocalUrl(referer))
            {
                return LocalRedirect(referer);
            }

            return RedirectToAction("Index", "Login");
        }
    }
}

