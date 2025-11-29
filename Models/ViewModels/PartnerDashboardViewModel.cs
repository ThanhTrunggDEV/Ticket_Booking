using Ticket_Booking.Models.DomainModels;

namespace Ticket_Booking.Models.ViewModels
{
    public class PartnerDashboardViewModel
    {
        public int TotalCompanies { get; set; }
        public int TotalTrips { get; set; }
        public int TotalBookings { get; set; }
        public decimal TotalRevenue { get; set; }
        public double AverageRating { get; set; }
        public List<Trip> RecentTrips { get; set; } = new List<Trip>();
        public List<Ticket> RecentBookings { get; set; } = new List<Ticket>();
    }
}
