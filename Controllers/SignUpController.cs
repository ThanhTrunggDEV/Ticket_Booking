using Microsoft.AspNetCore.Mvc;

namespace Ticket_Booking.Controllers
{
    public class SignUpController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
