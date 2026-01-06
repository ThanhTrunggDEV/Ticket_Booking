namespace Ticket_Booking.Models.DomainModels
{
    /// <summary>
    /// Tracks history of ticket changes for audit purposes
    /// </summary>
    public class TicketChangeHistory
    {
        public int Id { get; set; }
        public int OriginalTicketId { get; set; }
        public int NewTicketId { get; set; } // Reference to the newly created ticket
        public DateTime ChangeDate { get; set; }
        public decimal ChangeFee { get; set; }
        public decimal PriceDifference { get; set; } // NewPrice - OldPrice
        public decimal TotalAmountPaid { get; set; } // Amount paid by user (ChangeFee + positive PriceDifference)
        public string? ChangeReason { get; set; }
        public string? Status { get; set; } // e.g., "Completed", "Pending Payment", "Cancelled"

        // Navigation properties
        public Ticket OriginalTicket { get; set; } = null!;
        public Ticket NewTicket { get; set; } = null!;
    }
}

