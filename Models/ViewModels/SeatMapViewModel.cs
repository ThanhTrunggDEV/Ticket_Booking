using Ticket_Booking.Enums;

namespace Ticket_Booking.Models.ViewModels
{
    /// <summary>
    /// View model for seat map display during check-in
    /// </summary>
    public class SeatMapViewModel
    {
        public int TripId { get; set; }
        public SeatClass SeatClass { get; set; }
        public List<SeatInfo> Seats { get; set; } = new();
        public int TotalRows { get; set; }
        public int SeatsPerRow { get; set; }
        public int TotalSeats { get; set; }
        public int AvailableSeats { get; set; }
        public int BookedSeats { get; set; }
    }

    /// <summary>
    /// Information about a specific seat
    /// </summary>
    public class SeatInfo
    {
        public string SeatNumber { get; set; } = string.Empty;  // Format: {Row}{Letter} (e.g., "12A")
        public bool IsAvailable { get; set; }
        public bool IsSelected { get; set; }  // Currently selected by user
        public int Row { get; set; }  // Row number (1-50)
        public int Column { get; set; }  // Column index (0-based)
        public string ColumnLetter { get; set; } = string.Empty;  // Letter (A-F)
        public SeatPosition Position { get; set; }  // Window, Middle, Aisle
    }

    /// <summary>
    /// Seat position within a row
    /// </summary>
    public enum SeatPosition
    {
        Window,
        Middle,
        Aisle
    }
}


