namespace Ticket_Booking.Models.CurrencyModels
{
    /// <summary>
    /// Represents a price formatted for display with currency information
    /// </summary>
    public class PriceDisplay
    {
        public decimal OriginalAmount { get; set; }      // USD (source)
        public string OriginalCurrency { get; set; } = "USD";
        public decimal ConvertedAmount { get; set; }     // VND or USD (converted)
        public string Currency { get; set; } = string.Empty;             // "VND" or "USD" (target)
        public decimal ExchangeRate { get; set; }
        public string FormattedString { get; set; } = string.Empty;      // Formatted display (e.g., "₫1,500,000" or "$65.00")
        public bool IsApproximate { get; set; }          // True if using cached/fallback rate
    }
}

