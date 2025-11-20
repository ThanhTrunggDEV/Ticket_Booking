
using Microsoft.AspNetCore.Mvc;
using Ticket_Booking.Helpers;
using Ticket_Booking.Interfaces;
using Ticket_Booking.Models.BindingModels;
using Ticket_Booking.Models.DomainModels;
using Ticket_Booking.Services;
using System.Text.Json;

namespace Ticket_Booking.Controllers
{
    public class SignUpController : Controller
    {
        private readonly ILogger<SignUpController> _logger;
        private readonly IRepository<User> _userRepository;
        private readonly MailService _mailService;

        public SignUpController(ILogger<SignUpController> logger, IRepository<User> repository, MailService mailService)
        {
            _logger = logger;
            _userRepository = repository;
            _mailService = mailService;
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

            string otp = OTP.GenerateOTP();
            
            HttpContext.Session.SetString("UserRegister", JsonSerializer.Serialize(student));
            HttpContext.Session.SetString("OTP", otp);
            HttpContext.Session.SetString("OTP_Expiry", DateTime.Now.AddMinutes(5).ToString());

            try 
            {
                string body = $"Your OTP for Ticket Booking registration is: <b>{otp}</b>. It expires in 5 minutes.";
                _mailService.SendEmail(student.Email, "Ticket Booking - Verify Email", body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email");
            }

            return Ok(new { redirectUrl = Url.Action("Otp", "SignUp") });
        }

        [Route("SignUp/Otp")]
        [HttpGet]
        public IActionResult Otp()
        {
            var userJson = HttpContext.Session.GetString("UserRegister");
            if (string.IsNullOrEmpty(userJson))
            {
                return RedirectToAction("Index");
            }
            return View();
        }

        [Route("SignUp/VerifyOtp")]
        [HttpPost]
        public async Task<IActionResult> VerifyOtp(string otp)
        {
            var storedOtp = HttpContext.Session.GetString("OTP");
            var expiryStr = HttpContext.Session.GetString("OTP_Expiry");
            var userJson = HttpContext.Session.GetString("UserRegister");

            if (string.IsNullOrEmpty(storedOtp) || string.IsNullOrEmpty(userJson))
            {
                return BadRequest("Session expired. Please sign up again.");
            }

            if (DateTime.Parse(expiryStr) < DateTime.Now)
            {
                return BadRequest("OTP expired. Please request a new one.");
            }

            if (storedOtp != otp)
            {
                return BadRequest("Invalid OTP.");
            }

            var userRegister = JsonSerializer.Deserialize<UserRegister>(userJson);
            var user = new User
            {
                FullName = userRegister.FullName,
                Email = userRegister.Email,
                PasswordHash = AuthenticationService.HashPassword(userRegister.Password),
                Phone = userRegister.Phone,
                Role = Enums.Role.User,
                CreatedAt = DateTime.Now
            };

            await _userRepository.AddAsync(user);

            HttpContext.Session.Remove("UserRegister");
            HttpContext.Session.Remove("OTP");
            HttpContext.Session.Remove("OTP_Expiry");

            return Ok();
        }

        [Route("SignUp/ResendOtp")]
        [HttpPost]
        public IActionResult ResendOtp()
        {
            var userJson = HttpContext.Session.GetString("UserRegister");
            if (string.IsNullOrEmpty(userJson))
            {
                return BadRequest("Session expired.");
            }

            var userRegister = JsonSerializer.Deserialize<UserRegister>(userJson);
            string otp = OTP.GenerateOTP();
            
            HttpContext.Session.SetString("OTP", otp);
            HttpContext.Session.SetString("OTP_Expiry", DateTime.Now.AddMinutes(5).ToString());

            try 
            {
                string body = $"Your new OTP for Ticket Booking registration is: <b>{otp}</b>. It expires in 5 minutes.";
                _mailService.SendEmail(userRegister.Email, "Ticket Booking - Resend OTP", body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email");
                return StatusCode(500, "Failed to send email");
            }

            return Ok();
        }
    }
}
