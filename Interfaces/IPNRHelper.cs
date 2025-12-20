namespace Ticket_Booking.Interfaces
{
    /// <summary>
    /// Interface for generating unique PNR (Passenger Name Record) codes
    /// </summary>
    public interface IPNRHelper
    {
        /// <summary>
        /// Generates a random 6-character PNR code
        /// </summary>
        /// <returns>6-character alphanumeric PNR code (uppercase)</returns>
        string GeneratePNR();

        /// <summary>
        /// Generates a unique PNR code by checking against existing tickets
        /// </summary>
        /// <param name="repository">Ticket repository to check for existing PNRs</param>
        /// <returns>Unique 6-character PNR code</returns>
        /// <exception cref="InvalidOperationException">Thrown if unable to generate unique PNR after retries</exception>
        Task<string> GenerateUniquePNRAsync(Repositories.TicketRepository repository);

        /// <summary>
        /// Validates if a string matches PNR format (6 alphanumeric characters)
        /// </summary>
        /// <param name="pnr">PNR string to validate</param>
        /// <returns>True if valid format, false otherwise</returns>
        bool IsValidPNRFormat(string? pnr);
    }
}

