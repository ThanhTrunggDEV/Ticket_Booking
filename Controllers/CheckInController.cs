using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Ticket_Booking.Enums;
using Ticket_Booking.Interfaces;
using Ticket_Booking.Models.DomainModels;
using Ticket_Booking.Models.ViewModels;
using Ticket_Booking.Repositories;
using Ticket_Booking.Resources;
using Ticket_Booking.Helpers;
using Microsoft.AspNetCore.Http;

namespace Ticket_Booking.Controllers
{
    /// <summary>
    /// Controller for online check-in functionality
    /// Supports both PNR lookup (public) and logged-in user access
    /// </summary>
    public class CheckInController : Controller
    {
        private readonly IRepository<Ticket> _ticketRepository;
        private readonly ISeatMapService _seatMapService;
        private readonly ISeatSelectionService _seatSelectionService;
        private readonly IBoardingPassService _boardingPassService;
        private readonly IPNRHelper _pnrHelper;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public CheckInController(
            IRepository<Ticket> ticketRepository,
            ISeatMapService seatMapService,
            ISeatSelectionService seatSelectionService,
            IBoardingPassService boardingPassService,
            IPNRHelper pnrHelper,
            IStringLocalizer<SharedResource> localizer)
        {
            _ticketRepository = ticketRepository;
            _seatMapService = seatMapService;
            _seatSelectionService = seatSelectionService;
            _boardingPassService = boardingPassService;
            _pnrHelper = pnrHelper;
            _localizer = localizer;
        }

        /// <summary>
        /// Displays check-in form (public access via PNR)
        /// </summary>
        /// <param name="pnr">PNR code</param>
        /// <param name="email">Email address</param>
        /// <returns>Check-in view with eligibility status</returns>
        [HttpGet]
        public async Task<IActionResult> Index(string? pnr, string? email)
        {
            // If PNR and email provided, validate and get ticket
            if (!string.IsNullOrWhiteSpace(pnr) && !string.IsNullOrWhiteSpace(email))
            {
                if (!_pnrHelper.IsValidPNRFormat(pnr))
                {
                    ModelState.AddModelError(string.Empty, "Invalid PNR format.");
                    ViewData["pnr"] = pnr;
                    ViewData["email"] = email;
                    return View();
                }

                var ticketRepository = (TicketRepository)_ticketRepository;
                var ticket = await ticketRepository.GetByPNRAndEmailAsync(pnr, email);

                if (ticket == null)
                {
                    ModelState.AddModelError(string.Empty, "Ticket not found. Please verify your PNR and email.");
                    ViewData["pnr"] = pnr;
                    ViewData["email"] = email;
                    return View();
                }

                return await ShowCheckInView(ticket);
            }

            // Show form for PNR/email input
            return View();
        }

        /// <summary>
        /// Processes PNR lookup for check-in (form submission)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Index")]
        public async Task<IActionResult> IndexPost(string pnr, string email)
        {
            if (string.IsNullOrWhiteSpace(pnr) || string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError(string.Empty, "PNR and email are required.");
                ViewData["pnr"] = pnr;
                ViewData["email"] = email;
                return View();
            }

            if (!_pnrHelper.IsValidPNRFormat(pnr))
            {
                ModelState.AddModelError(string.Empty, "Invalid PNR format. PNR must be 6 alphanumeric characters.");
                ViewData["pnr"] = pnr;
                ViewData["email"] = email;
                return View();
            }

            var ticketRepository = (TicketRepository)_ticketRepository;
            var ticket = await ticketRepository.GetByPNRAndEmailAsync(pnr, email);

            if (ticket == null)
            {
                ModelState.AddModelError(string.Empty, "Ticket not found. Please verify your PNR and email.");
                ViewData["pnr"] = pnr;
                ViewData["email"] = email;
                return View();
            }

            return await ShowCheckInView(ticket);
        }

        /// <summary>
        /// Displays user's tickets eligible for check-in (logged-in users)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> MyTickets()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToAction("Index", "Login");
            }

            var ticketRepository = (TicketRepository)_ticketRepository;
            var eligibleTickets = await ticketRepository.GetEligibleTicketsForCheckInAsync(userId.Value);

