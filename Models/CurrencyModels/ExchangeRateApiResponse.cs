using System.Text.Json.Serialization;

namespace Ticket_Booking.Models.CurrencyModels
{
    /// <summary>
    /// Response model from ExchangeRate-API
    /// </summary>
    public class ExchangeRateApiResponse
    {
        [JsonPropertyName("base")]
        public string Base { get; set; } = string.Empty;                 // "USD"
        
        [JsonPropertyName("date")]
        public DateTime Date { get; set; }
        
        [JsonPropertyName("rates")]
        public Dictionary<string, decimal> Rates { get; set; } = new();  // {"VND": 24500.00, ...}
    }
}

