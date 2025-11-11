namespace Ticket_Booking.Models.DomainModels
{
    public class Company
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int TransportTypeId { get; set; }
        public string Contact { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }

       
        public TransportType TransportType { get; set; } = null!;
        public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
