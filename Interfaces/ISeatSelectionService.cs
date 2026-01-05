namespace Ticket_Booking.Interfaces
{
    /// <summary>
    /// Interface for seat selection and assignment during check-in
    /// </summary>
    public interface ISeatSelectionService
    {
        /// <summary>
        /// Assigns a seat to a ticket during check-in
        /// </summary>
        /// <param name="ticketId">The ticket ID</param>
        /// <param name="seatNumber">The seat number to assign (e.g., "12A")</param>
        /// <returns>True if seat was successfully assigned, false otherwise</returns>
        Task<bool> AssignSeatAsync(int ticketId, string seatNumber);

        /// <summary>
        /// Changes the seat for a ticket (if already assigned)
        /// </summary>
        /// <param name="ticketId">The ticket ID</param>
        /// <param name="newSeatNumber">The new seat number to assign</param>
        /// <returns>True if seat was successfully changed, false otherwise</returns>
        Task<bool> ChangeSeatAsync(int ticketId, string newSeatNumber);

        /// <summary>
        /// Validates if a seat is available for assignment
        /// </summary>
        /// <param name="tripId">The trip ID</param>
        /// <param name="seatNumber">The seat number to validate</param>
        /// <param name="seatClass">The seat class</param>
        /// <returns>True if seat is available, false otherwise</returns>
        bool ValidateSeatAvailability(int tripId, string seatNumber, Ticket_Booking.Enums.SeatClass seatClass);
    }
}


