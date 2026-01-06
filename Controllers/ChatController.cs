using Microsoft.AspNetCore.Mvc;
using Ticket_Booking.Interfaces;
using Ticket_Booking.Repositories;
using Ticket_Booking.Models.DomainModels;
using Ticket_Booking.Enums;
using System.Text.RegularExpressions;

namespace Ticket_Booking.Controllers
{
    public class ChatController : Controller
    {
        private readonly IAiChatService _aiChatService;
        private readonly TripRepository _tripRepository;

        public ChatController(IAiChatService aiChatService, TripRepository tripRepository)
        {
            _aiChatService = aiChatService;
            _tripRepository = tripRepository;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Send(string message, string? conversationId)
        {
            var userId = HttpContext.Session.GetInt32("UserId"); // optional; allow anonymous for chat

            var convId = string.IsNullOrWhiteSpace(conversationId)
                ? Guid.NewGuid().ToString("N")
                : conversationId;

            // Check if the message is asking about flights
            bool isFlightRelated = IsFlightRelatedQuery(message);
            
            // Only get trips if user is asking about flights
            List<Trip> tripsForAI = new List<Trip>();
            List<Trip> tripsForDisplay = new List<Trip>();
            SearchParams? searchParams = null;

            if (isFlightRelated)
            {
                // Get all available trips (upcoming flights only)
                var allTrips = (await _tripRepository.GetAllAsync())
                    .Where(t => t.DepartureTime > DateTime.Now)
                    .OrderBy(t => t.DepartureTime)
                    .ToList();

                // Try to extract flight search parameters from message
                searchParams = ExtractSearchParams(message);

                if (searchParams != null && (!string.IsNullOrEmpty(searchParams.FromCity) || !string.IsNullOrEmpty(searchParams.ToCity)))
                {
                    // Filter trips based on search params - STRICT matching
                    tripsForAI = allTrips
                        .Where(t => 
                            (string.IsNullOrEmpty(searchParams.FromCity) || MatchCity(t.FromCity, searchParams.FromCity)) &&
                            (string.IsNullOrEmpty(searchParams.ToCity) || MatchCity(t.ToCity, searchParams.ToCity)) &&
                            (!searchParams.Date.HasValue || t.DepartureTime.Date == searchParams.Date.Value.Date))
                        .OrderBy(t => t.DepartureTime)
                        .Take(20) // Limit to 20 for AI context
                        .ToList();
                    
                    tripsForDisplay = tripsForAI.Take(10).ToList();
                }
                else
                {
                    // If asking about flights but no specific search params, don't send trips
                    // Let AI handle general flight questions without showing all trips
                    tripsForAI = new List<Trip>();
                    tripsForDisplay = new List<Trip>();
                }
            }

            // Get AI reply - only send trips if flight-related query
            var reply = await _aiChatService.GetReplyAsync(convId, message, tripsForAI, searchParams);

            return Json(new
            {
                conversationId = convId,
                reply,
                trips = tripsForDisplay.Select(t => new
                {
                    id = t.Id,
                    fromCity = t.FromCity,
                    toCity = t.ToCity,
                    departureTime = t.DepartureTime.ToString("dd/MM/yyyy HH:mm"),
                    arrivalTime = t.ArrivalTime.ToString("dd/MM/yyyy HH:mm"),
                    companyName = t.Company?.Name ?? "Unknown",
                    economyPrice = t.EconomyPrice,
                    businessPrice = t.BusinessPrice,
                    firstClassPrice = t.FirstClassPrice,
                    economySeats = t.EconomySeats,
                    businessSeats = t.BusinessSeats,
                    firstClassSeats = t.FirstClassSeats
                }).ToList()
            });
        }

        private bool IsFlightRelatedQuery(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return false;

            var lowerMessage = message.ToLower();
            
            // Keywords that indicate flight-related queries
            var flightKeywords = new[]
            {
                "chuyến bay", "chuyen bay", "flight", "flights",
                "đặt vé", "dat ve", "book", "booking", "đặt chuyến", "dat chuyen",
                "tìm chuyến", "tim chuyen", "find flight", "search flight",
                "vé máy bay", "ve may bay", "ticket", "tickets",
                "bay", "fly", "flying",
                "hà nội", "ha noi", "hanoi", "sài gòn", "sai gon", "ho chi minh", "hcm",
                "đà nẵng", "da nang", "danang", "nha trang", "phú quốc", "phu quoc",
                "từ", "to", "đến", "den", "tới", "toi", "from"
            };

            // Check if message contains any flight-related keywords
            return flightKeywords.Any(keyword => lowerMessage.Contains(keyword));
        }

        private bool MatchCity(string dbCity, string searchCity)
        {
            if (string.IsNullOrEmpty(dbCity) || string.IsNullOrEmpty(searchCity))
                return false;

            // Normalize both for comparison
            var dbLower = dbCity.ToLower().Trim();
            var searchLower = searchCity.ToLower().Trim();
            
            var dbNormalized = NormalizeCityName(dbLower);
            var searchNormalized = NormalizeCityName(searchLower);

            // Exact match after normalization (most reliable)
            if (dbNormalized.Equals(searchNormalized, StringComparison.OrdinalIgnoreCase))
                return true;

            // Contains match (handle "Ho Chi Minh City" vs "Ho Chi Minh" or "Sài Gòn")
            if (dbLower.Contains(searchLower) || searchLower.Contains(dbLower))
                return true;
            
            // Also check normalized versions for contains
            if (dbNormalized.ToLower().Contains(searchNormalized.ToLower()) || 
                searchNormalized.ToLower().Contains(dbNormalized.ToLower()))
                return true;

            // Check if both normalize to same city (e.g., "hcm" -> "Ho Chi Minh City", "sài gòn" -> "Ho Chi Minh City")
            // Extract key words for comparison
            var dbKeyWords = ExtractCityKeywords(dbLower);
            var searchKeyWords = ExtractCityKeywords(searchLower);
            
            // Check if any keywords match
            if (dbKeyWords.Any(kw => searchKeyWords.Contains(kw)) || 
                searchKeyWords.Any(kw => dbKeyWords.Contains(kw)))
                return true;
            
            // Final check: if normalized names are the same, they match
            return dbNormalized.Equals(searchNormalized, StringComparison.OrdinalIgnoreCase);
        }

        private List<string> ExtractCityKeywords(string city)
        {
            var keywords = new List<string>();
            var normalized = NormalizeCityName(city);
            
            // Add normalized name
            keywords.Add(normalized.ToLower());
            
            // Add individual words
            foreach (var word in normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                if (word.Length > 2)
                    keywords.Add(word.ToLower());
            }
            
            // Add common aliases
            var aliasMap = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
            {
                { "ho chi minh city", new List<string> { "hcm", "sai gon", "sài gòn", "ho chi minh" } },
                { "ha noi", new List<string> { "hanoi", "hà nội" } },
                { "da nang", new List<string> { "danang", "đà nẵng" } }
            };
            
            if (aliasMap.TryGetValue(normalized.ToLower(), out var aliases))
            {
                keywords.AddRange(aliases);
            }
            
            return keywords.Distinct().ToList();
        }

        private SearchParams? ExtractSearchParams(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return null;

            var lowerMessage = message.ToLower();
            
            // Common Vietnamese city names
            var cities = new[] { "hà nội", "ha noi", "hanoi", "ho chi minh", "hồ chí minh", "hcm", "sài gòn", "sai gon", 
                                "đà nẵng", "da nang", "danang", "nha trang", "nha trang", "phú quốc", "phu quoc", 
                                "đà lạt", "dalat", "huế", "hue", "cần thơ", "can tho" };

            string? fromCity = null;
            string? toCity = null;
            DateTime? date = null;

            // Try to find "từ X đến Y" or "from X to Y" or "X -> Y" or "X to Y"
            // Use simpler pattern that matches any text between keywords
            var fromToPatterns = new[]
            {
                @"(?:từ|from)\s+(.+?)\s+(?:đến|to|->|tới)\s+(.+?)(?:\s|$|\.|,|!|\?)",
                @"(.+?)\s+(?:đến|to|->|tới)\s+(.+?)(?:\s|$|\.|,|!|\?)",
                @"(.+?)\s+-\s+(.+?)(?:\s|$|\.|,|!|\?)"
            };

            foreach (var pattern in fromToPatterns)
            {
                var match = Regex.Match(lowerMessage, pattern, RegexOptions.IgnoreCase);
                if (match.Success && match.Groups.Count >= 3)
                {
                    var from = match.Groups[1].Value.Trim();
                    var to = match.Groups[2].Value.Trim();
                    
                    // Validate these are likely city names (not common words)
                    // Trim and clean up the extracted text
                    from = from.Trim().TrimEnd(',', '.', '!', '?');
                    to = to.Trim().TrimEnd(',', '.', '!', '?');
                    
                    if (from.Length > 2 && to.Length > 2)
                    {
                        fromCity = NormalizeCityName(from);
                        toCity = NormalizeCityName(to);
                        
                        // Only break if both cities were successfully normalized (not empty)
                        if (!string.IsNullOrEmpty(fromCity) && !string.IsNullOrEmpty(toCity))
                        {
                            break;
                        }
                    }
                }
            }

            // If pattern matching didn't work, try to find city pairs in message
            if (fromCity == null || toCity == null)
            {
                var foundCities = new List<string>();
                foreach (var city in cities)
                {
                    if (lowerMessage.Contains(city))
                    {
                        foundCities.Add(city);
                    }
                }
                
                // Also try to find cities by checking normalized names using regex patterns
                // This handles Vietnamese city names with special characters
                var cityPatterns = new Dictionary<string, string>
                {
                    { @"\bhà\s+nội\b", "Ha Noi" },
                    { @"\bha\s+noi\b", "Ha Noi" },
                    { @"\bhanoi\b", "Ha Noi" },
                    { @"\bsài\s+gòn\b", "Ho Chi Minh City" },
                    { @"\bsai\s+gon\b", "Ho Chi Minh City" },
                    { @"\bhồ\s+chí\s+minh\b", "Ho Chi Minh City" },
                    { @"\bho\s+chi\s+minh\b", "Ho Chi Minh City" },
                    { @"\bho\s+chi\s+minh\s+city\b", "Ho Chi Minh City" },
                    { @"\bhcm\b", "Ho Chi Minh City" },
                    { @"\bđà\s+nẵng\b", "Da Nang" },
                    { @"\bda\s+nang\b", "Da Nang" },
                    { @"\bdanang\b", "Da Nang" },
                    { @"\bnha\s+trang\b", "Nha Trang" },
                    { @"\bphú\s+quốc\b", "Phu Quoc" },
                    { @"\bphu\s+quoc\b", "Phu Quoc" }
                };
                
                // Find all matching cities in order of appearance
                var cityMatches = new List<(int position, string city)>();
                foreach (var pattern in cityPatterns)
                {
                    var matches = Regex.Matches(lowerMessage, pattern.Key, RegexOptions.IgnoreCase);
                    foreach (Match match in matches)
                    {
                        cityMatches.Add((match.Index, pattern.Value));
                    }
                }
                
                // Sort by position and add unique cities
                if (cityMatches.Count > 0)
                {
                    var sortedMatches = cityMatches.OrderBy(m => m.position).Select(m => m.city).Distinct().ToList();
                    foundCities.AddRange(sortedMatches);
                }
                
                if (foundCities.Count >= 2)
                {
                    fromCity = foundCities[0];
                    toCity = foundCities[1];
                }
                else if (foundCities.Count == 1)
                {
                    // If only one city found, try to infer from context
                    // Check if message has "từ" or "from" - then it's fromCity
                    // Check if message has "đến", "to", "->" - then it's toCity
                    if (Regex.IsMatch(lowerMessage, @"(?:từ|from)\s+", RegexOptions.IgnoreCase))
                    {
                        // Likely the found city is fromCity
                        fromCity = foundCities[0];
                    }
                    else if (Regex.IsMatch(lowerMessage, @"(?:đến|to|->|tới)\s+", RegexOptions.IgnoreCase))
                    {
                        // Likely the found city is toCity
                        toCity = foundCities[0];
                    }
                    else
                    {
                        // Default: assume it's toCity (more common in queries)
                        toCity = foundCities[0];
                    }
                }
            }

            // Try to extract date
            var datePatterns = new[]
            {
                @"(\d{1,2})[\/\-](\d{1,2})[\/\-](\d{4})", // dd/mm/yyyy
                @"(\d{4})[\/\-](\d{1,2})[\/\-](\d{1,2})", // yyyy/mm/dd
                @"(ngày|date|hôm|tomorrow|ngày mai)\s+(\d{1,2})[\/\-](\d{1,2})", // ngày dd/mm
            };

            foreach (var pattern in datePatterns)
            {
                var dateMatch = Regex.Match(lowerMessage, pattern, RegexOptions.IgnoreCase);
                if (dateMatch.Success)
                {
                    try
                    {
                        if (dateMatch.Groups.Count == 4)
                        {
                            if (int.Parse(dateMatch.Groups[3].Value) > 1000) // yyyy/mm/dd
                                date = new DateTime(int.Parse(dateMatch.Groups[3].Value), 
                                    int.Parse(dateMatch.Groups[2].Value), 
                                    int.Parse(dateMatch.Groups[1].Value));
                            else // dd/mm/yyyy
                                date = new DateTime(int.Parse(dateMatch.Groups[3].Value), 
                                    int.Parse(dateMatch.Groups[2].Value), 
                                    int.Parse(dateMatch.Groups[1].Value));
                        }
                        else if (dateMatch.Groups.Count == 4 && lowerMessage.Contains("tomorrow") || lowerMessage.Contains("ngày mai"))
                        {
                            date = DateTime.Now.AddDays(1);
                        }
                    }
                    catch { }
                    break;
                }
            }

            // Don't set default date - only filter by date if user explicitly provides one
            // This allows finding flights on any available date
            // if (!date.HasValue)
            // {
            //     date = DateTime.Now.Date;
            // }

            if (fromCity != null && toCity != null)
            {
                return new SearchParams
                {
                    FromCity = fromCity,
                    ToCity = toCity,
                    Date = date
                };
            }

            return null;
        }

        private string NormalizeCityName(string city)
        {
            if (string.IsNullOrWhiteSpace(city))
                return string.Empty;

            var cityLower = city.ToLower().Trim();
            
            var cityMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "hà nội", "Ha Noi" },
                { "ha noi", "Ha Noi" },
                { "hanoi", "Ha Noi" },
                { "ho chi minh", "Ho Chi Minh City" },
                { "hồ chí minh", "Ho Chi Minh City" },
                { "hcm", "Ho Chi Minh City" },
                { "sài gòn", "Ho Chi Minh City" },
                { "sai gon", "Ho Chi Minh City" },
                { "ho chi minh city", "Ho Chi Minh City" },
                { "đà nẵng", "Da Nang" },
                { "da nang", "Da Nang" },
                { "danang", "Da Nang" },
                { "nha trang", "Nha Trang" },
                { "phú quốc", "Phu Quoc" },
                { "phu quoc", "Phu Quoc" },
                { "đà lạt", "Dalat" },
                { "dalat", "Dalat" },
                { "huế", "Hue" },
                { "hue", "Hue" },
                { "cần thơ", "Can Tho" },
                { "can tho", "Can Tho" }
            };

            // Try exact match first
            if (cityMap.TryGetValue(cityLower, out var normalized))
                return normalized;

            // Try partial match
            foreach (var kvp in cityMap)
            {
                if (cityLower.Contains(kvp.Key) || kvp.Key.Contains(cityLower))
                    return kvp.Value;
            }

            // If no match, return capitalized version
            return char.ToUpper(city[0]) + city.Substring(1).ToLower();
        }

        private class SearchParams
        {
            public string FromCity { get; set; } = string.Empty;
            public string ToCity { get; set; } = string.Empty;
            public DateTime? Date { get; set; }
        }
    }
}


