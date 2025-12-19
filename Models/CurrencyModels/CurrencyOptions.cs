namespace Ticket_Booking.Models.CurrencyModels
{
    /// <summary>
    /// Configuration options for currency service
    /// </summary>
    public class CurrencyOptions
    {
        public const string SectionName = "Currency";
        
        public string DefaultCurrency { get; set; } = "USD";      // Default currency
        public decimal DefaultExchangeRate { get; set; } = 24500m;  // Fallback rate (e.g., 24500 VND per USD)
        public int CacheExpirationMinutes { get; set; } = 60;  // 60 (1 hour)
        public string ApiProvider { get; set; } = "ExchangeRate-API";          // "ExchangeRate-API"
        public string ApiEndpoint { get; set; } = "https://api.exchangerate-api.com/v4/latest/USD";
        public string ApiKey { get; set; } = string.Empty;              // Optional
    }
}

