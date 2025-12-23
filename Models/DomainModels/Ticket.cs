using Ticket_Booking.Enums;

namespace Ticket_Booking.Models.DomainModels
{
    public class Ticket
    {
        public int Id { get; set; }
        public int TripId { get; set; }
        public int UserId { get; set; }
        public SeatClass SeatClass { get; set; }
        public string SeatNumber { get; set; } = string.Empty;
        public string PassengerName { get; set; } = string.Empty;  // Passenger name on ticket (can be different from User.FullName)
        public DateTime BookingDate { get; set; }
        public PaymentStatus PaymentStatus { get; set; } 
        public string? QrCode { get; set; }
        public string? PNR { get; set; }  // Passenger Name Record - 6-character booking code
        public bool IsCheckedIn { get; set; }  // Online check-in status
        public DateTime? CheckInTime { get; set; }  // UTC timestamp when check-in occurred
        public string? BoardingPassUrl { get; set; }  // Relative path to boarding pass PDF (e.g., boarding-passes/ABC123/boarding-pass-ABC123-20241220120000.pdf)
        public decimal TotalPrice { get; set; }
        
        // Cancellation fields
        public bool IsCancelled { get; set; }  // Whether the ticket has been cancelled
        public DateTime? CancelledAt { get; set; }  // UTC timestamp when cancellation occurred
        public string? CancellationReason { get; set; }  // Optional reason for cancellation
        
        // Round-trip booking fields
        public TicketType Type { get; set; } = TicketType.OneWay;  // OneWay or RoundTrip
        public int? OutboundTicketId { get; set; }  // For return ticket: link to outbound ticket
        public int? ReturnTicketId { get; set; }  // For outbound ticket: link to return ticket
        public int? BookingGroupId { get; set; }  // Groups tickets in same round-trip booking

     
        public Trip Trip { get; set; } = null!;
        public User User { get; set; } = null!;
        public Payment? Payment { get; set; }
        
        // Navigation properties for round-trip linking
        public Ticket? OutboundTicket { get; set; }  // The outbound ticket (if this is a return ticket)
        public Ticket? ReturnTicket { get; set; }  // The return ticket (if this is an outbound ticket)
    }
}
