using Microsoft.AspNetCore.Mvc;
using Ticket_Booking.Interfaces;
using Ticket_Booking.Models.DomainModels;
using Ticket_Booking.Models.ViewModels;

namespace Ticket_Booking.Controllers
{
    public class AdminController : Controller
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Company> _companyRepository;
        private readonly IRepository<Trip> _tripRepository;
        private readonly IRepository<Ticket> _ticketRepository;

        public AdminController(
            IRepository<User> userRepository,
            IRepository<Company> companyRepository,
            IRepository<Trip> tripRepository,
            IRepository<Ticket> ticketRepository)
        {
            _userRepository = userRepository;
            _companyRepository = companyRepository;
            _tripRepository = tripRepository;
            _ticketRepository = ticketRepository;
        }

        public async Task<IActionResult> Index()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin")
            {
                return RedirectToAction("Index", "Login");
            }

            var viewModel = new AdminDashboardViewModel
            {
                TotalUsers = await _userRepository.CountAsync(),
                TotalCompanies = await _companyRepository.CountAsync(),
                TotalTrips = await _tripRepository.CountAsync(),
                TotalTickets = await _ticketRepository.CountAsync()
            };

            var tickets = await _ticketRepository.GetAllAsync();
            viewModel.TotalRevenue = tickets.Sum(t => t.TotalPrice);

            return View(viewModel);
        }

        public async Task<IActionResult> UserManagement()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin")
            {
                return RedirectToAction("Index", "Login");
            }

            var users = await _userRepository.GetAllAsync();
            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin")
            {
                return RedirectToAction("Index", "Login");
            }

            await _userRepository.DeleteByIdAsync(id);
            await _userRepository.SaveChangesAsync();

            return RedirectToAction(nameof(UserManagement));
        }
    }
}
