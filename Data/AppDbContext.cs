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
