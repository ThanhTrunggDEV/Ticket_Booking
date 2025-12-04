using Microsoft.AspNetCore.Mvc;
using Ticket_Booking.Interfaces;
using Ticket_Booking.Models.BindingModels;
using Ticket_Booking.Models.DomainModels;
using Ticket_Booking.Models.ViewModels;

namespace Ticket_Booking.Controllers
{
    public class UserController : Controller
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Ticket> _ticketRepository;
        private readonly IRepository<Trip> _tripRepository;

        public UserController(IRepository<User> userRepository, IRepository<Ticket> ticketRepository, IRepository<Trip> tripRepository)
        {
            _userRepository = userRepository;
            _ticketRepository = ticketRepository;
            _tripRepository = tripRepository;
        }

        public async Task<IActionResult> Index(string? fromCity, string? toCity, DateTime? date)
        {
            var trips = await _tripRepository.GetAllAsync();
            
            var cities = trips.Select(t => t.FromCity)
                .Union(trips.Select(t => t.ToCity))
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            if (!string.IsNullOrEmpty(fromCity))
            {
                trips = trips.Where(t => t.FromCity == fromCity);
            }

            if (!string.IsNullOrEmpty(toCity))
            {
                trips = trips.Where(t => t.ToCity == toCity);
            }

            if (date.HasValue)
            {
                trips = trips.Where(t => t.DepartureTime.Date == date.Value.Date);
            }

            trips = trips.Where(t => t.DepartureTime > DateTime.Now);

            var viewModel = new SearchTripViewModel
            {
                FromCity = fromCity,
                ToCity = toCity,
                Date = date,
                Trips = trips,
                AvailableCities = cities
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var user = await _userRepository.GetByIdAsync(userId.Value);
            if (user == null)
            {
                return RedirectToAction("Index", "Login");
            }

            return View(user);
        }

        public async Task<IActionResult> MyBooking()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var ticketRepo = _ticketRepository as Ticket_Booking.Repositories.TicketRepository;
            if (ticketRepo != null)
            {
                var tickets = await ticketRepo.GetByUserAsync(userId.Value);
                return View(tickets);
            }
            
            // Fallback if casting fails (should not happen)
            var allTickets = await _ticketRepository.FindAsync(t => t.UserId == userId.Value);
            return View(allTickets);
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var user = await _userRepository.GetByIdAsync(userId.Value);
            if (user == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var model = new UserEditProfile
            {
                FullName = user.FullName,
                Phone = user.Phone,
                Email = user.Email
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditProfile(UserEditProfile model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Index", "Login");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userRepository.GetByIdAsync(userId.Value);
            if (user == null)
            {
                return RedirectToAction("Index", "Login");
            }

            user.FullName = model.FullName;
            user.Phone = model.Phone;
            
            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();

            return RedirectToAction("Profile");
        }
    }
}
