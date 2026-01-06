using Ticket_Booking.Models.DomainModels;

namespace Ticket_Booking.Interfaces
{
    public interface IAiChatService
    {
        Task<string> GetReplyAsync(string conversationId, string userMessage, IEnumerable<Trip>? availableTrips = null, object? searchParams = null, CancellationToken cancellationToken = default);
    }
}

