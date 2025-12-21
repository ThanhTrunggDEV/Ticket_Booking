using Ticket_Booking.Enums;
using Ticket_Booking.Interfaces;
using Ticket_Booking.Models.DomainModels;

namespace Ticket_Booking.Services
{
    /// <summary>
    /// Service for calculating ticket prices for one-way and round-trip bookings
    /// </summary>
    public class PriceCalculatorService : IPriceCalculatorService
    {
        private readonly IPricingService _pricingService;

        public PriceCalculatorService(IPricingService pricingService)
        {
            _pricingService = pricingService;
        }

        /// <summary>
        /// Calculates price for one-way ticket
        /// </summary>
        public decimal CalculateOneWayPrice(Trip trip, SeatClass seatClass)
        {
            if (trip == null)
                throw new ArgumentNullException(nameof(trip));

            return seatClass switch
            {
                SeatClass.Economy => trip.EconomyPrice,
                SeatClass.Business => trip.BusinessPrice,
                SeatClass.FirstClass => trip.FirstClassPrice,
                _ => throw new ArgumentException($"Invalid seat class: {seatClass}", nameof(seatClass))
            };
        }

        /// <summary>
        /// Calculates price for round-trip booking (same seat class for both legs)
        /// </summary>
        public RoundTripPriceBreakdown CalculateRoundTripPrice(
            Trip outboundTrip, 
            Trip returnTrip, 
            SeatClass seatClass)
        {
            return CalculateRoundTripPrice(outboundTrip, returnTrip, seatClass, seatClass);
        }

        /// <summary>
        /// Calculates price for round-trip booking (different seat classes per leg)
        /// </summary>
        public RoundTripPriceBreakdown CalculateRoundTripPrice(
            Trip outboundTrip, 
            Trip returnTrip, 
            SeatClass outboundSeatClass,
            SeatClass returnSeatClass)
        {
            if (outboundTrip == null)
                throw new ArgumentNullException(nameof(outboundTrip));
            if (returnTrip == null)
                throw new ArgumentNullException(nameof(returnTrip));

            // Calculate base prices for each leg
            var outboundPrice = CalculateOneWayPrice(outboundTrip, outboundSeatClass);
            var returnPrice = CalculateOneWayPrice(returnTrip, returnSeatClass);
            var subtotal = outboundPrice + returnPrice;

            // Get discount percentage for the route
            // Use the outbound trip's discount (both trips should have same discount for same route)
            var discountPercent = outboundTrip.RoundTripDiscountPercent;
            
            // If discount is 0, try to get from pricing service (for route-level discount)
            if (discountPercent == 0)
            {
                discountPercent = _pricingService.GetRoundTripDiscount(
                    outboundTrip.CompanyId, 
                    outboundTrip.FromCity, 
                    outboundTrip.ToCity);
            }

            // Calculate discount amount and total
            var discountAmount = subtotal * (discountPercent / 100m);
            var totalPrice = subtotal - discountAmount;

            // Calculate savings compared to two one-way tickets (without discount)
            var twoOneWayTotal = outboundPrice + returnPrice;
            var savingsAmount = twoOneWayTotal - totalPrice;

            return new RoundTripPriceBreakdown
            {
                OutboundPrice = outboundPrice,
                ReturnPrice = returnPrice,
                Subtotal = subtotal,
                DiscountPercent = discountPercent,
                DiscountAmount = discountAmount,
                TotalPrice = totalPrice,
                SavingsAmount = savingsAmount
            };
        }
    }
}

