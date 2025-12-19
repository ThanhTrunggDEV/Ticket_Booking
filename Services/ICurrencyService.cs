using Ticket_Booking.Models.CurrencyModels;

namespace Ticket_Booking.Services
{
    /// <summary>
    /// Service interface for currency conversion and exchange rate management
    /// </summary>
    public interface ICurrencyService
    {
        // Get exchange rate (from cache, API, or fallback)
        Task<decimal> GetExchangeRateAsync(string fromCurrency, string toCurrency);
        
        // Get exchange rate with metadata (cache status, fallback info)
        Task<CurrencyConversion> GetExchangeRateInfoAsync(string fromCurrency, string toCurrency);
        
        // Convert amount between currencies
        Task<decimal> ConvertAmountAsync(decimal amount, string fromCurrency, string toCurrency);
        
        // Format price for display (with currency symbol, rounding)
        Task<PriceDisplay> FormatPriceAsync(decimal usdAmount, string targetCurrency);
        
        // Get current user's selected currency
        string GetCurrentCurrency();
        
        // Check if currency is supported
        bool IsCurrencySupported(string currency);
        
        // Refresh exchange rate (force API call, update cache)
        Task RefreshExchangeRateAsync(string fromCurrency, string toCurrency);
    }
}

