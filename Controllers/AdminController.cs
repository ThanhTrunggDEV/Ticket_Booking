using Microsoft.AspNetCore.Mvc;

namespace Ticket_Booking.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
