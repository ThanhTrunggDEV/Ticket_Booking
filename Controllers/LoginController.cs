using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Ticket_Booking.Interfaces;
using Ticket_Booking.Models;
using Ticket_Booking.Models.DomainModels;

namespace Ticket_Booking.Controllers
{
    public class LoginController : Controller
    {
        private readonly ILogger<LoginController> _logger;
        private readonly IRepository<User> _userRepository;

        public LoginController(ILogger<LoginController> logger, IRepository<User> repository)
        {
            _logger = logger;
            _userRepository = repository;
        }

        public IActionResult Index()
        {
            return View();
        }

       

    
    }
}