            return View(eligibleTickets);
        }

        /// <summary>
        /// Displays seat map for a ticket
        /// </summary>
        /// <param name="ticketId">Ticket ID</param>
        /// <returns>Seat map view</returns>
        [HttpGet]
        public async Task<IActionResult> SeatMap(int ticketId)
        {
            var ticket = await ValidateTicketAccess(ticketId);
            if (ticket == null)
            {
                return RedirectToAction("Index");
            }

            // Ensure ticket has related data loaded
            var ticketRepository = (TicketRepository)_ticketRepository;
            ticket = await ticketRepository.GetCompleteAsync(ticketId);
            if (ticket == null || ticket.Trip == null)
            {
                TempData["Error"] = "Ticket not found.";
                return RedirectToAction("Index");
            }

            var seatMap = _seatMapService.GetSeatMap(ticket.TripId, ticket.SeatClass);

            var viewModel = new CheckInViewModel
            {
                Ticket = ticket,
                SeatMap = seatMap,
                IsEligible = await ticketRepository.IsEligibleForCheckInAsync(ticketId)
            };

            return View(viewModel);
        }

        /// <summary>
        /// Processes check-in request
        /// </summary>
        /// <param name="ticketId">Ticket ID</param>
        /// <param name="seatNumber">Optional seat number (if not provided, uses existing seat)</param>
        /// <returns>Confirmation view or error</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckIn(int ticketId, string? seatNumber)
        {
            var ticket = await ValidateTicketAccess(ticketId);
            if (ticket == null)
            {
                return RedirectToAction("Index");
            }

            var ticketRepository = (TicketRepository)_ticketRepository;

            // Validate eligibility
            if (!await ticketRepository.IsEligibleForCheckInAsync(ticketId))
            {
                TempData["Error"] = "This ticket is not eligible for check-in. Please check payment status and check-in window.";
                return RedirectToAction("Index");
            }

            // Ensure ticket has related data loaded
            ticket = await ticketRepository.GetCompleteAsync(ticketId);
            if (ticket == null || ticket.Trip == null || ticket.User == null)
            {
                TempData["Error"] = "Ticket data not found.";
                return RedirectToAction("Index");
            }

            // If seat number provided and different from current, assign new seat
            if (!string.IsNullOrWhiteSpace(seatNumber) && 
                !ticket.SeatNumber.Equals(seatNumber, StringComparison.OrdinalIgnoreCase))
            {
                var seatAssigned = await _seatSelectionService.AssignSeatAsync(ticketId, seatNumber);
                if (!seatAssigned)
                {
                    TempData["Error"] = "Seat selection failed. The seat may have been taken. Please try another seat.";
                    return RedirectToAction("SeatMap", new { ticketId });
                }

                // Reload ticket to get updated seat number
                ticket = await ticketRepository.GetCompleteAsync(ticketId);
            }

            // If no seat assigned yet, redirect to seat selection
            if (string.IsNullOrWhiteSpace(ticket?.SeatNumber))
            {
                TempData["Error"] = "Please select a seat before checking in.";
                return RedirectToAction("SeatMap", new { ticketId });
            }

            try
            {
                // Generate boarding pass
                var boardingPassPath = await _boardingPassService.GenerateBoardingPassAsync(ticket);

                // Update check-in status
                var checkInTime = DateTime.UtcNow;
                await ticketRepository.UpdateCheckInStatusAsync(ticketId, true, checkInTime, boardingPassPath);

                // Send boarding pass email (non-blocking - don't fail check-in if email fails)
                try
                {
                    await _boardingPassService.SendBoardingPassEmailAsync(ticket, boardingPassPath);
                }
                catch (Exception)
                {
                    // Log error but don't fail check-in
                    // In production, use ILogger here
                }

                // Redirect to confirmation
                return RedirectToAction("Confirmation", new { ticketId });
            }
            catch (Exception ex)
            {
                // For development/debugging, surface the exception message to help diagnose issues.
                // In production, this should be logged instead of shown to the user.
                TempData["Error"] = $"An error occurred during check-in: {ex.Message}";
                return RedirectToAction("SeatMap", new { ticketId });
            }
        }

        /// <summary>
        /// Selects or changes seat during check-in
        /// </summary>
        /// <param name="ticketId">Ticket ID</param>
        /// <param name="seatNumber">Seat number to assign</param>
        /// <returns>JSON response</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SelectSeat(int ticketId, string seatNumber)
        {
            var ticket = await ValidateTicketAccess(ticketId);
            if (ticket == null)
            {
                return Json(new { success = false, error = "Unauthorized access." });
            }

            if (string.IsNullOrWhiteSpace(seatNumber))
            {
                return Json(new { success = false, error = "Seat number is required." });
            }

            var ticketRepository = (TicketRepository)_ticketRepository;

            // Validate eligibility
            if (!await ticketRepository.IsEligibleForCheckInAsync(ticketId))
            {
                return Json(new { success = false, error = "Ticket is not eligible for check-in." });
            }

            // Assign seat
            var success = await _seatSelectionService.AssignSeatAsync(ticketId, seatNumber);
            if (success)
            {
                return Json(new { success = true, seatNumber = seatNumber.ToUpper(), message = "Seat assigned successfully." });
            }
            else
            {
                return Json(new { success = false, error = "Seat is not available. Please select another seat." });
            }
        }

        /// <summary>
        /// Displays check-in confirmation page
        /// </summary>
        /// <param name="ticketId">Ticket ID</param>
        /// <returns>Confirmation view</returns>
        [HttpGet]
        public async Task<IActionResult> Confirmation(int ticketId)
        {
            var ticket = await ValidateTicketAccess(ticketId);
            if (ticket == null)
            {
                return RedirectToAction("Index");
            }

            var ticketRepository = (TicketRepository)_ticketRepository;
            ticket = await ticketRepository.GetCompleteAsync(ticketId);
            if (ticket == null || ticket.Trip == null)
            {
                TempData["Error"] = "Ticket not found.";
                return RedirectToAction("Index");
            }

            if (!ticket.IsCheckedIn)
            {
                TempData["Error"] = "Check-in not completed.";
                return RedirectToAction("Index");
            }

            return View(ticket);
        }

        #region Helper Methods

        /// <summary>
        /// Validates that the current user has access to the ticket
        /// Supports both PNR+Email (session) and logged-in user access
        /// </summary>
        private async Task<Ticket?> ValidateTicketAccess(int ticketId)
        {
            var ticketRepository = (TicketRepository)_ticketRepository;
            var ticket = await ticketRepository.GetByIdAsync(ticketId);

            if (ticket == null)
                return null;

            // Check if user is logged in and owns the ticket
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId.HasValue && ticket.UserId == userId.Value)
                return ticket;

            // Check if PNR+Email validated in session
            var sessionPnr = HttpContext.Session.GetString("CheckInPNR");
            var sessionEmail = HttpContext.Session.GetString("CheckInEmail");
            if (!string.IsNullOrEmpty(sessionPnr) && !string.IsNullOrEmpty(sessionEmail))
            {
                var sessionTicket = await ticketRepository.GetByPNRAndEmailAsync(sessionPnr, sessionEmail);
                if (sessionTicket != null && sessionTicket.Id == ticketId)
                {
                    return ticket;
                }
            }

            return null; // Unauthorized
        }

        /// <summary>
        /// Shows check-in view with eligibility status
        /// </summary>
        private async Task<IActionResult> ShowCheckInView(Ticket ticket)
        {
            var ticketRepository = (TicketRepository)_ticketRepository;
            
            // Store PNR and email in session for subsequent requests
            HttpContext.Session.SetString("CheckInPNR", ticket.PNR ?? "");
            HttpContext.Session.SetString("CheckInEmail", ticket.User.Email);

            // Check eligibility
            var isEligible = await ticketRepository.IsEligibleForCheckInAsync(ticket.Id);

            // Calculate check-in window
            var departureTime = ticket.Trip.DepartureTime;
            var checkInWindowStart = departureTime.AddHours(-48);
            var checkInWindowEnd = departureTime.AddHours(-2);

            var viewModel = new CheckInViewModel
            {
                Ticket = ticket,
                IsEligible = isEligible,
                CheckInWindowStart = checkInWindowStart,
                CheckInWindowEnd = checkInWindowEnd,
                EligibilityMessage = isEligible 
                    ? "You can proceed with check-in." 
                    : GetEligibilityMessage(ticket)
            };

            return View("Index", viewModel);
        }

        /// <summary>
        /// Gets eligibility message explaining why check-in is not available
        /// </summary>
        private string GetEligibilityMessage(Ticket ticket)
        {
            if (ticket.IsCheckedIn)
                return "You have already checked in for this flight.";

            if (ticket.PaymentStatus != PaymentStatus.Success)
                return "Payment must be confirmed before check-in.";

            var now = DateTime.UtcNow;
            var departureTime = ticket.Trip.DepartureTime;
            var hoursUntilDeparture = (departureTime - now).TotalHours;

            if (hoursUntilDeparture < 2)
                return "Check-in has closed. Please check in at the airport.";

            if (hoursUntilDeparture > 48)
                return $"Check-in will be available starting {departureTime.AddHours(-48):g}.";

            if (ticket.Trip.Status == TripStatus.Cancelled)
                return "This flight has been cancelled.";

            return "Check-in is not available at this time.";
        }

        #endregion
    }
}

