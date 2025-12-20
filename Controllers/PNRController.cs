using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Ticket_Booking.Interfaces;
using Ticket_Booking.Models.DomainModels;
using Ticket_Booking.Repositories;
using Ticket_Booking.Helpers;
using Ticket_Booking.Resources;

namespace Ticket_Booking.Controllers
{
    /// <summary>
    /// Controller for PNR (Passenger Name Record) lookup functionality
    /// Allows users to look up their bookings using PNR code and email without logging in
    /// </summary>
    public class PNRController : Controller
    {
        private readonly IRepository<Ticket> _ticketRepository;
        private readonly IPNRHelper _pnrHelper;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public PNRController(IRepository<Ticket> ticketRepository, IPNRHelper pnrHelper, IStringLocalizer<SharedResource> localizer)
        {
            _ticketRepository = ticketRepository;
            _pnrHelper = pnrHelper;
            _localizer = localizer;
        }

        /// <summary>
        /// Displays the PNR lookup form
        /// </summary>
        /// <returns>Lookup form view</returns>
        [HttpGet]
        public IActionResult Lookup()
        {
            return View();
        }

        /// <summary>
        /// Processes PNR lookup request
        /// Validates PNR format and email, then retrieves ticket if found
        /// </summary>
        /// <param name="pnr">PNR code (6 alphanumeric characters)</param>
        /// <param name="email">Email address associated with the booking</param>
        /// <returns>Ticket details view if found, lookup form with error if not found</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Lookup(string pnr, string email)
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(pnr))
            {
                ModelState.AddModelError(nameof(pnr), _localizer["PNR.PNRCode"] + " is required.");
                ViewData["pnr"] = pnr;
                ViewData["email"] = email;
                return View();
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError(nameof(email), _localizer["PNR.EmailAddress"] + " is required.");
                ViewData["pnr"] = pnr;
                ViewData["email"] = email;
                return View();
            }

            // Validate PNR format
            if (!_pnrHelper.IsValidPNRFormat(pnr))
            {
                ModelState.AddModelError(nameof(pnr), "Invalid PNR format. PNR must be 6 alphanumeric characters.");
                ViewData["pnr"] = pnr;
                ViewData["email"] = email;
                return View();
            }

            // Validate email format (basic validation)
            if (!email.Contains("@") || !email.Contains("."))
            {
                ModelState.AddModelError(nameof(email), "Invalid email format.");
                ViewData["pnr"] = pnr;
                ViewData["email"] = email;
                return View();
            }

            try
            {
                // Get ticket by PNR and email (case-insensitive)
                var ticketRepository = (TicketRepository)_ticketRepository;
                var ticket = await ticketRepository.GetByPNRAndEmailAsync(pnr, email);

                if (ticket == null)
                {
                    // Don't reveal whether PNR or email is wrong (security best practice)
                    ModelState.AddModelError(string.Empty, _localizer["PNR.NotFound"]);
                    ViewData["pnr"] = pnr;
                    ViewData["email"] = email;
                    return View();
                }

                // Ticket found - pass to view
                return View("LookupResult", ticket);
            }
            catch (Exception)
            {
                // Log error (in production, use ILogger)
                ModelState.AddModelError(string.Empty, "An error occurred while looking up your booking. Please try again later.");
                ViewData["pnr"] = pnr;
                ViewData["email"] = email;
                return View();
            }
        }
    }
}

