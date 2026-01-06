using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ticket_Booking.Enums;
using Ticket_Booking.Interfaces;
using Ticket_Booking.Models.BindingModels;
using Ticket_Booking.Models.DomainModels;
using Ticket_Booking.Models.ViewModels;
using Ticket_Booking.Repositories;
using Ticket_Booking.Services;
using Ticket_Booking.Data;
using Ticket_Booking.Helpers;
using VNPAY;
using VNPAY.Models.Enums;
using Microsoft.Extensions.Configuration;

namespace Ticket_Booking.Controllers
{
    public class TicketChangeController : Controller
    {
        private readonly IRepository<Ticket> _ticketRepository;
        private readonly TripRepository _tripRepository;
        private readonly ITicketChangeService _ticketChangeService;
        private readonly AppDbContext _context;
        private readonly IPNRHelper _pnrHelper;
        private readonly TicketRepository _ticketRepositoryForPNR;
        private readonly IVnpayClientFactory _vnPayClientFactory;
        private readonly ICurrencyService _currencyService;
        private readonly IConfiguration _configuration;

        public TicketChangeController(
            IRepository<Ticket> ticketRepository,
            TripRepository tripRepository,
            ITicketChangeService ticketChangeService,
            AppDbContext context,
            IPNRHelper pnrHelper,
            TicketRepository ticketRepositoryForPNR,
            IVnpayClientFactory vnPayClientFactory,
            ICurrencyService currencyService,
            IConfiguration configuration)
        {
            _ticketRepository = ticketRepository;
            _tripRepository = tripRepository;
            _ticketChangeService = ticketChangeService;
            _context = context;
            _pnrHelper = pnrHelper;
            _ticketRepositoryForPNR = ticketRepositoryForPNR;
            _vnPayClientFactory = vnPayClientFactory;
            _currencyService = currencyService;
            _configuration = configuration;
        }

        /// <summary>
        /// Display ticket change page for a specific ticket
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(int ticketId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var ticket = await _ticketRepository.GetByIdAsync(ticketId);
            if (ticket == null || ticket.UserId != userId)
            {
                return NotFound();
            }

            // Check if ticket can be changed
            var (allowed, message, changeFee) = await _ticketChangeService.CheckChangeEligibilityAsync(ticket);
            if (!allowed)
            {
                TempData["ErrorMessage"] = message;
                return RedirectToAction("MyTickets", "User");
            }

            var originalTrip = await _tripRepository.GetByIdAsync(ticket.TripId);
            if (originalTrip == null)
            {
                return NotFound();
            }

            // Get available trips (same route, future dates)
            var availableTrips = await _tripRepository.GetAllAsync();
            var hoursBeforeDeparture = (int)(originalTrip.DepartureTime - DateTime.UtcNow).TotalHours;

            var viewModel = new TicketChangeViewModel
            {
                OriginalTicket = ticket,
                OriginalTrip = originalTrip,
                ChangeFee = changeFee,
                IsChangeAllowed = allowed,
                HoursBeforeDeparture = hoursBeforeDeparture,
                AvailableTrips = availableTrips
                    .Where(t => t.Id != ticket.TripId && 
                                t.DepartureTime > DateTime.UtcNow)  // Any future trip (allow different routes)
                    .OrderBy(t => t.DepartureTime)
                    .Take(50)  // Show more options
            };

            return View(viewModel);
        }

        /// <summary>
        /// Calculate change fee and price difference when user selects a new trip
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CalculateChange(int ticketId, int newTripId, SeatClass? newSeatClass)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập." });
            }

            var ticket = await _ticketRepository.GetByIdAsync(ticketId);
            if (ticket == null || ticket.UserId != userId)
            {
                return Json(new { success = false, message = "Không tìm thấy vé." });
            }

            var newTrip = await _tripRepository.GetByIdAsync(newTripId);
            if (newTrip == null)
            {
                return Json(new { success = false, message = "Không tìm thấy chuyến bay mới." });
            }

            // Check seat availability
            var targetSeatClass = newSeatClass ?? ticket.SeatClass;
            var availableSeats = targetSeatClass switch
            {
                SeatClass.Economy => newTrip.EconomySeats,
                SeatClass.Business => newTrip.BusinessSeats,
                SeatClass.FirstClass => newTrip.FirstClassSeats,
                _ => 0
            };

