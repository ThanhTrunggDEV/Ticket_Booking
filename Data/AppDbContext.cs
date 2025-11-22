using Microsoft.EntityFrameworkCore;
using Ticket_Booking.Models.DomainModels;
using Ticket_Booking.Services;
using RouteModel = Ticket_Booking.Models.DomainModels.Route;

namespace Ticket_Booking.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<TransportType> TransportTypes { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<RouteModel> Routes { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
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
            };

            User parnter = new User()
            {
                FullName = "Partner User",
                Email = "partner@gmail.com",
                PasswordHash = AuthenticationService.HashPassword("123"),
            };
            Users.Add(admin);
            Users.Add(parnter);
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

           
            modelBuilder.Entity<TransportType>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            });

           
            modelBuilder.Entity<Company>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Contact).HasMaxLength(100);
                entity.Property(e => e.LogoUrl).HasMaxLength(255);
                
                entity.HasOne(e => e.TransportType)
                    .WithMany(t => t.Companies)
                    .HasForeignKey(e => e.TransportTypeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

           
            modelBuilder.Entity<RouteModel>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FromCity).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ToCity).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Distance).HasPrecision(6, 2);
            });

           
            modelBuilder.Entity<Vehicle>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.VehicleName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Code).IsRequired().HasMaxLength(20);
                
                entity.HasOne(e => e.Company)
                    .WithMany(c => c.Vehicles)
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(e => e.TransportType)
                    .WithMany(t => t.Vehicles)
                    .HasForeignKey(e => e.TransportTypeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

          
            modelBuilder.Entity<Trip>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Price).HasPrecision(10, 2);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
                
                entity.HasOne(e => e.Vehicle)
                    .WithMany(v => v.Trips)
                    .HasForeignKey(e => e.VehicleId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(e => e.Route)
                    .WithMany(r => r.Trips)
                    .HasForeignKey(e => e.RouteId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

      
            modelBuilder.Entity<Ticket>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.SeatNumber).IsRequired().HasMaxLength(10);
                entity.Property(e => e.PaymentStatus).IsRequired().HasMaxLength(20);
                entity.Property(e => e.QrCode).HasMaxLength(255);
                entity.Property(e => e.TotalPrice).HasPrecision(10, 2);
                
                entity.HasOne(e => e.Trip)
                    .WithMany(t => t.Tickets)
                    .HasForeignKey(e => e.TripId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(e => e.User)
                    .WithMany(u => u.Tickets)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
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
