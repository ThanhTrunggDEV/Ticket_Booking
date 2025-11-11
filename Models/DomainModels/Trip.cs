using Ticket_Booking.Enums;

namespace Ticket_Booking.Models.DomainModels
{
    public class Trip
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public int RouteId { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public decimal Price { get; set; }
        public int AvailableSeats { get; set; }
        public TripStatus Status { get; set; } 

    
        public Vehicle Vehicle { get; set; } = null!;
        public Route Route { get; set; } = null!;
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}
