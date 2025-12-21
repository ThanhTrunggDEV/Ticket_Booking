namespace Ticket_Booking.Interfaces
{
    /// <summary>
    /// Service for managing round-trip pricing and discounts
    /// </summary>
    public interface IPricingService
    {
        /// <summary>
        /// Gets round-trip discount percentage for a route
        /// </summary>
        /// <param name="companyId">The company/airline ID</param>
        /// <param name="fromCity">Origin city</param>
        /// <param name="toCity">Destination city</param>
        /// <returns>Discount percentage (0-50)</returns>
        decimal GetRoundTripDiscount(int companyId, string fromCity, string toCity);
        
        /// <summary>
        /// Validates discount percentage (must be 0-50%)
        /// </summary>
        /// <param name="discountPercent">The discount percentage to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        bool ValidateDiscount(decimal discountPercent);
        
        /// <summary>
        /// Updates round-trip discount for a specific trip
        /// </summary>
        /// <param name="tripId">The trip ID</param>
        /// <param name="discountPercent">The discount percentage (0-50)</param>
        /// <returns>True if successful, false if validation fails or trip not found</returns>
        /// <exception cref="ArgumentException">Thrown if discount is out of range</exception>
        Task<bool> UpdateRoundTripDiscountAsync(int tripId, decimal discountPercent);
        
        /// <summary>
        /// Updates round-trip discount for all trips in a route (same company, from/to cities)
        /// </summary>
        /// <param name="companyId">The company/airline ID</param>
        /// <param name="fromCity">Origin city</param>
        /// <param name="toCity">Destination city</param>
        /// <param name="discountPercent">The discount percentage (0-50)</param>
        /// <returns>True if successful, false if validation fails</returns>
        /// <exception cref="ArgumentException">Thrown if discount is out of range</exception>
        Task<bool> UpdateRouteDiscountAsync(int companyId, string fromCity, string toCity, decimal discountPercent);
    }
}

