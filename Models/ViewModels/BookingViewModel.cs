using Ticket_Booking.Enums;
using Ticket_Booking.Interfaces;
using Ticket_Booking.Models.DomainModels;

namespace Ticket_Booking.Models.ViewModels
{
    /// <summary>
    /// ViewModel for booking flights (one-way or round-trip)
    /// </summary>
    public class BookingViewModel
    {
        public TicketType TicketType { get; set; } = TicketType.OneWay;
        
        // Outbound trip (required for both one-way and round-trip)
        public Trip? OutboundTrip { get; set; }
        public int? OutboundTripId { get; set; }
        
        // Return trip (only for round-trip)
        public Trip? ReturnTrip { get; set; }
        public int? ReturnTripId { get; set; }
        
        // Seat class selection
        public SeatClass SeatClass { get; set; } = SeatClass.Economy;
        public SeatClass? OutboundSeatClass { get; set; }  // For different classes per leg
        public SeatClass? ReturnSeatClass { get; set; }    // For different classes per leg
        
        // Pricing breakdown (for round-trip)
        public decimal? OutboundPrice { get; set; }
        public decimal? ReturnPrice { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal? TotalPrice { get; set; }
        public decimal? SavingsAmount { get; set; }  // Compared to two one-way tickets
        public decimal? DiscountPercent { get; set; }
        
        // Price breakdown object (from PriceCalculatorService)
        public RoundTripPriceBreakdown? PriceBreakdown { get; set; }
        
        // Available return trips (for round-trip selection)
        public IEnumerable<Trip>? AvailableReturnTrips { get; set; }
        
        // Validation and error messages
        public string? ErrorMessage { get; set; }
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    }
}

