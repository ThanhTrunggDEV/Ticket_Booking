using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ticket_Booking.Interfaces;
using Ticket_Booking.Models.DomainModels;

namespace Ticket_Booking.Services
{
    /// <summary>
    /// Minimal Gemini 2.5 Flash client for chat-style interactions.
    /// Uses configuration:
    ///   "Gemini:ApiKey"
    ///   "Gemini:Model" (default: "gemini-2.5-flash")
    ///   "Gemini:Endpoint" (optional override)
    /// </summary>
    public class GeminiChatService : IAiChatService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _model;
        private readonly string _endpoint;
        private readonly ILogger<GeminiChatService> _logger;

        public GeminiChatService(IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<GeminiChatService> logger)
        {
            _httpClient = httpClientFactory.CreateClient(nameof(GeminiChatService));
            _logger = logger;

            _apiKey = configuration["Gemini:ApiKey"] ?? string.Empty;
            _model = configuration["Gemini:Model"] ?? "gemini-2.5-flash";
            _endpoint = configuration["Gemini:Endpoint"]
                        ?? $"https://generativelanguage.googleapis.com/v1beta/models/{{model}}:generateContent";

            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                throw new InvalidOperationException("Gemini API key is not configured. Please set Gemini:ApiKey in appsettings.");
            }
        }

        public async Task<string> GetReplyAsync(string conversationId, string userMessage, IEnumerable<Trip>? availableTrips = null, object? searchParams = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userMessage))
            {
                return "Xin hãy nhập nội dung câu hỏi về chuyến bay.";
            }

            var url = _endpoint.Replace("{model}", _model, StringComparison.OrdinalIgnoreCase);
            var uri = $"{url}?key={_apiKey}";

            var requestPayload = new
            {
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[]
                        {
                            new { text = BuildSystemPrompt(conversationId, userMessage, availableTrips, searchParams) }
                        }
                    }
                }
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, uri)
            {
                Content = JsonContent.Create(requestPayload)
            };

            try
            {
                using var response = await _httpClient.SendAsync(request, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogError("Gemini API error: StatusCode={StatusCode}, Response={ErrorContent}", 
                        response.StatusCode, errorContent);
                    
                    // Try to parse error details if available
                    try
                    {
                        var errorJson = JsonDocument.Parse(errorContent);
                        if (errorJson.RootElement.TryGetProperty("error", out var errorObj))
                        {
                            if (errorObj.TryGetProperty("message", out var message))
                            {
                                _logger.LogError("Gemini API error message: {Message}", message.GetString());
                            }
                        }
                    }
                    catch { }
                    
                    return "Xin lỗi, hệ thống chat AI đang gặp lỗi. Vui lòng thử lại sau.";
                }

                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogInformation("Gemini API response received. Length: {Length}", responseContent.Length);
                
                using var json = JsonDocument.Parse(responseContent);
                var root = json.RootElement;
                
                // Log full response for debugging (first 500 chars to avoid log spam)
                var preview = responseContent.Length > 500 ? responseContent.Substring(0, 500) + "..." : responseContent;
                _logger.LogDebug("Gemini API response preview: {Preview}", preview);
                
                // Check for error in response
                if (root.TryGetProperty("error", out var errorElement))
                {
                    var errorMessage = errorElement.TryGetProperty("message", out var msg) 
                        ? msg.GetString() 
                        : "Unknown error";
                    var errorCode = errorElement.TryGetProperty("code", out var code) 
                        ? code.GetInt32() 
                        : 0;
                    _logger.LogError("Gemini API returned error: Code={ErrorCode}, Message={ErrorMessage}", 
                        errorCode, errorMessage);
                    return "Xin lỗi, hệ thống chat AI đang gặp lỗi. Vui lòng thử lại sau.";
                }
                
                if (!root.TryGetProperty("candidates", out var candidates) || candidates.GetArrayLength() == 0)
                {
                    _logger.LogWarning("Gemini API response has no candidates. Response keys: {Keys}, Full response: {Response}", 
                        string.Join(", ", root.EnumerateObject().Select(p => p.Name)), 
                        responseContent.Length > 1000 ? responseContent.Substring(0, 1000) + "..." : responseContent);
                    return "Xin lỗi, hệ thống chat AI đang gặp lỗi. Vui lòng thử lại sau.";
                }

                var candidate = candidates[0];
                
                // Check if candidate has finishReason that indicates an error
                if (candidate.TryGetProperty("finishReason", out var finishReason))
                {
                    var reason = finishReason.GetString();
                    if (reason != "STOP" && !string.IsNullOrEmpty(reason))
                    {
                        _logger.LogWarning("Gemini API finish reason: {FinishReason}", reason);
                        if (reason == "SAFETY" || reason == "RECITATION")
                        {
                            return "Xin lỗi, câu hỏi của bạn không thể được xử lý do vi phạm chính sách an toàn.";
                        }
                    }
                }
                
                if (!candidate.TryGetProperty("content", out var content))
                {
                    _logger.LogWarning("Gemini API candidate has no content. Candidate: {Candidate}", 
                        candidate.GetRawText());
                    return "Xin lỗi, hệ thống chat AI đang gặp lỗi. Vui lòng thử lại sau.";
                }
                
                if (!content.TryGetProperty("parts", out var parts) || parts.GetArrayLength() == 0)
                {
                    _logger.LogWarning("Gemini API content has no parts. Content: {Content}", 
                        content.GetRawText());
                    return "Xin lỗi, hệ thống chat AI đang gặp lỗi. Vui lòng thử lại sau.";
                }

                var firstPart = parts[0];
                if (!firstPart.TryGetProperty("text", out var textElement))
                {
                    _logger.LogWarning("Gemini API part has no text. Part: {Part}", 
                        firstPart.GetRawText());
                    return "Xin lỗi, hệ thống chat AI đang gặp lỗi. Vui lòng thử lại sau.";
                }

                var aiReply = textElement.GetString() ?? string.Empty;
                
                // Only append formatted trip list if we have search params (filtered trips)
                // This ensures we only show relevant trips that match the user's query
                if (searchParams != null && availableTrips != null && availableTrips.Any())
                {
                    // Extract search params using reflection
                    var fromCity = GetSearchParamValue<string>(searchParams, "FromCity");
                    var toCity = GetSearchParamValue<string>(searchParams, "ToCity");
                    var date = GetSearchParamValue<DateTime?>(searchParams, "Date");
                    
                    // Double-check filtering: only show trips that actually match the search criteria
                    // This is a safety measure in case the controller filtering didn't work perfectly
                    var relevantTrips = availableTrips
                        .Where(t => 
                        {
                            bool fromMatches = string.IsNullOrEmpty(fromCity) || MatchCity(t.FromCity, fromCity);
                            bool toMatches = string.IsNullOrEmpty(toCity) || MatchCity(t.ToCity, toCity);
                            bool dateMatches = !date.HasValue || t.DepartureTime.Date == date.Value.Date;
                            return fromMatches && toMatches && dateMatches;
                        })
                        .Take(5)
                        .ToList();
                    
                    // Only append if we have matching trips
                    if (relevantTrips.Any())
                    {
                        aiReply += "\n\n📋 **Các chuyến bay tìm được:**\n";
                        int index = 1;
                        foreach (var trip in relevantTrips)
                        {
                            aiReply += $"\n{index}. {trip.FromCity} → {trip.ToCity}\n";
                            aiReply += $"   ⏰ {trip.DepartureTime:dd/MM/yyyy HH:mm} - {trip.ArrivalTime:HH:mm}\n";
                            aiReply += $"   ✈️ {trip.Company?.Name ?? "Unknown"}\n";
                            aiReply += $"   💰 Economy: ${trip.EconomyPrice:F2} | Business: ${trip.BusinessPrice:F2} | First: ${trip.FirstClassPrice:F2}\n";
                            aiReply += $"   🎫 ID: {trip.Id}\n";
                            index++;
                        }
                        aiReply += "\n💡 Bạn có thể nói 'Đặt vé số X' hoặc 'Book chuyến số X' để đặt vé.";
                    }
                }
                // If no search params, don't append trip list - let AI handle it in the response
                    
                return aiReply;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON parsing error when processing Gemini API response");
                return "Xin lỗi, hệ thống chat AI đang gặp lỗi. Vui lòng thử lại sau.";
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request error when calling Gemini API");
                return "Xin lỗi, không thể kết nối đến hệ thống AI. Vui lòng kiểm tra kết nối mạng và thử lại sau.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in GeminiChatService");
                return "Xin lỗi, hệ thống chat AI đang gặp lỗi. Vui lòng thử lại sau.";
            }
        }

        private static string BuildSystemPrompt(string conversationId, string userMessage, IEnumerable<Trip>? availableTrips, object? searchParams)
        {
            var prompt = 
                "Bạn là trợ lý đặt vé máy bay thân thiện cho website Ticket_Booking. " +
                "Hãy trả lời ngắn gọn, rõ ràng bằng tiếng Việt.\n\n";

            // Only include flight data if we have search params (user is asking about specific flights)
            if (searchParams != null && availableTrips != null && availableTrips.Any())
            {
                prompt += "Bạn CHỈ được đề xuất các chuyến bay trong danh sách dưới đây, " +
                         "KHÔNG được tự bịa số hiệu hay giá vé.\n\n";
                
                var tripsList = availableTrips.ToList();
                
                // If search params exist, emphasize that these are the RELEVANT trips
                if (searchParams != null)
                {
                    var fromCity = GetSearchParamValue<string>(searchParams, "FromCity");
                    var toCity = GetSearchParamValue<string>(searchParams, "ToCity");
                    var date = GetSearchParamValue<DateTime?>(searchParams, "Date");
                    
                    var fromCityStr = !string.IsNullOrEmpty(fromCity) ? fromCity : "bất kỳ";
                    var toCityStr = !string.IsNullOrEmpty(toCity) ? toCity : "bất kỳ";
                    var dateStr = date.HasValue ? date.Value.ToString("dd/MM/yyyy") : "bất kỳ";
                    
                    prompt += $"**YÊU CẦU TÌM KIẾM:** Từ {fromCityStr} đến {toCityStr}, ngày {dateStr}\n\n";
                    prompt += "**CÁC CHUYẾN BAY PHÙ HỢP VỚI YÊU CẦU (CHỈ ĐƯỢC ĐỀ XUẤT CÁC CHUYẾN BAY NÀY):**\n\n";
                }
                else
                {
                    prompt += "**DỮ LIỆU CHUYẾN BAY CÓ SẴN:**\n\n";
                }
                
                int tripCount = 0;
                foreach (var trip in tripsList)
                {
                    prompt += $"ID {trip.Id}: {trip.FromCity} → {trip.ToCity}\n";
                    prompt += $"  - Thời gian: {trip.DepartureTime:dd/MM/yyyy HH:mm} - {trip.ArrivalTime:HH:mm}\n";
                    prompt += $"  - Hãng hàng không: {trip.Company?.Name ?? "Unknown"}\n";
                    prompt += $"  - Giá vé: Economy ${trip.EconomyPrice:F2} | Business ${trip.BusinessPrice:F2} | First Class ${trip.FirstClassPrice:F2}\n";
                    prompt += $"  - Ghế còn lại: Economy {trip.EconomySeats} | Business {trip.BusinessSeats} | First {trip.FirstClassSeats}\n\n";
                    
                    tripCount++;
                    if (tripCount >= 30) // Reduced limit for better focus
                    {
                        prompt += $"... và còn {tripsList.Count - 30} chuyến bay khác.\n\n";
                        break;
                    }
                }
                
                prompt += "\n**QUAN TRỌNG - HƯỚNG DẪN TRẢ LỜI (TUÂN THỦ NGHIÊM NGẶT):**\n";
                if (searchParams != null)
                {
                    prompt += "- Bạn CHỈ được đề xuất và nhắc đến CÁC CHUYẾN BAY TRONG DANH SÁCH TRÊN (đã được lọc chính xác theo yêu cầu).\n";
                    prompt += "- TUYỆT ĐỐI KHÔNG được đề xuất, nhắc đến, hoặc tạo ra bất kỳ chuyến bay nào KHÔNG CÓ trong danh sách trên.\n";
                    prompt += "- Nếu danh sách trống hoặc không có chuyến bay phù hợp, hãy thông báo: 'Không tìm thấy chuyến bay phù hợp với yêu cầu của bạn.'\n";
                    prompt += "- Khi liệt kê chuyến bay, CHỈ liệt kê các chuyến bay trong danh sách trên, không thêm bất kỳ chuyến bay nào khác.\n";
                }
                else
                {
                    prompt += "- Khi người dùng hỏi về chuyến bay cụ thể, hãy tìm trong danh sách trên.\n";
                    prompt += "- Nếu người dùng hỏi về tuyến đường, hãy CHỈ đề xuất các chuyến bay có tuyến đường đó trong danh sách.\n";
                    prompt += "- KHÔNG được tự tạo hoặc bịa ra thông tin chuyến bay không có trong danh sách.\n";
                }
                prompt += "- Luôn cung cấp thông tin chính xác từ dữ liệu (ID, giá, thời gian, thành phố).\n";
                prompt += "- Đề xuất người dùng đặt vé bằng cách nói ID chuyến bay cụ thể.\n\n";
            }
            else
            {
                prompt += "**KHÔNG CÓ CHUYẾN BAY PHÙ HỢP:**\n";
                prompt += "Hiện tại không tìm thấy chuyến bay nào phù hợp với yêu cầu. " +
                         "Hãy thông báo cho người dùng một cách lịch sự và đề nghị họ thử tìm kiếm với tiêu chí khác.\n\n";
            }

            prompt += $"ConversationId: {conversationId}\n\n";
            prompt += $"Câu hỏi của người dùng:\n{userMessage}";

            return prompt;
        }

        private static T? GetSearchParamValue<T>(object? searchParams, string propertyName)
        {
            if (searchParams == null)
                return default;

            var property = searchParams.GetType().GetProperty(propertyName);
            if (property != null)
            {
                var value = property.GetValue(searchParams);
                if (value is T typedValue)
                    return typedValue;
            }

            return default;
        }

        private static bool MatchCity(string dbCity, string searchCity)
        {
            if (string.IsNullOrEmpty(dbCity) || string.IsNullOrEmpty(searchCity))
                return false;

            var dbLower = dbCity.ToLower().Trim();
            var searchLower = searchCity.ToLower().Trim();

            // Normalize city names for better matching
            var normalizedDb = NormalizeCityForMatching(dbLower);
            var normalizedSearch = NormalizeCityForMatching(searchLower);

            // Exact match after normalization
            if (normalizedDb.Equals(normalizedSearch, StringComparison.OrdinalIgnoreCase))
                return true;

            // Contains match
            if (normalizedDb.Contains(normalizedSearch) || normalizedSearch.Contains(normalizedDb))
                return true;

            // Check for common aliases
            var aliasMap = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
            {
                { "ho chi minh city", new List<string> { "hcm", "sai gon", "sài gòn", "ho chi minh", "hồ chí minh" } },
                { "ha noi", new List<string> { "hanoi", "hà nội" } },
                { "da nang", new List<string> { "danang", "đà nẵng" } }
            };

            foreach (var kvp in aliasMap)
            {
                if ((normalizedDb.Contains(kvp.Key) || kvp.Key.Contains(normalizedDb)) &&
                    kvp.Value.Any(alias => normalizedSearch.Contains(alias) || alias.Contains(normalizedSearch)))
                    return true;
            }

            return false;
        }

        private static string NormalizeCityForMatching(string city)
        {
            if (string.IsNullOrEmpty(city))
                return string.Empty;

            var normalized = city.ToLower().Trim();
            
            // Remove common suffixes
            normalized = normalized.Replace(" city", "").Trim();
            
            return normalized;
        }
    }
}


