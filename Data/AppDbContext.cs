using Microsoft.EntityFrameworkCore;

namespace Ticket_Booking.Data
{
    public class AppDbContext : DbContext
    {
        
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            
        }
    }
}
