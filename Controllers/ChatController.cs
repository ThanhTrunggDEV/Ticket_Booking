using Microsoft.AspNetCore.Mvc;
using Ticket_Booking.Interfaces;
using Ticket_Booking.Repositories;
using Ticket_Booking.Models.DomainModels;
using Ticket_Booking.Enums;

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

            var reply = await _aiChatService.GetReplyAsync(convId, message);

            return Json(new
            {
                conversationId = convId,
                reply
            });
        }
    }
}


