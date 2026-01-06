using Ticket_Booking.Models.DomainModels;

namespace Ticket_Booking.Interfaces
{
    /// <summary>
    /// Interface for generating boarding passes and sending them via email
    /// </summary>
    public interface IBoardingPassService
    {
        /// <summary>
        /// Generates a PDF boarding pass for a ticket
        /// </summary>
        /// <param name="ticket">The ticket to generate boarding pass for</param>
        /// <returns>Relative path to the generated boarding pass PDF file</returns>
        Task<string> GenerateBoardingPassAsync(Ticket ticket);

        /// <summary>
        /// Sends boarding pass via email to the ticket holder
        /// </summary>
        /// <param name="ticket">The ticket</param>
        /// <param name="boardingPassPath">Path to the boarding pass PDF file</param>
        /// <returns>Task representing the async operation</returns>
        Task SendBoardingPassEmailAsync(Ticket ticket, string boardingPassPath);
    }
}



