using Microsoft.EntityFrameworkCore;
using Ticket_Booking.Enums;
using Ticket_Booking.Models.DomainModels;
using Ticket_Booking.Services;

namespace Ticket_Booking.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
        {
        }
        public AppDbContext() { }
        public DbSet<User> Users { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Trip> Trips { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Review> Reviews { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=ticket_booking.db");
        }
        public void Seed()
        {
            User admin = new User()
            {
                FullName = "Admin User",
                Email = "admin@gmail.com",
                PasswordHash = AuthenticationService.HashPassword("123"),
                Role = Role.Admin
            };

            User parnter = new User()
            {
                FullName = "Partner User",
                Email = "partner@gmail.com",
                PasswordHash = AuthenticationService.HashPassword("123"),
                Role = Role.Partner
            };

            User user = new()
            {
                FullName = "Thanh Trung",
                Email = "user@gmail.com",
                PasswordHash = AuthenticationService.HashPassword("123"),
                Role = Role.User
            };

            if(Users.Any(u => u.Email == user.Email) == false)
                Users.Add(user);
            if(Users.Any(u => u.Email == admin.Email) == false)
                Users.Add(admin);
            if(Users.Any(u => u.Email == parnter.Email) == false)
                Users.Add(parnter);

            SaveChanges();

            // Seed Companies
            SeedCompanies(parnter.Id);
            
            // Seed Trips
            SeedTrips();
        }

        private void SeedCompanies(int partnerId)
        {
            if (Companies.Any())
                return;

            var companies = new List<Company>
            {
                new Company
                {
                    Name = "Vietnam Airlines",
                    Contact = "contact@vietnamairlines.com",
                    LogoUrl = "https://www.vietnamairlines.com/logo.png",
                    OwnerId = partnerId
                },
                new Company
                {
                    Name = "VietJet Air",
                    Contact = "contact@vietjetair.com",
                    LogoUrl = "https://www.vietjetair.com/logo.png",
                    OwnerId = partnerId
                },
                new Company
                {
                    Name = "Bamboo Airways",
                    Contact = "contact@bambooairways.com",
                    LogoUrl = "https://www.bambooairways.com/logo.png",
                    OwnerId = partnerId
                },
                new Company
                {
                    Name = "Jetstar Pacific",
                    Contact = "contact@jetstarpacific.com",
                    LogoUrl = "https://www.jetstarpacific.com/logo.png",
                    OwnerId = partnerId
                }
            };

            Companies.AddRange(companies);
            SaveChanges();
        }

        private void SeedTrips()
        {
            if (Trips.Any())
                return;

            var companies = Companies.ToList();
            if (!companies.Any())
                return;

            var now = DateTime.UtcNow;
            var random = new Random();

            // Popular routes in Vietnam
            var routes = new List<(string from, string to, int duration)>
            {
                ("Ho Chi Minh City", "Ha Noi", 120),      // 2 hours
                ("Ha Noi", "Ho Chi Minh City", 120),
                ("Ho Chi Minh City", "Da Nang", 90),      // 1.5 hours
                ("Da Nang", "Ho Chi Minh City", 90),
                ("Ha Noi", "Da Nang", 90),
                ("Da Nang", "Ha Noi", 90),
                ("Ho Chi Minh City", "Nha Trang", 60),    // 1 hour
                ("Nha Trang", "Ho Chi Minh City", 60),
                ("Ha Noi", "Phu Quoc", 150),              // 2.5 hours
                ("Phu Quoc", "Ha Noi", 150),
                ("Ho Chi Minh City", "Phu Quoc", 60),
                ("Phu Quoc", "Ho Chi Minh City", 60),
                ("Da Nang", "Nha Trang", 60),
                ("Nha Trang", "Da Nang", 60)
            };

            var planeNames = new List<string>
            {
                "Boeing 787 Dreamliner",
                "Airbus A350",
                "Boeing 737 MAX",
                "Airbus A321",
                "Boeing 777",
                "Airbus A330"
            };

            var trips = new List<Trip>();

            // Create trips for the next 30 days
            for (int day = 0; day < 30; day++)
            {
                var baseDate = now.AddDays(day);
                
                // 2-3 flights per day
                int flightsPerDay = random.Next(2, 4);
                
                for (int flight = 0; flight < flightsPerDay; flight++)
                {
                    var route = routes[random.Next(routes.Count)];
                    var company = companies[random.Next(companies.Count)];
                    var planeName = planeNames[random.Next(planeNames.Count)];
                    
                    // Random departure time between 6 AM and 10 PM
                    var hour = random.Next(6, 23);
                    var minute = random.Next(0, 60);
                    var departureTime = new DateTime(baseDate.Year, baseDate.Month, baseDate.Day, hour, minute, 0, DateTimeKind.Utc);
                    var arrivalTime = departureTime.AddMinutes(route.duration);

                    // Calculate distance (approximate)
                    var distance = route.duration * 8.5m; // ~8.5 km per minute of flight

                    // Price varies by route and class (in USD)
                    // Base price: $2-5 per minute of flight, depending on route popularity
                    var pricePerMinute = random.Next(20, 51) / 10m; // $2.0 - $5.0 per minute
                    var basePrice = route.duration * pricePerMinute; // Base price in USD
                    
                    // Random round-trip discount (0-20%)
                    var roundTripDiscount = random.Next(0, 21); // 0-20% discount
                    
                    trips.Add(new Trip
                    {
                        CompanyId = company.Id,
                        PlaneName = planeName,
                        FromCity = route.from,
                        ToCity = route.to,
                        Distance = distance,
                        EstimatedDuration = route.duration,
                        DepartureTime = departureTime,
                        ArrivalTime = arrivalTime,
                        EconomyPrice = basePrice,
                        EconomySeats = random.Next(150, 200), // 150-200 economy seats
                        BusinessPrice = basePrice * 2.5m,
                        BusinessSeats = random.Next(20, 40), // 20-40 business seats
                        FirstClassPrice = basePrice * 5m,
                        FirstClassSeats = random.Next(8, 16), // 8-16 first class seats
                        RoundTripDiscountPercent = roundTripDiscount,
                        PriceLastUpdated = now.AddDays(-random.Next(0, 30)), // Random date within last 30 days
                        Status = TripStatus.Active
                    });
                }
            }

            // Add some trips in check-in window (24-48 hours from now) for testing
            var checkInWindowStart = now.AddHours(24);
            var checkInWindowEnd = now.AddHours(48);
            
            for (int i = 0; i < 5; i++)
            {
                var route = routes[random.Next(routes.Count)];
                var company = companies[random.Next(companies.Count)];
                var planeName = planeNames[random.Next(planeNames.Count)];
                
                // Random time within check-in window
                var hoursOffset = random.Next(24, 48);
                var departureTime = now.AddHours(hoursOffset);
                var arrivalTime = departureTime.AddMinutes(route.duration);
                var distance = route.duration * 8.5m;
                
                // Price varies by route and class (in USD)
                // Base price: $2-5 per minute of flight, depending on route popularity
                var pricePerMinute = random.Next(20, 51) / 10m; // $2.0 - $5.0 per minute
                var basePrice = route.duration * pricePerMinute; // Base price in USD

                // Random round-trip discount (0-20%)
                var roundTripDiscount = random.Next(0, 21); // 0-20% discount
                
                trips.Add(new Trip
                {
                    CompanyId = company.Id,
                    PlaneName = planeName,
                    FromCity = route.from,
                    ToCity = route.to,
                    Distance = distance,
                    EstimatedDuration = route.duration,
                    DepartureTime = departureTime,
                    ArrivalTime = arrivalTime,
                    EconomyPrice = basePrice,
                    EconomySeats = random.Next(150, 200),
                    BusinessPrice = basePrice * 2.5m,
                    BusinessSeats = random.Next(20, 40),
                    FirstClassPrice = basePrice * 5m,
                    FirstClassSeats = random.Next(8, 16),
                    RoundTripDiscountPercent = roundTripDiscount,
                    PriceLastUpdated = now.AddDays(-random.Next(0, 30)), // Random date within last 30 days
                    Status = TripStatus.Active
                });
            }

            Trips.AddRange(trips);
            SaveChanges();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

          
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Phone).HasMaxLength(15);
            });

           
            modelBuilder.Entity<Company>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Contact).HasMaxLength(100);
                entity.Property(e => e.LogoUrl).HasMaxLength(255);
                
                entity.HasOne(e => e.Owner)
                    .WithMany(u => u.Companies)
                    .HasForeignKey(e => e.OwnerId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

          
            modelBuilder.Entity<Trip>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.EconomyPrice).HasPrecision(10, 2);
                entity.Property(e => e.BusinessPrice).HasPrecision(10, 2);
                entity.Property(e => e.FirstClassPrice).HasPrecision(10, 2);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
                entity.Property(e => e.RoundTripDiscountPercent).HasPrecision(5, 2).HasDefaultValue(0);
                entity.Property(e => e.PriceLastUpdated);
                
                entity.HasOne(e => e.Company)
                    .WithMany(c => c.Trips)
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

      
            modelBuilder.Entity<Ticket>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.SeatNumber).IsRequired().HasMaxLength(10);
                entity.Property(e => e.PaymentStatus).IsRequired().HasMaxLength(20);
                entity.Property(e => e.QrCode).HasMaxLength(255);
                entity.Property(e => e.PNR).HasMaxLength(6);
                entity.Property(e => e.IsCheckedIn).HasDefaultValue(false);
                entity.Property(e => e.BoardingPassUrl).HasMaxLength(500);
                entity.Property(e => e.TotalPrice).HasPrecision(10, 2);
                entity.Property(e => e.PassengerName).HasMaxLength(100);
                entity.Property(e => e.IsCancelled).HasDefaultValue(false);
                entity.Property(e => e.CancellationReason).HasMaxLength(500);
                
                // Round-trip booking fields
                entity.Property(e => e.Type).HasDefaultValue(TicketType.OneWay);
                entity.Property(e => e.OutboundTicketId);
                entity.Property(e => e.ReturnTicketId);
                entity.Property(e => e.BookingGroupId);
                
                // Unique index on PNR (allows multiple NULLs for backward compatibility)
                entity.HasIndex(e => e.PNR)
                    .IsUnique()
                    .HasFilter("[PNR] IS NOT NULL");
                
                // Index on IsCheckedIn for fast queries
                entity.HasIndex(e => e.IsCheckedIn)
                    .HasDatabaseName("IX_Tickets_IsCheckedIn");
                
                // Composite index for seat availability queries
                entity.HasIndex(e => new { e.TripId, e.SeatNumber })
                    .HasDatabaseName("IX_Tickets_TripId_SeatNumber");
                
                // Indexes for round-trip booking queries
                entity.HasIndex(e => e.Type)
                    .HasDatabaseName("IX_Tickets_Type");
                entity.HasIndex(e => e.BookingGroupId)
                    .HasDatabaseName("IX_Tickets_BookingGroupId");
                entity.HasIndex(e => new { e.BookingGroupId, e.Type })
                    .HasDatabaseName("IX_Tickets_BookingGroupId_Type");
                entity.HasIndex(e => e.OutboundTicketId)
                    .HasDatabaseName("IX_Tickets_OutboundTicketId");
                entity.HasIndex(e => e.ReturnTicketId)
                    .HasDatabaseName("IX_Tickets_ReturnTicketId");
                
                entity.HasOne(e => e.Trip)
                    .WithMany(t => t.Tickets)
                    .HasForeignKey(e => e.TripId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(e => e.User)
                    .WithMany(u => u.Tickets)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                // Self-referencing relationships for round-trip ticket linking
                entity.HasOne(e => e.ReturnTicket)
                    .WithOne()
                    .HasForeignKey<Ticket>(e => e.ReturnTicketId)
                    .OnDelete(DeleteBehavior.SetNull);
                
                entity.HasOne(e => e.OutboundTicket)
                    .WithOne()
                    .HasForeignKey<Ticket>(e => e.OutboundTicketId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Method).IsRequired().HasMaxLength(50);
                entity.Property(e => e.TransactionCode).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Amount).HasPrecision(10, 2);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
                
                entity.HasOne(e => e.Ticket)
                    .WithOne(t => t.Payment)
                    .HasForeignKey<Payment>(e => e.TicketId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            
            modelBuilder.Entity<Review>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Comment).HasMaxLength(255);
                
                entity.HasOne(e => e.User)
                    .WithMany(u => u.Reviews)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(e => e.Company)
                    .WithMany(c => c.Reviews)
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
