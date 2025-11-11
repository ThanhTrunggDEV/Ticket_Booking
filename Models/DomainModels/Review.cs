namespace Ticket_Booking.Models.DomainModels
{
    public class Review
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int CompanyId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }

        
        public User User { get; set; } = null!;
        public Company Company { get; set; } = null!;
    }
}
