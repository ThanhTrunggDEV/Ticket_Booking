namespace Ticket_Booking.Models.CurrencyModels
{
    /// <summary>
    /// Represents currency conversion information including exchange rate and metadata
    /// </summary>
    public class CurrencyConversion
    {
        public string FromCurrency { get; set; } = string.Empty; // "USD"
        public string ToCurrency { get; set; } = string.Empty;   // "VND"
        public decimal ExchangeRate { get; set; }
        public DateTime LastUpdated { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsFromCache { get; set; }    // Indicates if rate came from cache
        public bool IsFallback { get; set; }     // Indicates if using fallback/default rate
    }
}

