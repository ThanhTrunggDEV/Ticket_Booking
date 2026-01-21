using Ticket_Booking.Enums;
using Ticket_Booking.Models.ViewModels;

namespace Ticket_Booking.Interfaces
{
    /// <summary>
    /// Interface for generating seat maps and checking seat availability
    /// </summary>
    public interface ISeatMapService
    {
        /// <summary>
        /// Generates a seat map view model for a specific trip and seat class
        /// </summary>
        /// <param name="tripId">The trip ID</param>
        /// <param name="seatClass">The seat class (Economy, Business, FirstClass)</param>
        /// <returns>SeatMapViewModel containing seat information and availability</returns>
        SeatMapViewModel GetSeatMap(int tripId, SeatClass seatClass);

        /// <summary>
        /// Checks if a specific seat is available for a trip
        /// </summary>
        /// <param name="tripId">The trip ID</param>
        /// <param name="seatNumber">The seat number (e.g., "12A")</param>
        /// <returns>True if seat is available, false otherwise</returns>
        bool IsSeatAvailable(int tripId, string seatNumber);

        /// <summary>
        /// Gets a list of all available seat numbers for a trip and seat class
        /// </summary>
        /// <param name="tripId">The trip ID</param>
        /// <param name="seatClass">The seat class</param>
        /// <returns>List of available seat numbers</returns>
        List<string> GetAvailableSeats(int tripId, SeatClass seatClass);
    }
}




