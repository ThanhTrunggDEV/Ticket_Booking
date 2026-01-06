using Ticket_Booking.Models.DomainModels;

namespace Ticket_Booking.Models.ViewModels
{
    public class TicketChangePaymentSuccessViewModel
    {
        public Ticket OriginalTicket { get; set; } = null!;
        public Ticket NewTicket { get; set; } = null!;
        public Trip OriginalTrip { get; set; } = null!;
        public Trip NewTrip { get; set; } = null!;
        public decimal ChangeFee { get; set; }
        public decimal PriceDifference { get; set; }
        public decimal TotalAmountPaid { get; set; }
        public string? ChangeReason { get; set; }
    }
}

