using Microsoft.EntityFrameworkCore;
using Ticket_Booking.Interfaces;
using Ticket_Booking.Repositories;

namespace Ticket_Booking.Services
{
    /// <summary>
    /// Service for managing round-trip pricing and discounts
    /// </summary>
    public class PricingService : IPricingService
    {
        private readonly TripRepository _tripRepository;
        private const decimal MIN_DISCOUNT = 0m;
        private const decimal MAX_DISCOUNT = 50m;

        public PricingService(TripRepository tripRepository)
        {
            _tripRepository = tripRepository;
        }

        /// <summary>
        /// Gets round-trip discount percentage for a route
        /// Returns the discount from the first trip found for the route, or 0 if none found
        /// </summary>
        public decimal GetRoundTripDiscount(int companyId, string fromCity, string toCity)
        {
            if (string.IsNullOrWhiteSpace(fromCity))
                throw new ArgumentException("FromCity cannot be null or empty", nameof(fromCity));
            if (string.IsNullOrWhiteSpace(toCity))
                throw new ArgumentException("ToCity cannot be null or empty", nameof(toCity));

            // Find the first trip for this route to get the discount
            // All trips in the same route should have the same discount
            var trip = _tripRepository.FindAsync(t => 
                t.CompanyId == companyId && 
                t.FromCity == fromCity && 
                t.ToCity == toCity).Result.FirstOrDefault();

            return trip?.RoundTripDiscountPercent ?? 0m;
        }

        /// <summary>
        /// Validates discount percentage (must be 0-50%)
        /// </summary>
        public bool ValidateDiscount(decimal discountPercent)
        {
            return discountPercent >= MIN_DISCOUNT && discountPercent <= MAX_DISCOUNT;
        }

        /// <summary>
        /// Updates round-trip discount for a specific trip
        /// </summary>
        public async Task<bool> UpdateRoundTripDiscountAsync(int tripId, decimal discountPercent)
        {
            if (!ValidateDiscount(discountPercent))
            {
                throw new ArgumentException(
                    $"Discount must be between {MIN_DISCOUNT}% and {MAX_DISCOUNT}%. Provided: {discountPercent}%",
                    nameof(discountPercent));
            }

            var trip = await _tripRepository.GetByIdAsync(tripId);
            if (trip == null)
            {
                return false;
            }

            trip.RoundTripDiscountPercent = discountPercent;
            trip.PriceLastUpdated = DateTime.UtcNow;

            await _tripRepository.UpdateAsync(trip);
            await _tripRepository.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Updates round-trip discount for all trips in a route (same company, from/to cities)
        /// </summary>
        public async Task<bool> UpdateRouteDiscountAsync(int companyId, string fromCity, string toCity, decimal discountPercent)
        {
            if (!ValidateDiscount(discountPercent))
            {
                throw new ArgumentException(
                    $"Discount must be between {MIN_DISCOUNT}% and {MAX_DISCOUNT}%. Provided: {discountPercent}%",
                    nameof(discountPercent));
            }

            if (string.IsNullOrWhiteSpace(fromCity))
                throw new ArgumentException("FromCity cannot be null or empty", nameof(fromCity));
            if (string.IsNullOrWhiteSpace(toCity))
                throw new ArgumentException("ToCity cannot be null or empty", nameof(toCity));

            // Find all trips for this route
            var trips = await _tripRepository.FindAsync(t => 
                t.CompanyId == companyId && 
                t.FromCity == fromCity && 
                t.ToCity == toCity);

            if (!trips.Any())
            {
                return false;
            }

            // Update all trips in the route
            var now = DateTime.UtcNow;
            foreach (var trip in trips)
            {
                trip.RoundTripDiscountPercent = discountPercent;
                trip.PriceLastUpdated = now;
                await _tripRepository.UpdateAsync(trip);
            }

            await _tripRepository.SaveChangesAsync();
            return true;
        }
    }
}

