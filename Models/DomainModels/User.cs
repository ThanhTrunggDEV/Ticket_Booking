using Ticket_Booking.Enums;

namespace Ticket_Booking.Models.DomainModels
{
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public Role Role { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

       
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
