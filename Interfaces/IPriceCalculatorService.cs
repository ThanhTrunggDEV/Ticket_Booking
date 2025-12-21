using Ticket_Booking.Enums;
using Ticket_Booking.Models.DomainModels;

namespace Ticket_Booking.Interfaces
{
    /// <summary>
    /// Service for calculating ticket prices for one-way and round-trip bookings
    /// </summary>
    public interface IPriceCalculatorService
    {
        /// <summary>
        /// Calculates price for one-way ticket
        /// </summary>
        /// <param name="trip">The trip for the one-way ticket</param>
        /// <param name="seatClass">The seat class (Economy, Business, FirstClass)</param>
        /// <returns>The price for the one-way ticket</returns>
        decimal CalculateOneWayPrice(Trip trip, SeatClass seatClass);
        
        /// <summary>
        /// Calculates price for round-trip booking (same seat class for both legs)
        /// </summary>
        /// <param name="outboundTrip">The outbound trip</param>
        /// <param name="returnTrip">The return trip</param>
        /// <param name="seatClass">The seat class for both legs</param>
        /// <returns>Price breakdown including discount and savings</returns>
        RoundTripPriceBreakdown CalculateRoundTripPrice(
            Trip outboundTrip, 
            Trip returnTrip, 
            SeatClass seatClass);
        
        /// <summary>
        /// Calculates price for round-trip booking (different seat classes per leg)
        /// </summary>
        /// <param name="outboundTrip">The outbound trip</param>
        /// <param name="returnTrip">The return trip</param>
        /// <param name="outboundSeatClass">The seat class for outbound leg</param>
        /// <param name="returnSeatClass">The seat class for return leg</param>
        /// <returns>Price breakdown including discount and savings</returns>
        RoundTripPriceBreakdown CalculateRoundTripPrice(
            Trip outboundTrip, 
            Trip returnTrip, 
            SeatClass outboundSeatClass,
            SeatClass returnSeatClass);
    }
    
    /// <summary>
    /// Price breakdown for round-trip bookings
    /// </summary>
    public class RoundTripPriceBreakdown
    {
        public decimal OutboundPrice { get; set; }
        public decimal ReturnPrice { get; set; }
        public decimal Subtotal { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal SavingsAmount { get; set; }  // vs two one-way tickets
    }
}

