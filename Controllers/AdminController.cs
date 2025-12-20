using Microsoft.AspNetCore.Mvc;
using Ticket_Booking.Enums;
using Ticket_Booking.Interfaces;
using Ticket_Booking.Models.DomainModels;
using Ticket_Booking.Models.ViewModels;
using Ticket_Booking.Repositories;

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
        public async Task<IActionResult> ChangeRole(int id, Role role)
        {
            var currentRole = HttpContext.Session.GetString("UserRole");
            if (currentRole != "Admin")
            {
                return RedirectToAction("Index", "Login");
            }

            var user = await _userRepository.GetByIdAsync(id);
            if (user != null)
            {
                user.Role = role;
                await _userRepository.UpdateAsync(user);
                await _userRepository.SaveChangesAsync();
            }

            return RedirectToAction(nameof(UserManagement));
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

        public async Task<IActionResult> PartnerManagement()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin")
            {
                return RedirectToAction("Index", "Login");
            }

            var companies = await _companyRepository.GetAllAsync();
            return View(companies);
        }

        [HttpPost]
        public async Task<IActionResult> DeletePartner(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin")
            {
                return RedirectToAction("Index", "Login");
            }

            await _companyRepository.DeleteByIdAsync(id);
            await _companyRepository.SaveChangesAsync();

            return RedirectToAction(nameof(PartnerManagement));
        }

        public async Task<IActionResult> TripManagement()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin")
            {
                return RedirectToAction("Index", "Login");
            }

            var trips = await _tripRepository.GetAllAsync();
            return View(trips);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTrip(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin")
            {
                return RedirectToAction("Index", "Login");
            }

            await _tripRepository.DeleteByIdAsync(id);
            await _tripRepository.SaveChangesAsync();

            return RedirectToAction(nameof(TripManagement));
        }

        /// <summary>
        /// Search ticket by PNR code (Admin only)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> SearchTicketByPNR(string pnr)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin")
            {
                return RedirectToAction("Index", "Login");
            }

            if (string.IsNullOrWhiteSpace(pnr))
            {
                return RedirectToAction("Index");
            }

            var ticketRepository = (TicketRepository)_ticketRepository;
            var ticket = await ticketRepository.GetByPNRAsync(pnr);

            if (ticket == null)
            {
                TempData["Error"] = $"No ticket found with PNR: {pnr}";
                return RedirectToAction("Index");
            }

            // Redirect to ticket detail view (if exists) or show in a view
            return RedirectToAction("Ticket", "User", new { id = ticket.Id });
        }
    }
}
