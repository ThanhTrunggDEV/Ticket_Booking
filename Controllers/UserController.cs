using Microsoft.AspNetCore.Mvc;

namespace Ticket_Booking.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
