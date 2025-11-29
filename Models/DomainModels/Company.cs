namespace Ticket_Booking.Models.DomainModels
{
    public class Company
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Contact { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public int? OwnerId { get; set; }

       
        public User? Owner { get; set; }
        public ICollection<Trip> Trips { get; set; } = new List<Trip>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
