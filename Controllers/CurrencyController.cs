using Microsoft.AspNetCore.Mvc;
using Ticket_Booking.Services;

namespace Ticket_Booking.Controllers
{
    public class CurrencyController : Controller
    {
        private const string CurrencyCookieName = "currency";
        private static readonly string[] SupportedCurrencies = { "USD", "VND" };
        private readonly ICurrencyService _currencyService;
        private readonly ILogger<CurrencyController> _logger;

        public CurrencyController(ICurrencyService currencyService, ILogger<CurrencyController> logger)
        {
            _currencyService = currencyService;
            _logger = logger;
        }

        /// <summary>
        /// Health check endpoint for currency service
        /// </summary>
        [HttpGet]
        [Route("/api/currency/health")]
        public async Task<IActionResult> HealthCheck()
        {
            try
            {
                var rateInfo = await _currencyService.GetExchangeRateInfoAsync("USD", "VND");
                var currentCurrency = _currencyService.GetCurrentCurrency();
                
                return Ok(new
                {
                    status = "healthy",
                    service = "CurrencyService",
                    currentCurrency = currentCurrency,
                    exchangeRate = new
                    {
                        from = rateInfo.FromCurrency,
                        to = rateInfo.ToCurrency,
                        rate = rateInfo.ExchangeRate,
                        lastUpdated = rateInfo.LastUpdated,
                        expiresAt = rateInfo.ExpiresAt,
                        isFromCache = rateInfo.IsFromCache,
                        isFallback = rateInfo.IsFallback
                    },
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed");
                return StatusCode(503, new
                {
                    status = "unhealthy",
                    service = "CurrencyService",
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        [HttpGet]
        [HttpPost]
        public IActionResult SetCurrency(string currency, string returnUrl)
        {
            // Validate currency parameter (whitelist approach)
            if (string.IsNullOrEmpty(currency) || !SupportedCurrencies.Contains(currency.ToUpperInvariant()))
            {
                currency = "USD"; // Default to USD if invalid
            }
            else
            {
                currency = currency.ToUpperInvariant();
            }

            // Set cookie with currency preference
            Response.Cookies.Append(
                CurrencyCookieName,
                currency,
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

