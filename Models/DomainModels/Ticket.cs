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
        public DateTime BookingDate { get; set; }
        public PaymentStatus PaymentStatus { get; set; } 
        public string? QrCode { get; set; }
        public string? PNR { get; set; }  // Passenger Name Record - 6-character booking code
        public bool IsCheckedIn { get; set; }  // Online check-in status
        public DateTime? CheckInTime { get; set; }  // UTC timestamp when check-in occurred
        public string? BoardingPassUrl { get; set; }  // Relative path to boarding pass PDF (e.g., boarding-passes/ABC123/boarding-pass-ABC123-20241220120000.pdf)
        public decimal TotalPrice { get; set; }

     
        public Trip Trip { get; set; } = null!;
        public User User { get; set; } = null!;
        public Payment? Payment { get; set; }
    }
}
