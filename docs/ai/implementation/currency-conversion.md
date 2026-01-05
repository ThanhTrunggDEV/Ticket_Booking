---
phase: implementation
title: Implementation Guide
description: Technical implementation notes, patterns, and code guidelines
---

# Implementation Guide

## Development Setup
**How do we get started?**

- Prerequisites and dependencies:
  - ASP.NET Core 8.0 (already in project)
  - HttpClient for API calls (built-in)
  - IMemoryCache or IDistributedCache for caching (built-in)
  - Exchange Rate API account (to be created)
- Environment setup steps:
  - Sign up for exchange rate API (e.g., ExchangeRate-API free tier)
  - Obtain API key if required
  - Add API key to appsettings.json (or user secrets for development)
- Configuration needed:
  - Add API endpoint and key to configuration
  - Configure cache expiration time
  - Set default exchange rate (fallback)

## Code Structure
**How is the code organized?**

- Directory structure:
  ```
  Services/
    CurrencyService.cs (new)
    ICurrencyService.cs (new)
  Controllers/
    CurrencyController.cs (new)
  Views/
    Shared/
      _CurrencySwitcher.cshtml (new partial view)
  Helpers/
    CurrencyHelpers.cs (new - optional)
  Models/
    CurrencyConversion.cs (new - optional)
    PriceDisplay.cs (new - optional)
  ```
- Module organization:
  - CurrencyService handles all currency logic
  - CurrencyController handles currency switching
  - View helpers format prices for display
  - Models represent currency data structures
- Naming conventions:
  - Service interfaces: I{Name}Service
  - Services: {Name}Service
  - Controllers: {Name}Controller
  - Views: PascalCase with underscores for partials

## Implementation Notes
**Key technical details to remember:**

### Core Features

#### CurrencyService Implementation
- Use dependency injection for HttpClient and IMemoryCache
- Implement async methods for API calls
- Cache key format: "ExchangeRate_{FromCurrency}_{ToCurrency}"
- Cache expiration: 1 hour (configurable)
- Fallback exchange rate: Store in configuration (e.g., 24,500 VND = 1 USD)
- Error handling:
  - Try cache first
  - If cache miss, call API
  - If API fails, use fallback rate
  - Log all failures

#### Exchange Rate API Client
- Recommended API: ExchangeRate-API (free tier: 1,500 requests/month)
  - Endpoint: `https://api.exchangerate-api.com/v4/latest/USD`
  - No API key required for free tier
  - Returns JSON with base currency and rates
- Alternative: Fixer.io (requires API key, more features)
- Implementation pattern:
  ```csharp
  public class ExchangeRateApiClient
  {
      private readonly HttpClient _httpClient;
      private readonly ILogger<ExchangeRateApiClient> _logger;
      
      public async Task<decimal> GetExchangeRateAsync(string fromCurrency, string toCurrency)
      {
          // Implementation
      }
  }
  ```

#### CurrencyController
- Similar pattern to LanguageController
- Validate currency parameter (whitelist: "VND", "USD")
- Set cookie with currency preference
- Safe redirect implementation
- Cookie options:
  - Expires: 1 year
  - IsEssential: true
  - SameSite: Lax

#### Price Display Helpers
- HtmlHelper extension:
  ```csharp
  public static class CurrencyHtmlHelper
  {
      public static IHtmlContent DisplayPrice(this IHtmlHelper helper, decimal usdAmount, ICurrencyService currencyService)
      {
          // Get user's currency preference
          // Convert amount
          // Format with currency symbol
          // Return formatted HTML
      }
  }
  ```
- Currency symbols:
  - VND: ₫ (or "VND")
  - USD: $ (or "USD")
- Number formatting:
  - VND: No decimals, comma separators (e.g., 1,500,000 ₫)
  - USD: 2 decimals, comma separators (e.g., $1,500.00)

### Patterns & Best Practices
- Use async/await for all API calls
- Implement retry logic for API calls (Polly library if needed)
- Cache aggressively to reduce API calls
- Log all API failures for monitoring
- Use configuration for all magic numbers (exchange rates, cache expiration)
- Follow existing localization pattern for consistency
- Use dependency injection throughout

## Integration Points
**How do pieces connect?**

- CurrencyService integrates with:
  - HttpClient for API calls
  - IMemoryCache for caching
  - IConfiguration for settings
- CurrencyController integrates with:
  - Cookie management (HttpContext.Response.Cookies)
  - Routing system (returnUrl redirect)
- Views integrate with:
  - CurrencyService (via dependency injection)
  - Price display helpers
  - Currency switcher partial view
- Middleware integrates with:
  - Request pipeline (reads cookie, sets currency context)

## Error Handling
**How do we handle failures?**

- API unavailable:
  - Use cached exchange rate if available
  - Fall back to default rate from configuration
  - Log error for monitoring
  - Continue with fallback (don't break user experience)
- Invalid API response:
  - Validate response structure
  - Use fallback rate
  - Log error
- Network timeout:
  - Set reasonable timeout (e.g., 5 seconds)
  - Use cached rate or fallback
  - Log timeout
- Cache miss and API failure:
  - Use default exchange rate from configuration
  - Log warning
  - Continue operation

## Performance Considerations
**How do we keep it fast?**

- Caching strategy:
  - Cache exchange rates for 1 hour
  - Use IMemoryCache (fast, in-memory)
  - Consider IDistributedCache for multi-server (Redis)
- API call optimization:
  - Only fetch when cache expired
  - Use async operations (non-blocking)
  - Set reasonable timeout
- Price conversion:
  - Cache converted prices if same amount and currency
  - Use efficient decimal operations
  - Avoid repeated conversions in loops

## Security Notes
**What security measures are in place?**

- Currency parameter validation:
  - Whitelist approach (only allow "VND", "USD")
  - Reject any other values
- ReturnUrl validation:
  - Use Url.IsLocalUrl() to prevent open redirects
  - Only allow relative URLs or same-domain URLs
- API key security:
  - Store in configuration (not in code)
  - Use user secrets for development
  - Use environment variables or secure vault for production
- Cookie security:
  - SameSite = Lax (CSRF protection)
  - IsEssential = true (GDPR compliance)
  - HttpOnly = false (needed for JavaScript access if required)



