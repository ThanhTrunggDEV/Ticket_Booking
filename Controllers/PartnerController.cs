using Microsoft.AspNetCore.Mvc;
using Ticket_Booking.Interfaces;
using Ticket_Booking.Models.DomainModels;
using Ticket_Booking.Models.ViewModels;
using Ticket_Booking.Repositories;
using Ticket_Booking.Services;

namespace Ticket_Booking.Controllers
{
    public class PartnerController : Controller
    {
        private readonly IRepository<Trip> _tripRepository;
        private readonly IRepository<Ticket> _ticketRepository;
        private readonly IRepository<Company> _companyRepository;
        private readonly IRepository<Review> _reviewRepository;
        private readonly IRepository<Payment> _paymentRepository;
        private readonly IPricingService _pricingService;
        private readonly TripRepository _tripRepositoryConcrete;

        public PartnerController(
            IRepository<Review> reviewRepository,
            IRepository<Trip> tripRepository,
            IRepository<Ticket> ticketRepository,
            IRepository<Company> companyRepository,
            IRepository<Payment> paymentRepository,
            IPricingService pricingService,
            TripRepository tripRepositoryConcrete)
        {
            _tripRepository = tripRepository;
            _ticketRepository = ticketRepository;
            _companyRepository = companyRepository;
            _reviewRepository = reviewRepository;
            _paymentRepository = paymentRepository;
            _pricingService = pricingService;
            _tripRepositoryConcrete = tripRepositoryConcrete;
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
            if (userId == null) return RedirectToAction("Index", "Login");

            // Get trip with Company included
            var trip = await _tripRepositoryConcrete.GetCompleteAsync(id);

            if (trip == null || trip.Company == null || trip.Company.OwnerId != userId)
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
            if (userId == null) return RedirectToAction("Index", "Login");

            // Get trip with Company included
            var existingTrip = await _tripRepositoryConcrete.GetCompleteAsync(trip.Id);

            if (existingTrip == null || existingTrip.Company == null || existingTrip.Company.OwnerId != userId)
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
            
            // Update round-trip discount if provided
            if (trip.RoundTripDiscountPercent >= 0 && trip.RoundTripDiscountPercent <= 50)
            {
                existingTrip.RoundTripDiscountPercent = trip.RoundTripDiscountPercent;
                existingTrip.PriceLastUpdated = DateTime.UtcNow;
            }

            existingTrip.Status = trip.Status;

            await _tripRepository.UpdateAsync(existingTrip);
            await _tripRepository.SaveChangesAsync();

            return RedirectToAction(nameof(TripsManagement));
        }

        /// <summary>
        /// Update pricing for a specific trip (including round-trip discount)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdateTripPricing(
            int tripId,
            decimal economyPrice,
            decimal businessPrice,
            decimal firstClassPrice,
            decimal roundTripDiscountPercent)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Partner") return Unauthorized();

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            var trip = await _tripRepository.GetByIdAsync(tripId);
            if (trip == null || trip.Company.OwnerId != userId)
            {
                return NotFound();
            }

            // Validate discount
            if (!_pricingService.ValidateDiscount(roundTripDiscountPercent))
            {
                TempData["Error"] = "Discount must be between 0% and 50%.";
                return RedirectToAction(nameof(TripsManagement));
            }

            // Update prices
            trip.EconomyPrice = economyPrice;
            trip.BusinessPrice = businessPrice;
            trip.FirstClassPrice = firstClassPrice;
            trip.RoundTripDiscountPercent = roundTripDiscountPercent;
            trip.PriceLastUpdated = DateTime.UtcNow;

            await _tripRepository.UpdateAsync(trip);
            await _tripRepository.SaveChangesAsync();

            TempData["Success"] = "Pricing updated successfully.";
            return RedirectToAction(nameof(TripsManagement));
        }

        /// <summary>
        /// Update round-trip discount for all trips in a route
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UpdateRouteDiscount(
            int companyId,
            string fromCity,
            string toCity,
            decimal discountPercent)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Partner") return Unauthorized();

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            // Verify company ownership
            var company = await _companyRepository.GetByIdAsync(companyId);
            if (company == null || company.OwnerId != userId)
            {
                return Unauthorized();
            }

            // Validate discount
            if (!_pricingService.ValidateDiscount(discountPercent))
            {
                TempData["Error"] = "Discount must be between 0% and 50%.";
                return RedirectToAction(nameof(TripsManagement));
            }

            // Update discount for all trips in the route
            var success = await _pricingService.UpdateRouteDiscountAsync(companyId, fromCity, toCity, discountPercent);
            
            if (success)
            {
                TempData["Success"] = $"Round-trip discount updated to {discountPercent}% for all trips on route {fromCity} → {toCity}.";
            }
            else
            {
                TempData["Error"] = "No trips found for this route.";
            }

            return RedirectToAction(nameof(TripsManagement));
        }

        /// <summary>
        /// Get pricing information for a specific route
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> RoutePricing(int companyId, string fromCity, string toCity)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Partner") return Unauthorized();

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            // Verify company ownership
            var company = await _companyRepository.GetByIdAsync(companyId);
            if (company == null || company.OwnerId != userId)
            {
                return Unauthorized();
            }

            // Get discount for the route
            var discount = _pricingService.GetRoundTripDiscount(companyId, fromCity, toCity);
            
            // Get trips for the route
            var trips = await _tripRepositoryConcrete.SearchTripsAsync(fromCity, toCity, null);
            var routeTrips = trips.Where(t => t.CompanyId == companyId).ToList();

            ViewBag.CompanyId = companyId;
            ViewBag.FromCity = fromCity;
            ViewBag.ToCity = toCity;
            ViewBag.Discount = discount;
            ViewBag.Trips = routeTrips;

            return View();
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
