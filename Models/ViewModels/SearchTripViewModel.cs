using Ticket_Booking.Models.DomainModels;

namespace Ticket_Booking.Models.ViewModels
{
    public class SearchTripViewModel
    {
        public string? FromCity { get; set; }
        public string? ToCity { get; set; }
        public DateTime? Date { get; set; }
        public IEnumerable<Trip> Trips { get; set; } = new List<Trip>();
        public IEnumerable<string> AvailableCities { get; set; } = new List<string>();
    }
}
