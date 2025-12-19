using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;
using Ticket_Booking.Models.CurrencyModels;

namespace Ticket_Booking.Services
{
    /// <summary>
    /// Service for currency conversion and exchange rate management
    /// </summary>
    public class CurrencyService : ICurrencyService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly ILogger<CurrencyService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly CurrencyOptions _options;
        
        private const string CurrencyCookieName = "currency";
        private const string SupportedCurrencies = "USD,VND";
        private static readonly string[] SupportedCurrencyList = { "USD", "VND" };

        public CurrencyService(
            HttpClient httpClient,
            IMemoryCache cache,
            ILogger<CurrencyService> logger,
            IHttpContextAccessor httpContextAccessor,
            IOptions<CurrencyOptions> options)
        {
            _httpClient = httpClient;
            _cache = cache;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _options = options.Value;
        }

        public string GetCurrentCurrency()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
                return _options.DefaultCurrency;

            // Try to get from cookie
            var currency = httpContext.Request.Cookies[CurrencyCookieName];
            if (!string.IsNullOrEmpty(currency) && IsCurrencySupported(currency))
                return currency;

            // Fallback: Check language preference for default
            var culture = httpContext.Features.Get<Microsoft.AspNetCore.Localization.IRequestCultureFeature>()?.RequestCulture?.Culture;
            if (culture != null && culture.Name.StartsWith("vi", StringComparison.OrdinalIgnoreCase))
                return "VND";

