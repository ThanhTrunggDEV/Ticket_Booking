using Ticket_Booking.Models.DomainModels;
using Ticket_Booking.Enums;

namespace Ticket_Booking.Models.ViewModels
{
    public class SearchTripViewModel
    {
        public string? FromCity { get; set; }
        public string? ToCity { get; set; }
        public DateTime? Date { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string TripType { get; set; } = "OneWay"; // OneWay or RoundTrip
        public SortCriteria SortBy { get; set; } = SortCriteria.DepartureTimeAsc;
        public SeatClass SeatClass { get; set; } = SeatClass.Economy;
        public IEnumerable<Trip> Trips { get; set; } = new List<Trip>();
        public IEnumerable<string> AvailableCities { get; set; } = new List<string>();
    }
}
