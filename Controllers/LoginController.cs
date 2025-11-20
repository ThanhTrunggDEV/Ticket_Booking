using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Ticket_Booking.Helpers;
using Ticket_Booking.Interfaces;
using Ticket_Booking.Models;
using Ticket_Booking.Models.BindingModels;
using Ticket_Booking.Models.DomainModels;
using Ticket_Booking.Services;

namespace Ticket_Booking.Controllers
{
    public class LoginController : Controller
    {
        private readonly ILogger<LoginController> _logger;
        private readonly IRepository<User> _userRepository;
        private readonly MailService _mailService;

        public LoginController(ILogger<LoginController> logger, IRepository<User> repository, MailService mailService)
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
        public IActionResult Index(UserLogin userLogin)
        {
            var user = _userRepository.GetByEmailAsync(userLogin.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }
            else
            {
                User u = user.Result;
                if (AuthenticationService.VerifyPassword(userLogin.Password, u.PasswordHash) == false)
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                }

            }
            return RedirectToAction("Index", "Home");

        }

        [HttpGet]
        [Route("/ForgotPassword")]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [Route("Login/SendForgotPasswordOtp")]
        public async Task<IActionResult> SendForgotPasswordOtp(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
            {
                return BadRequest("Email not found.");
            }

            string otp = OTP.GenerateOTP();
            HttpContext.Session.SetString("Forgot_Email", email);
            HttpContext.Session.SetString("Forgot_OTP", otp);
            HttpContext.Session.SetString("Forgot_OTP_Expiry", DateTime.Now.AddMinutes(5).ToString());

            try
            {
                string body = $"Your OTP for Password Reset is: <b>{otp}</b>. It expires in 5 minutes.";
                _mailService.SendEmail(email, "Ticket Booking - Reset Password", body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email");
                return StatusCode(500, "Failed to send email");
            }

            return Ok();
        }

        [HttpPost]
        [Route("Login/VerifyForgotPasswordOtp")]
        public IActionResult VerifyForgotPasswordOtp(string email, string otp)
        {
            var storedEmail = HttpContext.Session.GetString("Forgot_Email");
            var storedOtp = HttpContext.Session.GetString("Forgot_OTP");
            var expiryStr = HttpContext.Session.GetString("Forgot_OTP_Expiry");

            if (string.IsNullOrEmpty(storedOtp) || storedEmail != email)
            {
                return BadRequest("Invalid session or email.");
            }

            if (DateTime.Parse(expiryStr) < DateTime.Now)
            {
                return BadRequest("OTP expired.");
            }

            if (storedOtp != otp)
            {
                return BadRequest("Invalid OTP.");
            }

            // Mark OTP as verified in session to allow password reset
            HttpContext.Session.SetString("Forgot_Verified", "true");

            return Ok();
        }

        [HttpPost]
        [Route("Login/ResetPassword")]
        public async Task<IActionResult> ResetPassword(string email, string newPassword)
        {
            var storedEmail = HttpContext.Session.GetString("Forgot_Email");
            var isVerified = HttpContext.Session.GetString("Forgot_Verified");

            if (storedEmail != email || isVerified != "true")
            {
                return BadRequest("Unauthorized request.");
            }

            var user = await _userRepository.GetByEmailAsync(email);
            if (user != null)
            {
                user.PasswordHash = AuthenticationService.HashPassword(newPassword);
                await _userRepository.UpdateAsync(user);
            }

            // Clear session
            HttpContext.Session.Remove("Forgot_Email");
            HttpContext.Session.Remove("Forgot_OTP");
            HttpContext.Session.Remove("Forgot_OTP_Expiry");
            HttpContext.Session.Remove("Forgot_Verified");

            return Ok();
        }

    }
}

  
       

    
    

