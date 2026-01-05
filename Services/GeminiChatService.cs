using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Ticket_Booking.Interfaces;

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

        public GeminiChatService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient(nameof(GeminiChatService));

            _apiKey = configuration["Gemini:ApiKey"] ?? string.Empty;
            _model = configuration["Gemini:Model"] ?? "gemini-2.5-flash";
            _endpoint = configuration["Gemini:Endpoint"]
                        ?? $"https://generativelanguage.googleapis.com/v1beta/models/{{model}}:generateContent";

            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                throw new InvalidOperationException("Gemini API key is not configured. Please set Gemini:ApiKey in appsettings.");
            }
        }

        public async Task<string> GetReplyAsync(string conversationId, string userMessage, CancellationToken cancellationToken = default)
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
                            new { text = BuildSystemPrompt(conversationId, userMessage) }
                        }
                    }
                }
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, uri)
            {
                Content = JsonContent.Create(requestPayload)
            };

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return "Xin lỗi, hệ thống chat AI đang gặp lỗi. Vui lòng thử lại sau.";
            }

            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var json = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

            var candidate = json.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0];

            if (candidate.TryGetProperty("text", out var textElement))
            {
                return textElement.GetString() ?? string.Empty;
            }

            return "Xin lỗi, mình không hiểu câu hỏi. Bạn có thể hỏi lại về việc tìm hoặc đặt chuyến bay.";
        }

        private static string BuildSystemPrompt(string conversationId, string userMessage)
        {
            // High-level instruction to steer Gemini to flight search/booking domain.
            return
                "Bạn là trợ lý đặt vé máy bay cho website Ticket_Booking. " +
                "Hãy trả lời ngắn gọn, rõ ràng bằng tiếng Việt. " +
                "Bạn chỉ có quyền đề xuất chuyến bay/giá dựa trên dữ liệu mà backend cung cấp, " +
                "không tự bịa số hiệu hay giá vé.\n\n" +
                $"ConversationId: {conversationId}\n\n" +
                $"Câu hỏi của người dùng:\n{userMessage}";
        }
    }
}


