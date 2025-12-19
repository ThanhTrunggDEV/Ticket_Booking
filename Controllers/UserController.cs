using Microsoft.AspNetCore.Mvc;
using Ticket_Booking.Interfaces;
using Ticket_Booking.Models.BindingModels;
using Ticket_Booking.Models.DomainModels;
using Ticket_Booking.Models.ViewModels;
using Ticket_Booking.Repositories;
using Ticket_Booking.Enums;
using VNPAY;
using VNPAY.Models.Enums;
using System.Threading.Tasks;

namespace Ticket_Booking.Controllers
{
    public class UserController : Controller
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Ticket> _ticketRepository;
        private readonly IRepository<Trip> _tripRepository;
        private readonly IVnpayClient _vnPayClient;

        private static int _currentTripId;

        public UserController(IRepository<User> userRepository, IRepository<Ticket> ticketRepository, IRepository<Trip> tripRepository, IVnpayClient vnpayClient)
        {
            _userRepository = userRepository;
            _ticketRepository = ticketRepository;
            _tripRepository = tripRepository;
            _vnPayClient = vnpayClient;
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
            if (ticketRepo != null)
            {
                var tickets = await ticketRepo.GetByUserAsync(userId.Value);
                return View(tickets);
            }
            
            var allTickets = await _ticketRepository.FindAsync(t => t.UserId == userId.Value);
            return View(allTickets);
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
        public async Task<IActionResult> BookTrip(int tripId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var trip = await _tripRepository.GetByIdAsync(tripId);
            if (trip == null)
            {
                return NotFound();
            }

            return View(trip);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmBooking(int tripId, SeatClass seatClass)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var trip = await _tripRepository.GetByIdAsync(tripId);

            if (trip == null)
            {
                return NotFound();
            }
            _currentTripId = tripId;
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

            var ticket = new Ticket
            {
                TripId = tripId,
                UserId = userId.Value,
                SeatClass = seatClass,
                SeatNumber = "A1", 
                BookingDate = DateTime.Now,
                PaymentStatus = PaymentStatus.Pending, // Chờ thanh toán
                TotalPrice = price,
                QrCode = Guid.NewGuid().ToString(),
            };

            await _ticketRepository.AddAsync(ticket);
            await _tripRepository.UpdateAsync(trip);
            await _ticketRepository.SaveChangesAsync();

            
            try
            {
                var moneyToPay = (long)(price * 100); 
                var description = $"Thanh toan ve so {ticket.Id} - {trip.FromCity} to {trip.ToCity}";
                
                var paymentUrlInfo = _vnPayClient.CreatePaymentUrl(
                    moneyToPay,
                    description,
                    BankCode.ANY
                );

                return Redirect(paymentUrlInfo.Url);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error creating payment: {ex.Message}";
                // Nếu lỗi, xóa ticket đã tạo
                await _ticketRepository.DeleteAsync(ticket);
                await _ticketRepository.SaveChangesAsync();
                // Hoàn lại số ghế
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

        public async Task<IActionResult> PaySuccess()
        {


            var ticket = await _ticketRepository.GetByIdAsync(_currentTripId);
            if (ticket == null)
            {
                return NotFound();
            }
            ticket.PaymentStatus = PaymentStatus.Success;
            await _ticketRepository.UpdateAsync(ticket);
            await _ticketRepository.SaveChangesAsync();
            return RedirectToAction("MyBooking");
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
