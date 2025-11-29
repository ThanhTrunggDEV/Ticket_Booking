namespace Ticket_Booking.Models.DomainModels
{
    public class Vehicle
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string VehicleName { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public int TransportTypeId { get; set; }

      
        public Company Company { get; set; } = null!;
        public TransportType TransportType { get; set; } = null!;
        public ICollection<Trip> Trips { get; set; } = new List<Trip>();
    }
}