            if (availableSeats <= 0)
            {
                return Json(new { success = false, message = "Chuyến bay này đã hết chỗ." });
            }

            // Calculate fees
            var changeFee = await _ticketChangeService.CalculateChangeFeeAsync(ticket);
            var (totalDue, refundAmount) = await _ticketChangeService.CalculateTotalChangeAmountAsync(
                ticket, newTrip, newSeatClass);
            var priceDifference = await _ticketChangeService.CalculatePriceDifferenceAsync(
                ticket, newTrip, newSeatClass);

            var newPrice = targetSeatClass switch
            {
                SeatClass.Economy => newTrip.EconomyPrice,
                SeatClass.Business => newTrip.BusinessPrice,
                SeatClass.FirstClass => newTrip.FirstClassPrice,
                _ => newTrip.EconomyPrice
            };

            return Json(new
            {
                success = true,
                changeFee,
                priceDifference,
                totalDue,
                refundAmount,
                newPrice,
                originalPrice = ticket.TotalPrice
            });
        }

        /// <summary>
        /// Confirm and process ticket change
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ConfirmChange(int ticketId, int newTripId, SeatClass? newSeatClass, string? changeReason)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var ticket = await _ticketRepository.GetByIdAsync(ticketId);
            if (ticket == null || ticket.UserId != userId)
            {
                return NotFound();
            }

            // Re-check eligibility
            var (allowed, message, _) = await _ticketChangeService.CheckChangeEligibilityAsync(ticket);
            if (!allowed)
            {
                TempData["ErrorMessage"] = message;
                return RedirectToAction("Index", new { ticketId });
            }

            var newTrip = await _tripRepository.GetByIdAsync(newTripId);
            if (newTrip == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy chuyến bay mới.";
                return RedirectToAction("Index", new { ticketId });
            }

            // Check seat availability
            var targetSeatClass = newSeatClass ?? ticket.SeatClass;
            var availableSeats = targetSeatClass switch
            {
                SeatClass.Economy => newTrip.EconomySeats,
                SeatClass.Business => newTrip.BusinessSeats,
                SeatClass.FirstClass => newTrip.FirstClassSeats,
                _ => 0
            };

            if (availableSeats <= 0)
            {
                TempData["ErrorMessage"] = "Chuyến bay này đã hết chỗ.";
                return RedirectToAction("Index", new { ticketId });
            }

            // Calculate amounts
            var changeFee = await _ticketChangeService.CalculateChangeFeeAsync(ticket);
            var (totalDue, refundAmount) = await _ticketChangeService.CalculateTotalChangeAmountAsync(
                ticket, newTrip, targetSeatClass);
            var priceDifference = await _ticketChangeService.CalculatePriceDifferenceAsync(
                ticket, newTrip, targetSeatClass);

            var newPrice = targetSeatClass switch
            {
                SeatClass.Economy => newTrip.EconomyPrice,
                SeatClass.Business => newTrip.BusinessPrice,
                SeatClass.FirstClass => newTrip.FirstClassPrice,
                _ => newTrip.EconomyPrice
            };

            // If total due > 0, redirect to payment
            if (totalDue > 0)
            {
                // Store change request in session for payment callback
                var changeRequest = new TicketChangeRequest
                {
                    NewTripId = newTripId,
                    NewSeatClass = targetSeatClass,
                    ChangeFee = changeFee,
                    PriceDifference = priceDifference,
                    TotalDue = totalDue,
                    RefundAmount = refundAmount,
                    ChangeReason = changeReason
                };
                HttpContext.Session.SetString($"TicketChange_{ticketId}", 
                    System.Text.Json.JsonSerializer.Serialize(changeRequest));

                // Redirect to payment page
                return RedirectToAction("PayForChange", new { ticketId, amount = totalDue });
            }

            // If no payment needed (same price or cheaper - only change fee), process change directly
            // Note: No refunds even if new ticket is cheaper
            await ProcessTicketChangeAsync(ticket, newTrip, targetSeatClass, changeFee, priceDifference, 0, changeReason, userId.Value);

            TempData["SuccessMessage"] = "Đổi vé thành công!";

            return RedirectToAction("MyTickets", "User");
        }

        /// <summary>
        /// Payment page for ticket change - redirects to VNPay
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> PayForChange(int ticketId, decimal amount)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var ticket = await _ticketRepository.GetByIdAsync(ticketId);
            if (ticket == null || ticket.UserId != userId)
            {
                return NotFound();
            }

            // Verify change request exists in session
            var changeDataJson = HttpContext.Session.GetString($"TicketChange_{ticketId}");
            if (string.IsNullOrEmpty(changeDataJson))
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin đổi vé. Vui lòng thử lại.";
                return RedirectToAction("Index", new { ticketId });
            }

            try
            {
                // Store ticketId in session for callback (since VNPay doesn't return custom data)
                HttpContext.Session.SetString("CurrentTicketChangeId", ticketId.ToString());

                // Get ticket change callback URL from config
                var ticketChangeCallbackUrl = _configuration["VNPAY:TicketChangeCallbackUrl"] 
                    ?? _configuration["VNPAY:CallbackUrl"] 
                    ?? $"{Request.Scheme}://{Request.Host}/TicketChange/PaySuccess";

                // Create VNPay client with ticket change callback URL
                var vnPayClient = _vnPayClientFactory.CreateClient(ticketChangeCallbackUrl);

                // Convert USD to VND for VNPay (amount is in USD)
                var priceInVnd = await _currencyService.ConvertAmountAsync(amount, "USD", "VND");
                var moneyToPay = (long)Math.Round(priceInVnd); // VNPay expects long (VND)
                var description = $"Thanh toan phi doi ve so {ticketId} - PNR: {ticket.PNR}";

                var paymentUrlInfo = vnPayClient.CreatePaymentUrl(
                    (double)moneyToPay,
                    description,
                    BankCode.ANY
                );

                return Redirect(paymentUrlInfo.Url);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi tạo thanh toán: {ex.Message}";
                return RedirectToAction("Index", new { ticketId });
            }
        }

        /// <summary>
        /// VNPay callback handler for ticket change payment
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> PaySuccess()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Handle VNPay callback safely
            try
            {
                // Get ticket change callback URL from config to create matching client
                var ticketChangeCallbackUrl = _configuration["VNPAY:TicketChangeCallbackUrl"] 
                    ?? _configuration["VNPAY:CallbackUrl"] 
                    ?? $"{Request.Scheme}://{Request.Host}/TicketChange/PaySuccess";
                
                var vnPayClient = _vnPayClientFactory.CreateClient(ticketChangeCallbackUrl);
                var paymentResult = vnPayClient.GetPaymentResult(this.Request);
                
                // Get ticketId from session (stored when creating payment URL)
                var ticketIdStr = HttpContext.Session.GetString("CurrentTicketChangeId");
                if (string.IsNullOrEmpty(ticketIdStr) || !int.TryParse(ticketIdStr, out int ticketId))
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin vé cần đổi.";
                    return RedirectToAction("MyBooking", "User");
                }

                // Clear session flag
                HttpContext.Session.Remove("CurrentTicketChangeId");

                var ticket = await _ticketRepository.GetByIdAsync(ticketId);
                if (ticket == null || ticket.UserId != userId)
                {
                    TempData["ErrorMessage"] = "Vé không tồn tại hoặc không thuộc quyền sở hữu của bạn.";
                    return RedirectToAction("MyBooking", "User");
                }

                // Get change data from session
                var changeDataJson = HttpContext.Session.GetString($"TicketChange_{ticketId}");
                if (string.IsNullOrEmpty(changeDataJson))
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin đổi vé.";
                    return RedirectToAction("MyBooking", "User");
                }

                var changeData = System.Text.Json.JsonSerializer.Deserialize<TicketChangeRequest>(changeDataJson);
                if (changeData == null)
                {
                    TempData["ErrorMessage"] = "Dữ liệu đổi vé không hợp lệ.";
                    return RedirectToAction("MyBooking", "User");
                }

                var newTrip = await _tripRepository.GetByIdAsync(changeData.NewTripId);
                if (newTrip == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy chuyến bay mới.";
                    return RedirectToAction("MyBooking", "User");
                }

                // Get original trip before processing
                var originalTrip = await _tripRepository.GetByIdAsync(ticket.TripId);
                
                // Process the ticket change and get new ticket
                var newTicket = await ProcessTicketChangeAsync(
                    ticket, 
                    newTrip, 
                    changeData.NewSeatClass, 
                    changeData.ChangeFee, 
                    changeData.PriceDifference, 
                    changeData.RefundAmount, 
                    changeData.ChangeReason, 
                    userId.Value);

                // Get complete ticket with trip information
                var ticketRepo = _ticketRepository as TicketRepository;
                var newTicketComplete = ticketRepo != null 
                    ? await ticketRepo.GetCompleteAsync(newTicket.Id)
                    : await _ticketRepository.GetByIdAsync(newTicket.Id);
                
                var originalTicketComplete = ticketRepo != null
                    ? await ticketRepo.GetCompleteAsync(ticket.Id)
                    : await _ticketRepository.GetByIdAsync(ticket.Id);

                if (newTicketComplete == null || originalTicketComplete == null)
                {
                    TempData["ErrorMessage"] = "Không thể lấy thông tin vé sau khi đổi.";
                    return RedirectToAction("MyBooking", "User");
                }

                // Clear session
                HttpContext.Session.Remove($"TicketChange_{ticketId}");

                // Create view model
                var viewModel = new TicketChangePaymentSuccessViewModel
                {
                    OriginalTicket = originalTicketComplete,
                    NewTicket = newTicketComplete,
                    OriginalTrip = originalTrip ?? originalTicketComplete.Trip,
                    NewTrip = newTrip,
                    ChangeFee = changeData.ChangeFee,
                    PriceDifference = changeData.PriceDifference,
                    TotalAmountPaid = changeData.PriceDifference > 0 
                        ? changeData.ChangeFee + changeData.PriceDifference 
                        : changeData.ChangeFee,
                    ChangeReason = changeData.ChangeReason
                };

                return View("PaySuccess", viewModel);
            }
            catch (VNPAY.Models.Exceptions.VnpayException)
            {
                // If VNPay callback data is invalid or missing
                TempData["ErrorMessage"] = "Xác thực thanh toán thất bại. Vui lòng thử lại hoặc liên hệ hỗ trợ.";
                return RedirectToAction("MyBooking", "User");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi xử lý thanh toán: {ex.Message}";
                return RedirectToAction("MyBooking", "User");
            }
        }

        /// <summary>
        /// Process ticket change after payment success
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ProcessChangeAfterPayment(int ticketId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập." });
            }

            var changeDataJson = HttpContext.Session.GetString($"TicketChange_{ticketId}");
            if (string.IsNullOrEmpty(changeDataJson))
            {
                return Json(new { success = false, message = "Không tìm thấy thông tin đổi vé." });
            }

            var changeData = System.Text.Json.JsonSerializer.Deserialize<TicketChangeRequest>(changeDataJson);
            if (changeData == null)
            {
                return Json(new { success = false, message = "Dữ liệu đổi vé không hợp lệ." });
            }

            var ticket = await _ticketRepository.GetByIdAsync(ticketId);
            if (ticket == null || ticket.UserId != userId)
            {
                return Json(new { success = false, message = "Không tìm thấy vé." });
            }

            var newTrip = await _tripRepository.GetByIdAsync(changeData.NewTripId);
            if (newTrip == null)
            {
                return Json(new { success = false, message = "Không tìm thấy chuyến bay mới." });
            }

            var newSeatClass = changeData.NewSeatClass;
            var changeFee = changeData.ChangeFee;
            var priceDifference = changeData.PriceDifference;
            var refundAmount = changeData.RefundAmount;
            var changeReason = changeData.ChangeReason;

            await ProcessTicketChangeAsync(ticket, newTrip, newSeatClass, changeFee, priceDifference, refundAmount, changeReason, userId.Value);

            // Clear session
            HttpContext.Session.Remove($"TicketChange_{ticketId}");

            return Json(new { success = true, message = "Đổi vé thành công!" });
        }

        /// <summary>
        /// Internal method to process ticket change
        /// </summary>
        private async Task<Ticket> ProcessTicketChangeAsync(
            Ticket ticket, 
            Trip newTrip, 
            SeatClass newSeatClass,
            decimal changeFee,
            decimal priceDifference,
            decimal refundAmount,
            string? changeReason,
            int userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Get original trip for history
                var originalTrip = await _tripRepository.GetByIdAsync(ticket.TripId);

                // Generate new unique PNR for the changed ticket
                var newPNR = await _pnrHelper.GenerateUniquePNRAsync(_ticketRepositoryForPNR);

                // Create new ticket for the change
                var newTicket = new Ticket
                {
                    TripId = newTrip.Id,
                    UserId = userId,
                    SeatClass = newSeatClass,
                    PassengerName = ticket.PassengerName,
                    BookingDate = DateTime.UtcNow,
                    PaymentStatus = PaymentStatus.Success,
                    PNR = newPNR, // Generate new unique PNR for the changed ticket
                    TotalPrice = newSeatClass switch
                    {
                        SeatClass.Economy => newTrip.EconomyPrice,
                        SeatClass.Business => newTrip.BusinessPrice,
                        SeatClass.FirstClass => newTrip.FirstClassPrice,
                        _ => newTrip.EconomyPrice
                    },
                    Type = ticket.Type,
                    OutboundTicketId = ticket.OutboundTicketId,
                    ReturnTicketId = ticket.ReturnTicketId,
                    BookingGroupId = ticket.BookingGroupId,
                    MealOption = ticket.MealOption,
                    BaggageOption = ticket.BaggageOption,
                    AddOnPrice = ticket.AddOnPrice
                };
                await _ticketRepository.AddAsync(newTicket);
                await _context.SaveChangesAsync(); // Save to get newTicket.Id

                // Cancel original ticket - ensure it's tracked by context
                var ticketToCancel = await _context.Tickets.FindAsync(ticket.Id);
                if (ticketToCancel != null)
                {
                    ticketToCancel.IsCancelled = true;
                    ticketToCancel.CancelledAt = DateTime.UtcNow;
                    ticketToCancel.CancellationReason = changeReason ?? "Ticket changed by user.";
                    _context.Tickets.Update(ticketToCancel);
                }

                // Create change history record
                var changeHistory = new TicketChangeHistory
                {
                    OriginalTicketId = ticket.Id,
                    NewTicketId = newTicket.Id,
                    ChangeDate = DateTime.UtcNow,
                    ChangeFee = changeFee,
                    PriceDifference = priceDifference,
                    TotalAmountPaid = priceDifference > 0 ? changeFee + priceDifference : changeFee,
                    ChangeReason = changeReason,
                    Status = "Completed"
                };

                _context.TicketChangeHistories.Add(changeHistory);

                // Update seat availability
                switch (newSeatClass)
                {
                    case SeatClass.Economy:
                        newTrip.EconomySeats--;
                        if (originalTrip != null)
                        {
                            originalTrip.EconomySeats++;
                        }
                        break;
                    case SeatClass.Business:
                        newTrip.BusinessSeats--;
                        if (originalTrip != null)
                        {
                            originalTrip.BusinessSeats++;
                        }
                        break;
                    case SeatClass.FirstClass:
                        newTrip.FirstClassSeats--;
                        if (originalTrip != null)
                        {
                            originalTrip.FirstClassSeats++;
                        }
                        break;
                }

                // If payment is needed, create payment record
                // Note: If new ticket is cheaper, customer still pays change fee but no refund
                // Payment amount = change fee + (price difference if new ticket is more expensive, else 0)
                var paymentAmount = priceDifference > 0 ? changeFee + priceDifference : changeFee;
                if (paymentAmount > 0)
                {
                    var payment = new Payment
                    {
                        TicketId = newTicket.Id,  // Link payment to new ticket
                        Method = PaymentMethod.VNPAY,  // Default, can be changed
                        TransactionCode = Guid.NewGuid().ToString("N")[..16].ToUpper(),  // Generate transaction code
                        Amount = paymentAmount,
                        PaymentDate = DateTime.UtcNow,
                        Status = PaymentStatus.Success
                    };
                    _context.Payments.Add(payment);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                
                return newTicket;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}

