using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
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
        
        [McpServerTool, Description("Find round-trip routes with flights in both directions")]
        public static string FindRoundTripRoutes()
        {
            var futureTrips = _context.Trips
                .Include(t => t.Company)
                .Where(t => t.DepartureTime > DateTime.Now)
                .ToList();
            
            var roundTripRoutes = new List<string>();
            roundTripRoutes.Add("=== ROUND-TRIP ROUTES WITH MATCHING FLIGHTS ===\n");
            
            var groupedByRoute = futureTrips
                .GroupBy(t => new { t.FromCity, t.ToCity })
                .ToList();
            
            int count = 0;
            foreach (var outboundGroup in groupedByRoute)
            {
                // Find matching return flights
                var returnFlights = futureTrips
                    .Where(t => t.FromCity == outboundGroup.Key.ToCity && 
                               t.ToCity == outboundGroup.Key.FromCity)
                    .ToList();
                
                if (returnFlights.Any())
                {
                    count++;
                    var outboundDates = outboundGroup.Select(t => t.DepartureTime.Date).Distinct().OrderBy(d => d).ToList();
                    var returnDates = returnFlights.Select(t => t.DepartureTime.Date).Distinct().OrderBy(d => d).ToList();
                    var airlines = outboundGroup.Select(t => t.Company?.Name).Distinct().Where(n => n != null).ToList();
                    
                    roundTripRoutes.Add($"\n{count}. {outboundGroup.Key.FromCity} ↔ {outboundGroup.Key.ToCity}");
                    roundTripRoutes.Add($"   Outbound: {outboundGroup.Count()} flights on {outboundDates.Count} dates");
                    roundTripRoutes.Add($"   Return: {returnFlights.Count} flights on {returnDates.Count} dates");
                    roundTripRoutes.Add($"   Airlines: {string.Join(", ", airlines)}");
                    
                    if (outboundDates.Any() && returnDates.Any())
                    {
                        var testOutDate = outboundDates.First();
                        var testRetDate = returnDates.First(d => d > testOutDate);
                        roundTripRoutes.Add($"   Test URL: /User?tripType=RoundTrip&fromCity={Uri.EscapeDataString(outboundGroup.Key.FromCity)}&toCity={Uri.EscapeDataString(outboundGroup.Key.ToCity)}&date={testOutDate:yyyy-MM-dd}&returnDate={testRetDate:yyyy-MM-dd}&sortBy=DepartureTimeAsc&seatClass=Economy");
                    }
                }
            }
            
            if (count == 0)
            {
                roundTripRoutes.Add("\n❌ No round-trip routes found!");
                roundTripRoutes.Add("\nAvailable one-way routes:");
                foreach (var route in groupedByRoute.Take(10))
                {
                    roundTripRoutes.Add($"  • {route.Key.FromCity} → {route.Key.ToCity} ({route.Count()} flights)");
                }
            }
            else
            {
                roundTripRoutes.Add($"\n✅ Found {count} round-trip routes!");
            }
            
            return string.Join("\n", roundTripRoutes);
        }
    }
}
