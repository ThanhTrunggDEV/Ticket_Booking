using Ticket_Booking.Models.DomainModels;

namespace Ticket_Booking.Models.ViewModels
{
    /// <summary>
    /// View model for check-in page
    /// </summary>
    public class CheckInViewModel
    {
        public Ticket? Ticket { get; set; }
        public bool IsEligible { get; set; }
        public string? EligibilityMessage { get; set; }
        public DateTime? CheckInWindowStart { get; set; }  // 48 hours before departure
        public DateTime? CheckInWindowEnd { get; set; }  // 2 hours before departure
        public SeatMapViewModel? SeatMap { get; set; }
    }
}

