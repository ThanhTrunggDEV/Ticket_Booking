using Microsoft.AspNetCore.Mvc;
using Ticket_Booking.Interfaces;
using Ticket_Booking.Models.DomainModels;
using Ticket_Booking.Models.ViewModels;

namespace Ticket_Booking.Controllers
{
    public class PartnerController : Controller
    {
        private readonly IRepository<Trip> _tripRepository;
        private readonly IRepository<Ticket> _ticketRepository;
        private readonly IRepository<Company> _companyRepository;
        private readonly IRepository<Review> _reviewRepository;
        private readonly IRepository<Payment> _paymentRepository;

        public PartnerController(
            IRepository<Review> reviewRepository,
            IRepository<Trip> tripRepository,
            IRepository<Ticket> ticketRepository,
            IRepository<Company> companyRepository,
            IRepository<Payment> paymentRepository)
        {
            _tripRepository = tripRepository;
            _ticketRepository = ticketRepository;
            _companyRepository = companyRepository;
            _reviewRepository = reviewRepository;
            _paymentRepository = paymentRepository;
        }

        public async Task<IActionResult> Index()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Partner")
            {
                return RedirectToAction("Index", "Login");
            }

            var userId = HttpContext.Session.GetInt32("UserId");

            var trips = await _tripRepository.GetAllAsync();
                trips = trips.Where(t => t.Company.OwnerId == userId);

            var tickets = await _ticketRepository.GetAllAsync();
                tickets = tickets.Where(t => t.Trip.Company.OwnerId == userId);

            var companies = await _companyRepository.GetAllAsync();
            var companiesCount = companies.Count(c => c.OwnerId == userId);

            var reviews = await _reviewRepository.GetAllAsync();
                reviews = reviews.Where(r => r.Company.OwnerId == userId);
            
            var viewModel = new PartnerDashboardViewModel
            {
                TotalCompanies = companiesCount,
                TotalTrips = trips.Count(),
                TotalBookings = tickets.Count(), 
                TotalRevenue = tickets.Sum(t => t.TotalPrice),
                AverageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0.0,
                RecentTrips = trips.OrderByDescending(t => t.DepartureTime).Take(5).ToList(),
                RecentBookings = tickets.OrderByDescending(t => t.BookingDate).Take(5).ToList()
            };

            return View(viewModel);
        }

        public async Task<IActionResult> CompaniesManagement()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Partner")
            {
                return RedirectToAction("Index", "Login");
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Index", "Login");

            var companies = await _companyRepository.FindAsync(c => c.OwnerId == userId);
            return View(companies);
        }

