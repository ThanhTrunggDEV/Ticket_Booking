using Ticket_Booking.Enums;

namespace Ticket_Booking.Models.DomainModels
{
    public class Trip
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string PlaneName { get; set; } = string.Empty;
        public string FromCity { get; set; } = string.Empty;
        public string ToCity { get; set; } = string.Empty;
        public decimal Distance { get; set; }
        public int EstimatedDuration { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public decimal Price { get; set; }
        public int AvailableSeats { get; set; }
        public TripStatus Status { get; set; } 

    
        public Company Company { get; set; } = null!;
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}
