using Ticket_Booking.Interfaces;
using Ticket_Booking.Repositories;

namespace Ticket_Booking.Helpers
{
    /// <summary>
    /// Helper class for generating unique PNR (Passenger Name Record) codes
    /// PNR format: 6 alphanumeric characters, excluding confusing characters (0, O, 1, I, L)
    /// </summary>
    public class PNRHelper : IPNRHelper
    {
        // Character set: A-Z, 2-9 (excludes 0, O, 1, I, L for clarity)
        private const string PNR_CHARS = "ABCDEFGHJKMNPQRSTUVWXYZ23456789";
        private const int PNR_LENGTH = 6;
        private const int MAX_RETRIES = 5;

        /// <summary>
        /// Generates a random 6-character PNR code
        /// </summary>
        /// <returns>6-character alphanumeric PNR code in uppercase</returns>
        public string GeneratePNR()
        {
            var random = new Random();
            var chars = new char[PNR_LENGTH];
            
            for (int i = 0; i < PNR_LENGTH; i++)
            {
                chars[i] = PNR_CHARS[random.Next(PNR_CHARS.Length)];
            }
            
            return new string(chars);
        }

        /// <summary>
        /// Generates a unique PNR code by checking against existing tickets
        /// Retries up to MAX_RETRIES times if collision occurs
        /// </summary>
        /// <param name="repository">Ticket repository to check for existing PNRs</param>
        /// <returns>Unique 6-character PNR code</returns>
        /// <exception cref="InvalidOperationException">Thrown if unable to generate unique PNR after MAX_RETRIES attempts</exception>
        public async Task<string> GenerateUniquePNRAsync(TicketRepository repository)
        {
            if (repository == null)
                throw new ArgumentNullException(nameof(repository));

            for (int attempt = 0; attempt < MAX_RETRIES; attempt++)
            {
                var pnr = GeneratePNR();
                
                // Check if PNR already exists (case-insensitive)
                var exists = await repository.PNRExistsAsync(pnr);
                
                if (!exists)
                {
                    return pnr;
                }
                
                // If collision, retry with new random PNR
                // Log collision for monitoring (optional)
            }

            // If all retries failed, throw exception
            throw new InvalidOperationException(
                $"Unable to generate unique PNR after {MAX_RETRIES} attempts. " +
                "This is extremely rare. Please try again or contact support.");
        }

        /// <summary>
        /// Validates if a string matches PNR format
        /// PNR must be exactly 6 alphanumeric characters (case-insensitive)
        /// </summary>
        /// <param name="pnr">PNR string to validate</param>
        /// <returns>True if valid format, false otherwise</returns>
        public bool IsValidPNRFormat(string? pnr)
        {
            if (string.IsNullOrWhiteSpace(pnr))
                return false;

            // Must be exactly 6 characters
            if (pnr.Length != PNR_LENGTH)
                return false;

            // Must contain only valid characters (case-insensitive)
            var upperPnr = pnr.ToUpper();
            foreach (char c in upperPnr)
            {
                if (!PNR_CHARS.Contains(c))
                    return false;
            }

            return true;
        }
    }
}

