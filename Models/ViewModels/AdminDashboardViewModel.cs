namespace Ticket_Booking.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalCompanies { get; set; }
        public int TotalTrips { get; set; }
        public int TotalTickets { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
