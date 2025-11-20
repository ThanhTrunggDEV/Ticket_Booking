using Microsoft.AspNetCore.Mvc;
using Ticket_Booking.Interfaces;
using Ticket_Booking.Models.DomainModels;

namespace Ticket_Booking.Controllers
{
    public class UserController : Controller
    {
        private readonly IRepository<User> _userRepository;

        public UserController(IRepository<User> userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<IActionResult> Index()
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
    }
}