            return _options.DefaultCurrency;
        }

        public bool IsCurrencySupported(string currency)
        {
            return SupportedCurrencyList.Contains(currency?.ToUpperInvariant());
        }

        public async Task<decimal> GetExchangeRateAsync(string fromCurrency, string toCurrency)
        {
            var info = await GetExchangeRateInfoAsync(fromCurrency, toCurrency);
            return info.ExchangeRate;
        }

        public async Task<CurrencyConversion> GetExchangeRateInfoAsync(string fromCurrency, string toCurrency)
        {
            fromCurrency = fromCurrency?.ToUpperInvariant() ?? "USD";
            toCurrency = toCurrency?.ToUpperInvariant() ?? "USD";

            // Same currency, no conversion needed
            if (fromCurrency == toCurrency)
            {
                return new CurrencyConversion
                {
                    FromCurrency = fromCurrency,
                    ToCurrency = toCurrency,
                    ExchangeRate = 1m,
                    LastUpdated = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddHours(24),
                    IsFromCache = false,
                    IsFallback = false
                };
            }

            // Normalize to USD as base (ExchangeRate-API uses USD as base)
            string targetCurrency = toCurrency;
            
            if (fromCurrency != "USD")
            {
                // Need to convert: fromCurrency -> USD -> toCurrency
                // For now, we'll handle USD -> VND and VND -> USD
                // More complex conversions can be added later
                if (fromCurrency == "VND" && toCurrency == "USD")
                {
                    // Inverse conversion
                    var usdToVnd = await GetUsdToVndRateAsync();
                    return new CurrencyConversion
                    {
                        FromCurrency = fromCurrency,
                        ToCurrency = toCurrency,
                        ExchangeRate = 1m / usdToVnd.ExchangeRate,
                        LastUpdated = usdToVnd.LastUpdated,
                        ExpiresAt = usdToVnd.ExpiresAt,
                        IsFromCache = usdToVnd.IsFromCache,
                        IsFallback = usdToVnd.IsFallback
                    };
                }
                else if (fromCurrency == "USD" && toCurrency == "VND")
                {
                    return await GetUsdToVndRateAsync();
                }
                else
                {
                    _logger.LogWarning("Unsupported currency conversion: {From} -> {To}", fromCurrency, toCurrency);
                    return CreateFallbackConversion(fromCurrency, toCurrency);
                }
            }

            // USD -> VND
            if (targetCurrency == "VND")
            {
                return await GetUsdToVndRateAsync();
            }

            // Unsupported conversion
            _logger.LogWarning("Unsupported currency conversion: {From} -> {To}", fromCurrency, toCurrency);
            return CreateFallbackConversion(fromCurrency, toCurrency);
        }

        private async Task<CurrencyConversion> GetUsdToVndRateAsync()
        {
            const string cacheKey = "ExchangeRate_USD_VND";

            // Try cache first
            if (_cache.TryGetValue(cacheKey, out CurrencyConversion? cachedRate) && cachedRate != null)
            {
                if (cachedRate.ExpiresAt > DateTime.UtcNow)
                {
                    _logger.LogDebug("Exchange rate retrieved from cache (expires: {ExpiresAt})", cachedRate.ExpiresAt);
                    return new CurrencyConversion
                    {
                        FromCurrency = cachedRate.FromCurrency,
                        ToCurrency = cachedRate.ToCurrency,
                        ExchangeRate = cachedRate.ExchangeRate,
                        LastUpdated = cachedRate.LastUpdated,
                        ExpiresAt = cachedRate.ExpiresAt,
                        IsFromCache = true,
                        IsFallback = false
                    };
                }
                else
                {
                    _logger.LogDebug("Cached rate expired at {ExpiresAt}, fetching new rate", cachedRate.ExpiresAt);
                }
            }
            else
            {
                _logger.LogDebug("Cache miss - no cached rate found");
            }

            // Cache expired or not found, fetch from API
            _logger.LogDebug("Cache miss or expired, fetching from API");
            
            const int maxRetries = 2;
            for (int attempt = 0; attempt <= maxRetries; attempt++)
            {
                try
                {
                    if (attempt > 0)
                    {
                        // Exponential backoff: 1s, 2s
                        var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt - 1));
                        _logger.LogInformation("Retrying API call after {Delay}ms (attempt {Attempt}/{MaxRetries})", delay.TotalMilliseconds, attempt + 1, maxRetries + 1);
                        await Task.Delay(delay);
                    }

                    var response = await _httpClient.GetAsync(_options.ApiEndpoint);
                    
                    // Handle specific HTTP status codes
                    if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        _logger.LogWarning("API rate limit exceeded (429). Using cached or fallback rate.");
                        break; // Exit retry loop, will use fallback
                    }
                    
                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("API returned non-success status: {StatusCode} {ReasonPhrase}", 
                            response.StatusCode, response.ReasonPhrase);
                        if (attempt < maxRetries)
                            continue; // Retry
                        break; // Exit retry loop, will use fallback
                    }

                    var json = await response.Content.ReadAsStringAsync();
                    var apiResponse = System.Text.Json.JsonSerializer.Deserialize<ExchangeRateApiResponse>(json, new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (apiResponse?.Rates != null && apiResponse.Rates.TryGetValue("VND", out var rate))
                    {
                        // Validate rate is reasonable (between 20,000 and 30,000 VND per USD)
                        if (rate < 20000 || rate > 30000)
                        {
                            _logger.LogWarning("Exchange rate seems invalid: {Rate} VND/USD. Using fallback.", rate);
                            break; // Exit retry loop, will use fallback
                        }

                        var conversion = new CurrencyConversion
                        {
                            FromCurrency = "USD",
                            ToCurrency = "VND",
                            ExchangeRate = rate,
                            LastUpdated = DateTime.UtcNow,
                            ExpiresAt = DateTime.UtcNow.AddMinutes(_options.CacheExpirationMinutes),
                            IsFromCache = false,
                            IsFallback = false
                        };

                        // Cache the result
                        _cache.Set(cacheKey, conversion, TimeSpan.FromMinutes(_options.CacheExpirationMinutes + 5)); // Cache slightly longer than expiration

                        _logger.LogInformation("Exchange rate fetched from API: 1 USD = {Rate} VND", rate);
                        return conversion;
                    }
                    else
                    {
                        _logger.LogWarning("API response missing VND rate or invalid response structure");
                        if (attempt < maxRetries)
                            continue; // Retry
                        break; // Exit retry loop, will use fallback
                    }
                }
                catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
                {
                    _logger.LogWarning("API request timed out (attempt {Attempt}/{MaxRetries})", attempt + 1, maxRetries + 1);
                    if (attempt < maxRetries)
                        continue; // Retry
                    break; // Exit retry loop, will use fallback
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogWarning(ex, "HTTP request failed (attempt {Attempt}/{MaxRetries})", attempt + 1, maxRetries + 1);
                    if (attempt < maxRetries)
                        continue; // Retry
                    break; // Exit retry loop, will use fallback
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error fetching exchange rate (attempt {Attempt}/{MaxRetries})", attempt + 1, maxRetries + 1);
                    if (attempt < maxRetries)
                        continue; // Retry
                    break; // Exit retry loop, will use fallback
                }
            }

            // All retries failed or rate limit exceeded - use fallback
            _logger.LogWarning("All API attempts failed. Using fallback mechanism.");

            // Try to use expired cache if available
            if (_cache.TryGetValue(cacheKey, out CurrencyConversion? expiredCache) && expiredCache != null)
            {
                _logger.LogInformation("Using expired cached rate as fallback (last updated: {LastUpdated})", expiredCache.LastUpdated);
                return new CurrencyConversion
                {
                    FromCurrency = expiredCache.FromCurrency,
                    ToCurrency = expiredCache.ToCurrency,
                    ExchangeRate = expiredCache.ExchangeRate,
                    LastUpdated = expiredCache.LastUpdated,
                    ExpiresAt = expiredCache.ExpiresAt,
                    IsFromCache = true,
                    IsFallback = true
                };
            }

            // Use default fallback rate
            _logger.LogWarning("Using default exchange rate from configuration: {Rate}", _options.DefaultExchangeRate);
            return CreateFallbackConversion("USD", "VND");
        }

        private CurrencyConversion CreateFallbackConversion(string fromCurrency, string toCurrency)
        {
            decimal rate = 1m;
            if (fromCurrency == "USD" && toCurrency == "VND")
            {
                rate = _options.DefaultExchangeRate;
            }
            else if (fromCurrency == "VND" && toCurrency == "USD")
            {
                rate = 1m / _options.DefaultExchangeRate;
            }

            return new CurrencyConversion
            {
                FromCurrency = fromCurrency,
                ToCurrency = toCurrency,
                ExchangeRate = rate,
                LastUpdated = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                IsFromCache = false,
                IsFallback = true
            };
        }

        public async Task<decimal> ConvertAmountAsync(decimal amount, string fromCurrency, string toCurrency)
        {
            if (fromCurrency == toCurrency)
                return amount;

            var conversion = await GetExchangeRateInfoAsync(fromCurrency, toCurrency);
            return Math.Round(amount * conversion.ExchangeRate, 2, MidpointRounding.AwayFromZero);
        }

        public async Task<PriceDisplay> FormatPriceAsync(decimal usdAmount, string targetCurrency)
        {
            // Validate input
            if (usdAmount < 0)
            {
                _logger.LogWarning("Negative amount provided: {Amount}. Using absolute value.", usdAmount);
                usdAmount = Math.Abs(usdAmount);
            }

            targetCurrency = targetCurrency?.ToUpperInvariant() ?? GetCurrentCurrency();
            
            if (!IsCurrencySupported(targetCurrency))
            {
                _logger.LogWarning("Unsupported target currency: {Currency}. Defaulting to {DefaultCurrency}", targetCurrency, _options.DefaultCurrency);
                targetCurrency = _options.DefaultCurrency;
            }

            try
            {
                var conversion = await GetExchangeRateInfoAsync("USD", targetCurrency);
                var convertedAmount = await ConvertAmountAsync(usdAmount, "USD", targetCurrency);

                // Format based on currency
                string formattedString;
                if (targetCurrency == "VND")
                {
                    // VND: No decimals, use ₫ symbol
                    formattedString = $"₫{convertedAmount:N0}";
                }
                else
                {
                    // USD: 2 decimals, use $ symbol
                    formattedString = $"${convertedAmount:N2}";
                }

                return new PriceDisplay
                {
                    OriginalAmount = usdAmount,
                    OriginalCurrency = "USD",
                    ConvertedAmount = convertedAmount,
                    Currency = targetCurrency,
                    ExchangeRate = conversion.ExchangeRate,
                    FormattedString = formattedString,
                    IsApproximate = conversion.IsFallback || conversion.IsFromCache
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error formatting price for {Amount} USD to {Currency}. Using USD fallback.", usdAmount, targetCurrency);
                
                // Graceful degradation: return USD format if conversion fails
                return new PriceDisplay
                {
                    OriginalAmount = usdAmount,
                    OriginalCurrency = "USD",
                    ConvertedAmount = usdAmount,
                    Currency = "USD",
                    ExchangeRate = 1m,
                    FormattedString = $"${usdAmount:N2}",
                    IsApproximate = true
                };
            }
        }

        public async Task RefreshExchangeRateAsync(string fromCurrency, string toCurrency)
        {
            fromCurrency = fromCurrency?.ToUpperInvariant() ?? "USD";
            toCurrency = toCurrency?.ToUpperInvariant() ?? "VND";

            // Rate limiting: Check if refresh was called recently (within last 5 seconds)
            const string rateLimitKey = "CurrencyRefresh_RateLimit";
            if (_cache.TryGetValue(rateLimitKey, out DateTime? lastRefresh) && lastRefresh.HasValue)
            {
                var timeSinceLastRefresh = DateTime.UtcNow - lastRefresh.Value;
                if (timeSinceLastRefresh < TimeSpan.FromSeconds(5))
                {
                    _logger.LogWarning("Refresh rate limit: Last refresh was {Seconds} seconds ago. Skipping refresh.", timeSinceLastRefresh.TotalSeconds);
                    return; // Skip refresh to prevent API spam
                }
            }

            // Set rate limit marker
            _cache.Set(rateLimitKey, DateTime.UtcNow, TimeSpan.FromSeconds(5));

            // Clear cache
            const string cacheKey = "ExchangeRate_USD_VND";
            _cache.Remove(cacheKey);

            // Force fetch new rate
            await GetExchangeRateInfoAsync(fromCurrency, toCurrency);
            
            _logger.LogInformation("Exchange rate refreshed for {From} -> {To}", fromCurrency, toCurrency);
        }
    }
}

