namespace Ticket_Booking.Interfaces
{
    public interface IAiChatService
    {
        Task<string> GetReplyAsync(string conversationId, string userMessage, CancellationToken cancellationToken = default);
    }
}

