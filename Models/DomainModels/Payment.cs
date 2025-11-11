using Ticket_Booking.Enums;

namespace Ticket_Booking.Models.DomainModels
{
    public class Payment
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public PaymentMethod Method { get; set; } 
        public string TransactionCode { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public PaymentStatus Status { get; set; }  

      
        public Ticket Ticket { get; set; } = null!;
    }
}