        [HttpGet]
        public async Task<IActionResult> CreateCompany()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Partner") return RedirectToAction("Index", "Login");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateCompany(Company company)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Index", "Login");

            company.OwnerId = userId;
            await _companyRepository.AddAsync(company);
            await _companyRepository.SaveChangesAsync();

            return RedirectToAction(nameof(CompaniesManagement));
        }

        [HttpGet]
        public async Task<IActionResult> EditCompany(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Partner") return RedirectToAction("Index", "Login");

            var userId = HttpContext.Session.GetInt32("UserId");
            var company = await _companyRepository.GetByIdAsync(id);

            if (company == null || company.OwnerId != userId)
            {
                return NotFound();
            }

            return View(company);
        }

        [HttpPost]
        public async Task<IActionResult> EditCompany(Company company)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var existingCompany = await _companyRepository.GetByIdAsync(company.Id);

            if (existingCompany == null || existingCompany.OwnerId != userId)
            {
                return NotFound();
            }

            existingCompany.Name = company.Name;
            existingCompany.Contact = company.Contact;
            existingCompany.LogoUrl = company.LogoUrl;

            await _companyRepository.UpdateAsync(existingCompany);
            await _companyRepository.SaveChangesAsync();

            return RedirectToAction(nameof(CompaniesManagement));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCompany(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var company = await _companyRepository.GetByIdAsync(id);

            if (company == null || company.OwnerId != userId)
            {
                return NotFound();
            }

            // 1. Delete Reviews
            var reviews = await _reviewRepository.FindAsync(r => r.CompanyId == id);
            foreach (var review in reviews)
            {
                await _reviewRepository.DeleteAsync(review);
            }

            // 2. Delete Trips (and their Tickets/Payments)
            var trips = await _tripRepository.FindAsync(t => t.CompanyId == id);
            foreach (var trip in trips)
            {
                await DeleteTripInternal(trip.Id);
            }

            await _companyRepository.DeleteAsync(company);
            await _companyRepository.SaveChangesAsync();

            return RedirectToAction(nameof(CompaniesManagement));
        }

        private async Task DeleteTripInternal(int tripId)
        {
            var tickets = await _ticketRepository.FindAsync(t => t.TripId == tripId);
            foreach (var ticket in tickets)
            {
                var payment = await _paymentRepository.FirstOrDefaultAsync(p => p.TicketId == ticket.Id);
                if (payment != null)
                {
                    await _paymentRepository.DeleteAsync(payment);
                }
                await _ticketRepository.DeleteAsync(ticket);
            }

            var trip = await _tripRepository.GetByIdAsync(tripId);
            if (trip != null)
            {
                await _tripRepository.DeleteAsync(trip);
            }
        }

        public async Task<IActionResult> TripsManagement()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Partner") return RedirectToAction("Index", "Login");

            var trips = await _tripRepository.GetAllAsync();
             var userId = HttpContext.Session.GetInt32("UserId");
             trips = trips.Where(t => t.Company.OwnerId == userId);

            return View(trips);
        }

        [HttpGet]
        public async Task<IActionResult> CreateTrip()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Partner") return RedirectToAction("Index", "Login");

            var userId = HttpContext.Session.GetInt32("UserId");
            var companies = await _companyRepository.FindAsync(c => c.OwnerId == userId);

            ViewBag.Companies = companies;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateTrip(Trip trip)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Partner") return RedirectToAction("Index", "Login");

            var userId = HttpContext.Session.GetInt32("UserId");

            var company = await _companyRepository.GetByIdAsync(trip.CompanyId);
            if (company == null || company.OwnerId != userId)
            {
                return Unauthorized();
            }

            trip.Status = Ticket_Booking.Enums.TripStatus.Active;

            await _tripRepository.AddAsync(trip);
            await _tripRepository.SaveChangesAsync();

            return RedirectToAction(nameof(TripsManagement));
        }

        [HttpGet]
        public async Task<IActionResult> EditTrip(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Partner") return RedirectToAction("Index", "Login");

            var userId = HttpContext.Session.GetInt32("UserId");
            var trip = await _tripRepository.GetByIdAsync(id);

            if (trip == null || trip.Company.OwnerId != userId)
            {
                return NotFound();
            }

            var companies = await _companyRepository.FindAsync(c => c.OwnerId == userId);
            ViewBag.Companies = companies;

            return View(trip);
        }

        [HttpPost]
        public async Task<IActionResult> EditTrip(Trip trip)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Partner") return RedirectToAction("Index", "Login");

            var userId = HttpContext.Session.GetInt32("UserId");
            var existingTrip = await _tripRepository.GetByIdAsync(trip.Id);

            if (existingTrip == null || existingTrip.Company.OwnerId != userId)
            {
                return NotFound();
            }

            var company = await _companyRepository.GetByIdAsync(trip.CompanyId);
            if (company == null || company.OwnerId != userId)
            {
                return Unauthorized();
            }

            existingTrip.CompanyId = trip.CompanyId;
            existingTrip.PlaneName = trip.PlaneName;
            existingTrip.FromCity = trip.FromCity;
            existingTrip.ToCity = trip.ToCity;
            existingTrip.Distance = trip.Distance;
            existingTrip.EstimatedDuration = trip.EstimatedDuration;
            existingTrip.DepartureTime = trip.DepartureTime;
            existingTrip.ArrivalTime = trip.ArrivalTime;
            
            existingTrip.EconomyPrice = trip.EconomyPrice;
            existingTrip.EconomySeats = trip.EconomySeats;
            existingTrip.BusinessPrice = trip.BusinessPrice;
            existingTrip.BusinessSeats = trip.BusinessSeats;
            existingTrip.FirstClassPrice = trip.FirstClassPrice;
            existingTrip.FirstClassSeats = trip.FirstClassSeats;

            existingTrip.Status = trip.Status;

            await _tripRepository.UpdateAsync(existingTrip);
            await _tripRepository.SaveChangesAsync();

            return RedirectToAction(nameof(TripsManagement));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTrip(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Partner") return RedirectToAction("Index", "Login");

            var userId = HttpContext.Session.GetInt32("UserId");
            var trip = await _tripRepository.GetByIdAsync(id);

            if (trip == null || trip.Company.OwnerId != userId)
            {
                return NotFound();
            }

            await DeleteTripInternal(id);
            await _tripRepository.SaveChangesAsync();

            return RedirectToAction(nameof(TripsManagement));
        }
    }
}
