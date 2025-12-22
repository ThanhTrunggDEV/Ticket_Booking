using Ticket_Booking.Models.DomainModels;

namespace Ticket_Booking.Models.ViewModels
{
    public class PaymentSuccessViewModel
    {
        public Ticket PrimaryTicket { get; set; } = null!;
        public List<Ticket> Tickets { get; set; } = new();
    }
}


