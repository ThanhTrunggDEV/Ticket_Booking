using Microsoft.AspNetCore.Mvc;
using Ticket_Booking.Interfaces;
using Ticket_Booking.Models.DomainModels;
using Ticket_Booking.Models.ViewModels;

namespace Ticket_Booking.Controllers
{
    public class PartnerController : Controller
    {
        private readonly IRepository<Trip> _tripRepository;
        private readonly IRepository<Ticket> _ticketRepository;
        private readonly IRepository<Company> _companyRepository;

        public PartnerController(
            IRepository<Trip> tripRepository,
            IRepository<Ticket> ticketRepository,
            IRepository<Company> companyRepository)
        {
            _tripRepository = tripRepository;
            _ticketRepository = ticketRepository;
            _companyRepository = companyRepository;
        }

        public async Task<IActionResult> Index()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Partner")
            {
                return RedirectToAction("Index", "Login");
            }

            // TODO: Filter data by the logged-in Partner's ID.
            // Currently, there is no link between User (Partner) and Company/Trips in the database schema.
            // Displaying all data for demonstration purposes.

            var trips = await _tripRepository.GetAllAsync();
            var tickets = await _ticketRepository.GetAllAsync();
            
            var viewModel = new PartnerDashboardViewModel
            {
                TotalTrips = trips.Count(),
                TotalBookings = tickets.Count(),
                TotalRevenue = tickets.Sum(t => t.TotalPrice),
                AverageRating = 4.5, // Placeholder
                RecentTrips = trips.OrderByDescending(t => t.DepartureTime).Take(5).ToList(),
                RecentBookings = tickets.OrderByDescending(t => t.BookingDate).Take(5).ToList()
            };

            return View(viewModel);
        }
    }
}
