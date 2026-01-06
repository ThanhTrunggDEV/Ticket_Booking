using Ticket_Booking.Enums;
using Ticket_Booking.Models.DomainModels;

namespace Ticket_Booking.Models.ViewModels
{
    /// <summary>
    /// ViewModel for ticket change request
    /// </summary>
    public class TicketChangeViewModel
    {
        // Original ticket information
        public Ticket OriginalTicket { get; set; } = null!;
        public Trip OriginalTrip { get; set; } = null!;
        
        // New trip selection
        public int? NewTripId { get; set; }
        public Trip? NewTrip { get; set; }
        public SeatClass? NewSeatClass { get; set; }  // Optional: allow changing seat class
        
        // Change fee calculation
        public decimal ChangeFee { get; set; }  // Fixed change fee based on airline policy
        public decimal PriceDifference { get; set; }  // Difference between old and new ticket price
        public decimal TotalAmountDue { get; set; }  // Change fee + price difference (if new ticket is more expensive)
        public decimal RefundAmount { get; set; }  // If new ticket is cheaper, refund the difference
        
        // Change policy information
        public bool IsChangeAllowed { get; set; }
        public string? ChangePolicyMessage { get; set; }
        public int HoursBeforeDeparture { get; set; }  // Hours remaining before original departure
        
        // Available trips for selection
        public IEnumerable<Trip>? AvailableTrips { get; set; }
        
        // Validation
        public string? ErrorMessage { get; set; }
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    }
}

