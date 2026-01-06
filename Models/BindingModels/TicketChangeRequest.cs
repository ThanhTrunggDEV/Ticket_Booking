using Ticket_Booking.Enums;

namespace Ticket_Booking.Models.BindingModels
{
    /// <summary>
    /// Binding model for ticket change request stored in session
    /// </summary>
    public class TicketChangeRequest
    {
        public int NewTripId { get; set; }
        public SeatClass NewSeatClass { get; set; }
        public decimal ChangeFee { get; set; }
        public decimal PriceDifference { get; set; }
        public decimal TotalDue { get; set; }
        public decimal RefundAmount { get; set; }
        public string? ChangeReason { get; set; }
    }
}

