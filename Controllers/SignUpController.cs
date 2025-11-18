
using Microsoft.AspNetCore.Mvc;
using Ticket_Booking.Interfaces;
using Ticket_Booking.Models.BindingModels;
using Ticket_Booking.Models.DomainModels;
using Ticket_Booking.Services;

namespace Ticket_Booking.Controllers
{
    public class SignUpController : Controller
    {
        private readonly ILogger<SignUpController> _logger;
        private readonly IRepository<User> _userRepository;

        public SignUpController(ILogger<SignUpController> logger, IRepository<User> repository)
        {
            _logger = logger;
            _userRepository = repository;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        [Route("SignUp")]
        public IActionResult Index(UserRegister student)
        {
          //  _logger.LogInformation(student.Password + " " + student.ConfirmPassword);
            if(_userRepository.ExistsAsync(u => u.Email == student.Email).Result)
            {
                ModelState.AddModelError("Email", "Email is already registered.");
            }


            if(student.Password != student.ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "Confirm Password Doesn't Match");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Validation errors occurred during user registration.");
                var errors = ModelState.ToDictionary(
                    key => key.Key,
                    value => value.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );

                return BadRequest(errors);  
            }

            
            
                var user = new User
                {
                    FullName = student.FullName,
                    Email = student.Email,
                    PasswordHash = AuthenticationService.HashPassword(student.Password),
                    Phone = student.Phone
                };
                _userRepository.AddAsync(user);
                _userRepository.SaveChangesAsync();

            return Ok(new { success = true });

        }
    }
}
