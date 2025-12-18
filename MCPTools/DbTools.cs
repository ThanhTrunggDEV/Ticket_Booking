using System.ComponentModel;
using ModelContextProtocol.Server;
using Ticket_Booking.Data;
using Ticket_Booking.Models.DomainModels;

namespace Ticket_Booking.MCPTools
{
    [McpServerToolType]
    public static class DbTools
    {
        private readonly static AppDbContext _context = new AppDbContext();
        [McpServerTool, Description("Get all trips from the database")]
        public static List<Trip> GetAllTrips() => _context.Trips.ToList();
    }
}
