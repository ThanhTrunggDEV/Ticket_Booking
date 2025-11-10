using Microsoft.EntityFrameworkCore;

namespace Ticket_Booking.Data
{
    public class AppDbContext : DbContext
    {

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=ticket_booking.db");
        }
    }
}
