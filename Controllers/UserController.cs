using Microsoft.AspNetCore.Mvc;
using Ticket_Booking.Interfaces;
using Ticket_Booking.Models.BindingModels;
using Ticket_Booking.Models.DomainModels;
using Ticket_Booking.Models.ViewModels;
using Ticket_Booking.Repositories;
using Ticket_Booking.Enums;
using Ticket_Booking.Services;
using VNPAY;
using VNPAY.Models.Enums;
using System.Threading.Tasks;
using Ticket_Booking.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Ticket_Booking.Controllers
{
    public class UserController : Controller
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Ticket> _ticketRepository;
        private readonly IRepository<Trip> _tripRepository;
        private readonly IVnpayClient _vnPayClient;
        private readonly IRepository<Payment> _paymentRepository;
        private readonly IPNRHelper _pnrHelper;
        private readonly IPriceCalculatorService _priceCalculatorService;
        private readonly TripRepository _tripRepositoryConcrete;
        private readonly ICurrencyService _currencyService;

        // Stores the last created primary ticket id for payment callback
        private static int _currentTripId;

        public UserController(
            IRepository<Payment> paymentRepository,
            IRepository<User> userRepository, 
            IRepository<Ticket> ticketRepository, 
            IRepository<Trip> tripRepository, 
            IVnpayClient vnpayClient, 
            IPNRHelper pnrHelper,
            IPriceCalculatorService priceCalculatorService,
            TripRepository tripRepositoryConcrete,
            ICurrencyService currencyService)
        {
            _userRepository = userRepository;
            _ticketRepository = ticketRepository;
            _tripRepository = tripRepository;
            _paymentRepository = paymentRepository;
            _vnPayClient = vnpayClient;
            _pnrHelper = pnrHelper;
            _priceCalculatorService = priceCalculatorService;
            _tripRepositoryConcrete = tripRepositoryConcrete;
            _currencyService = currencyService;
        }

        public async Task<IActionResult> Index(string? fromCity, string? toCity, DateTime? date)
        {
            var trips = await _tripRepository.GetAllAsync();
            
            var cities = trips.Select(t => t.FromCity)
                .Union(trips.Select(t => t.ToCity))
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            if (!string.IsNullOrEmpty(fromCity))
            {
                trips = trips.Where(t => t.FromCity == fromCity);
            }

            if (!string.IsNullOrEmpty(toCity))
            {
                trips = trips.Where(t => t.ToCity == toCity);
            }

            if (date.HasValue)
            {
                trips = trips.Where(t => t.DepartureTime.Date == date.Value.Date);
            }

            trips = trips.Where(t => t.DepartureTime > DateTime.Now);

            var viewModel = new SearchTripViewModel
            {
                FromCity = fromCity,
                ToCity = toCity,
                Date = date,
                Trips = trips,
                AvailableCities = cities
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var user = await _userRepository.GetByIdAsync(userId.Value);
            if (user == null)
            {
                return RedirectToAction("Index", "Login");
            }

            return View(user);
        }

        public async Task<IActionResult> MyBooking()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var ticketRepo = _ticketRepository as TicketRepository;
            IEnumerable<Ticket> tickets;
            
            if (ticketRepo != null)
            {
                tickets = await ticketRepo.GetByUserAsync(userId.Value);
            }
            else
            {
                tickets = await _ticketRepository.FindAsync(t => t.UserId == userId.Value);
            }
            
            // For round-trip bookings, show both outbound and return tickets separately
            // Get all tickets: one-way tickets and both outbound and return tickets from round-trip bookings
            var displayTickets = tickets
                .Where(t => t.Type == TicketType.OneWay || 
                           t.Type == TicketType.RoundTrip) // Show all tickets including both outbound and return
                .OrderByDescending(t => t.BookingDate)
                .ThenBy(t => t.Type == TicketType.RoundTrip && t.OutboundTicketId.HasValue ? 1 : 0) // Show outbound before return
                .ToList();
            
            return View(displayTickets);
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var user = await _userRepository.GetByIdAsync(userId.Value);
            if (user == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var model = new UserEditProfile
            {
                FullName = user.FullName,
                Phone = user.Phone,
                Email = user.Email
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditProfile(UserEditProfile model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Index", "Login");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userRepository.GetByIdAsync(userId.Value);
            if (user == null)
            {
                return RedirectToAction("Index", "Login");
            }

            user.FullName = model.FullName;
            user.Phone = model.Phone;
            
            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();

            return RedirectToAction("Profile");
        }

        [HttpGet]
        public async Task<IActionResult> BookTrip(int tripId, TicketType? ticketType = null, int? returnTripId = null)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var outboundTrip = await _tripRepository.GetByIdAsync(tripId);
            if (outboundTrip == null)
            {
                return NotFound();
            }

            // Get user info for default passenger name
            var user = await _userRepository.GetByIdAsync(userId.Value);

            var viewModel = new BookingViewModel
            {
                TicketType = ticketType ?? TicketType.OneWay,
                OutboundTrip = outboundTrip,
                OutboundTripId = tripId,
                SeatClass = SeatClass.Economy,
                PassengerName = user?.FullName ?? string.Empty  // Default to user's full name
            };

            // If round-trip is selected, load available return trips
            if (viewModel.TicketType == TicketType.RoundTrip)
            {
                // Find return trips (reverse route: ToCity -> FromCity, same company)
                var returnTrips = await _tripRepositoryConcrete.SearchTripsAsync(
                    outboundTrip.ToCity, 
                    outboundTrip.FromCity, 
                    null);

                // Filter to same company and future dates (return must be after outbound)
                viewModel.AvailableReturnTrips = returnTrips
                    .Where(t => t.CompanyId == outboundTrip.CompanyId && 
                                t.DepartureTime > outboundTrip.ArrivalTime &&
                                (t.EconomySeats > 0 || t.BusinessSeats > 0 || t.FirstClassSeats > 0))
                    .OrderBy(t => t.DepartureTime)
                    .Take(20) // Limit to 20 options
                    .ToList();

                // If returnTripId is provided, load that specific trip
                if (returnTripId.HasValue)
                {
                    viewModel.ReturnTrip = await _tripRepository.GetByIdAsync(returnTripId.Value);
                    viewModel.ReturnTripId = returnTripId.Value;

                    // Calculate price breakdown if both trips are selected
                    if (viewModel.ReturnTrip != null)
                    {
                        viewModel.PriceBreakdown = _priceCalculatorService.CalculateRoundTripPrice(
                            outboundTrip,
                            viewModel.ReturnTrip,
                            viewModel.SeatClass);
                        
                        viewModel.OutboundPrice = viewModel.PriceBreakdown.OutboundPrice;
                        viewModel.ReturnPrice = viewModel.PriceBreakdown.ReturnPrice;
                        viewModel.DiscountAmount = viewModel.PriceBreakdown.DiscountAmount;
                        viewModel.TotalPrice = viewModel.PriceBreakdown.TotalPrice;
                        viewModel.SavingsAmount = viewModel.PriceBreakdown.SavingsAmount;
                        viewModel.DiscountPercent = viewModel.PriceBreakdown.DiscountPercent;
                    }
                }
            }

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmBooking(
            int tripId, 
            SeatClass seatClass, 
            TicketType? ticketType = null,
            int? returnTripId = null,
            string? passengerName = null)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Index", "Login");
            }

            // Validate and normalize passenger name
            var user = await _userRepository.GetByIdAsync(userId.Value);
            var normalizedPassengerName = string.IsNullOrWhiteSpace(passengerName) 
                ? (user?.FullName ?? string.Empty) 
                : passengerName.Trim();
            
            if (string.IsNullOrWhiteSpace(normalizedPassengerName) || normalizedPassengerName.Length < 2)
            {
                TempData["Error"] = "Passenger name must be at least 2 characters long.";
                return RedirectToAction("BookTrip", new { tripId, ticketType });
            }
            
            if (normalizedPassengerName.Length > 100)
            {
                TempData["Error"] = "Passenger name cannot exceed 100 characters.";
                return RedirectToAction("BookTrip", new { tripId, ticketType });
            }

            // Determine if this is a round-trip booking
            var isRoundTrip = ticketType == TicketType.RoundTrip && returnTripId.HasValue && returnTripId.Value > 0;
            
            if (isRoundTrip)
            {
                return await CreateRoundTripBookingAsync(tripId, returnTripId.Value, seatClass, userId.Value, normalizedPassengerName);
            }
            else
            {
                return await CreateOneWayBookingAsync(tripId, seatClass, userId.Value, normalizedPassengerName);
            }
        }

        private async Task<IActionResult> CreateOneWayBookingAsync(int tripId, SeatClass seatClass, int userId, string passengerName)
        {
            var trip = await _tripRepository.GetByIdAsync(tripId);
            if (trip == null)
            {
                return NotFound();
            }

            // Check availability and calculate price
            decimal price = 0;
            bool isAvailable = false;

            switch (seatClass)
            {
                case SeatClass.Economy:
                    if (trip.EconomySeats > 0)
                    {
                        price = trip.EconomyPrice;
                        isAvailable = true;
                        trip.EconomySeats--;
                    }
                    break;
                case SeatClass.Business:
                    if (trip.BusinessSeats > 0)
                    {
                        price = trip.BusinessPrice;
                        isAvailable = true;
                        trip.BusinessSeats--;
                    }
                    break;
                case SeatClass.FirstClass:
                    if (trip.FirstClassSeats > 0)
                    {
                        price = trip.FirstClassPrice;
                        isAvailable = true;
                        trip.FirstClassSeats--;
                    }
                    break;
            }

            if (!isAvailable)
            {
                TempData["Error"] = "Selected seat class is not available.";
                return RedirectToAction("BookTrip", new { tripId });
            }

            // Generate unique PNR code
            string pnr;
            try
            {
                var ticketRepository = (TicketRepository)_ticketRepository;
                pnr = await _pnrHelper.GenerateUniquePNRAsync(ticketRepository);
            }
            catch (InvalidOperationException)
            {
                TempData["Error"] = "Unable to generate booking code. Please try again.";
                return RedirectToAction("BookTrip", new { tripId });
            }

            var ticket = new Ticket
            {
                TripId = tripId,
                UserId = userId,
                SeatClass = seatClass,
                // Seat will be assigned later during check-in or seat selection
                SeatNumber = string.Empty,
                BookingDate = DateTime.Now,
                PaymentStatus = PaymentStatus.Pending,
                TotalPrice = price,
                QrCode = Guid.NewGuid().ToString(),
                PNR = pnr,
                Type = TicketType.OneWay,
                PassengerName = passengerName
            };

            await _ticketRepository.AddAsync(ticket);
            await _tripRepository.UpdateAsync(trip);
            await _ticketRepository.SaveChangesAsync();
            
            // Store ticket id for PaySuccess callback
            _currentTripId = ticket.Id;

            try
            {
                // Convert USD to VND for VNPay (prices in database are in USD)
                var priceInVnd = await _currencyService.ConvertAmountAsync(price, "USD", "VND");
                var moneyToPay = (long)Math.Round(priceInVnd); // VNPay expects long (VND)
                var description = $"Thanh toan ve so {ticket.Id} - {trip.FromCity} to {trip.ToCity}";

                var paymentUrlInfo = _vnPayClient.CreatePaymentUrl(
                    (double)moneyToPay,
                    description,
                    BankCode.ANY
                );

                return Redirect(paymentUrlInfo.Url);
            }
            catch (Exception)
            {
                TempData["Error"] = "Error creating payment. Please try again.";
                await _ticketRepository.DeleteAsync(ticket);
                await _ticketRepository.SaveChangesAsync();
                // Restore seats
                switch (seatClass)
                {
                    case SeatClass.Economy:
                        trip.EconomySeats++;
                        break;
                    case SeatClass.Business:
                        trip.BusinessSeats++;
                        break;
                    case SeatClass.FirstClass:
                        trip.FirstClassSeats++;
                        break;
                }
                await _tripRepository.UpdateAsync(trip);
                await _tripRepository.SaveChangesAsync();
                return RedirectToAction("BookTrip", new { tripId });
            }
        }

        private async Task<IActionResult> CreateRoundTripBookingAsync(int outboundTripId, int returnTripId, SeatClass seatClass, int userId, string passengerName)
        {
            var outboundTrip = await _tripRepository.GetByIdAsync(outboundTripId);
            var returnTrip = await _tripRepository.GetByIdAsync(returnTripId);

            if (outboundTrip == null || returnTrip == null)
            {
                TempData["Error"] = "One or both trips not found.";
                return RedirectToAction("BookTrip", new { tripId = outboundTripId, ticketType = TicketType.RoundTrip });
            }

            // Validate return trip is after outbound
            if (returnTrip.DepartureTime <= outboundTrip.ArrivalTime)
            {
                TempData["Error"] = "Return flight must depart after outbound flight arrival.";
                return RedirectToAction("BookTrip", new { tripId = outboundTripId, ticketType = TicketType.RoundTrip });
            }

            // Check availability for both trips
            bool outboundAvailable = CheckSeatAvailability(outboundTrip, seatClass);
            bool returnAvailable = CheckSeatAvailability(returnTrip, seatClass);

            if (!outboundAvailable || !returnAvailable)
            {
                TempData["Error"] = "Selected seat class is not available for one or both flights.";
                return RedirectToAction("BookTrip", new { tripId = outboundTripId, ticketType = TicketType.RoundTrip });
            }

            // Calculate round-trip price
            var priceBreakdown = _priceCalculatorService.CalculateRoundTripPrice(
                outboundTrip, returnTrip, seatClass);
            var totalPrice = priceBreakdown.TotalPrice;

            // Generate booking group ID (use timestamp-based unique ID)
            var bookingGroupId = (int)(DateTime.UtcNow.Ticks % int.MaxValue);

            // Generate PNRs
            var ticketRepository = (TicketRepository)_ticketRepository;
            string outboundPnr, returnPnr;
            try
            {
                outboundPnr = await _pnrHelper.GenerateUniquePNRAsync(ticketRepository);
                returnPnr = await _pnrHelper.GenerateUniquePNRAsync(ticketRepository);
            }
            catch (InvalidOperationException)
            {
                TempData["Error"] = "Unable to generate booking codes. Please try again.";
                return RedirectToAction("BookTrip", new { tripId = outboundTripId, ticketType = TicketType.RoundTrip });
            }

            // Use database transaction for atomicity
            using var transaction = await ((TicketRepository)_ticketRepository).BeginTransactionAsync();
            try
            {
                // Decrease seat availability
                DecreaseSeatAvailability(outboundTrip, seatClass);
                DecreaseSeatAvailability(returnTrip, seatClass);

                // Create outbound ticket (seat will be assigned later)
                var outboundTicket = new Ticket
                {
                    TripId = outboundTripId,
                    UserId = userId,
                    SeatClass = seatClass,
                    SeatNumber = string.Empty,
                    BookingDate = DateTime.Now,
                    PaymentStatus = PaymentStatus.Pending,
                    TotalPrice = priceBreakdown.OutboundPrice,
                    QrCode = Guid.NewGuid().ToString(),
                    PNR = outboundPnr,
                    Type = TicketType.RoundTrip,
                    BookingGroupId = bookingGroupId,
                    PassengerName = passengerName
                };

                // Create return ticket (seat will be assigned later)
                var returnTicket = new Ticket
                {
                    TripId = returnTripId,
                    UserId = userId,
                    SeatClass = seatClass,
                    SeatNumber = string.Empty,
                    BookingDate = DateTime.Now,
                    PaymentStatus = PaymentStatus.Pending,
                    TotalPrice = priceBreakdown.ReturnPrice,
                    QrCode = Guid.NewGuid().ToString(),
                    PNR = returnPnr,
                    Type = TicketType.RoundTrip,
                    BookingGroupId = bookingGroupId,
                    OutboundTicketId = null, // Will be set after outbound ticket is saved
                    PassengerName = passengerName
                };

                await _ticketRepository.AddAsync(outboundTicket);
                await _ticketRepository.AddAsync(returnTicket);
                await _ticketRepository.SaveChangesAsync();

                // For payment callback, treat outbound ticket as primary
                _currentTripId = outboundTicket.Id;

                // Link tickets bidirectionally
                returnTicket.OutboundTicketId = outboundTicket.Id;
                outboundTicket.ReturnTicketId = returnTicket.Id;
                await _ticketRepository.UpdateAsync(outboundTicket);
                await _ticketRepository.UpdateAsync(returnTicket);

                await _tripRepository.UpdateAsync(outboundTrip);
                await _tripRepository.UpdateAsync(returnTrip);
                await _ticketRepository.SaveChangesAsync();

                await transaction.CommitAsync();

                // Create payment
                try
                {
                    // Convert USD to VND for VNPay (prices in database are in USD)
                    var priceInVnd = await _currencyService.ConvertAmountAsync(totalPrice, "USD", "VND");
                    var moneyToPay = (long)Math.Round(priceInVnd); // VNPay expects long (VND)
                    var description = $"Thanh toan ve khứ hồi - {outboundTrip.FromCity} to {outboundTrip.ToCity} (PNR: {outboundPnr})";

                    var paymentUrlInfo = _vnPayClient.CreatePaymentUrl(
                        (double)moneyToPay,
                        description,
                        BankCode.ANY
                    );

                    return Redirect(paymentUrlInfo.Url);
                }
                catch (Exception)
                {
                    TempData["Error"] = "Error creating payment. Please try again.";
                    // Rollback will happen automatically when transaction is disposed
                    return RedirectToAction("BookTrip", new { tripId = outboundTripId, ticketType = TicketType.RoundTrip });
                }
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = "Error creating booking. Please try again.";
                return RedirectToAction("BookTrip", new { tripId = outboundTripId, ticketType = TicketType.RoundTrip });
            }
        }

        private bool CheckSeatAvailability(Trip trip, SeatClass seatClass)
        {
            return seatClass switch
            {
                SeatClass.Economy => trip.EconomySeats > 0,
                SeatClass.Business => trip.BusinessSeats > 0,
                SeatClass.FirstClass => trip.FirstClassSeats > 0,
                _ => false
            };
        }

        private void DecreaseSeatAvailability(Trip trip, SeatClass seatClass)
        {
            switch (seatClass)
            {
                case SeatClass.Economy:
                    trip.EconomySeats--;
                    break;
                case SeatClass.Business:
                    trip.BusinessSeats--;
                    break;
                case SeatClass.FirstClass:
                    trip.FirstClassSeats--;
                    break;
            }
        }

        public async Task<IActionResult> PaySuccess()
        {
            // Handle VNPay callback safely – in dev/manual testing there may be no VNPay parameters
            try
            {
                var paymentResult = _vnPayClient.GetPaymentResult(this.Request);
                // Currently we don't persist paymentResult, but calling it validates the response
            }
            catch (VNPAY.Models.Exceptions.VnpayException)
            {
                // If VNPay callback data is invalid or missing, treat as payment failure but
                // avoid crashing the app. Show a friendly message instead.
                TempData["Error"] = "Payment verification failed. Please try again or contact support.";
                return RedirectToAction("MyBooking");
            }
            
            var ticketRepository = (TicketRepository)_ticketRepository;
            var primaryTicket = await ticketRepository.GetCompleteAsync(_currentTripId);
            if (primaryTicket == null)
            {
                return NotFound();
            }
            
            // Update payment status for the primary ticket
            primaryTicket.PaymentStatus = PaymentStatus.Success;
            await _ticketRepository.UpdateAsync(primaryTicket);
            
            // Collect all tickets in this booking (round-trip or single)
            var tickets = new List<Ticket> { primaryTicket };
            
            if (primaryTicket.Type == TicketType.RoundTrip && primaryTicket.BookingGroupId.HasValue)
            {
                var linkedTickets = await ticketRepository.FindAsync(t => 
                    t.BookingGroupId == primaryTicket.BookingGroupId.Value && 
                    t.Id != primaryTicket.Id);
                
                foreach (var linkedTicket in linkedTickets)
                {
                    linkedTicket.PaymentStatus = PaymentStatus.Success;
                    await _ticketRepository.UpdateAsync(linkedTicket);
                    tickets.Add(linkedTicket);
                }
            }
            
            await _ticketRepository.SaveChangesAsync();
            
            var viewModel = new PaymentSuccessViewModel
            {
                PrimaryTicket = primaryTicket,
                Tickets = tickets.OrderBy(t => t.Trip.DepartureTime).ToList()
            };
            
            return View("PaymentSuccess", viewModel);
        }


        public async Task<IActionResult> Ticket(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Index", "Login");
            }

            Ticket? ticket = null;
            var ticketRepo = _ticketRepository as TicketRepository;
            
            if (ticketRepo != null)
            {
                ticket = await ticketRepo.GetCompleteAsync(id);
            }
            else
            {
                ticket = await _ticketRepository.GetByIdAsync(id);
            }

            if (ticket == null || ticket.UserId != userId.Value)
            {
                return NotFound();
            }

            return View(ticket);
        }
    }
}
