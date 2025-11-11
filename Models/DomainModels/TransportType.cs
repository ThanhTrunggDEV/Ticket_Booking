namespace Ticket_Booking.Models.DomainModels
{
    public class TransportType
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

  
        public ICollection<Company> Companies { get; set; } = new List<Company>();
        public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    }
}
