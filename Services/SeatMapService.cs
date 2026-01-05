using Ticket_Booking.Enums;
using Ticket_Booking.Interfaces;
using Ticket_Booking.Models.DomainModels;
using Ticket_Booking.Models.ViewModels;
using Ticket_Booking.Repositories;

namespace Ticket_Booking.Services
{
    /// <summary>
    /// Service for generating seat maps and checking seat availability
    /// </summary>
    public class SeatMapService : ISeatMapService
    {
        private readonly IRepository<Trip> _tripRepository;
        private readonly IRepository<Ticket> _ticketRepository;
        
        // Seat configuration: Standard aircraft layout
        private const int ECONOMY_SEATS_PER_ROW = 6;  // A-B-C | D-E-F (3-3 configuration)
        private const int BUSINESS_SEATS_PER_ROW = 4;  // A-B | C-D (2-2 configuration)
        private const int FIRST_CLASS_SEATS_PER_ROW = 4;  // A-B | C-D (2-2 configuration)
        
        private static readonly string[] SEAT_LETTERS = { "A", "B", "C", "D", "E", "F" };

        public SeatMapService(IRepository<Trip> tripRepository, IRepository<Ticket> ticketRepository)
        {
            _tripRepository = tripRepository;
            _ticketRepository = ticketRepository;
        }

        /// <summary>
        /// Generates a seat map view model for a specific trip and seat class
        /// </summary>
        public SeatMapViewModel GetSeatMap(int tripId, SeatClass seatClass)
        {
            var trip = _tripRepository.GetByIdAsync(tripId).Result;
            if (trip == null)
            {
                throw new ArgumentException($"Trip with ID {tripId} not found.");
            }

            // Get total seats for this class
            int totalSeats = GetTotalSeatsForClass(trip, seatClass);
            int seatsPerRow = GetSeatsPerRow(seatClass);
            int totalRows = (int)Math.Ceiling((double)totalSeats / seatsPerRow);

            // Get booked seats for this trip and class
            var bookedSeats = GetBookedSeats(tripId, seatClass);
            var bookedSeatSet = new HashSet<string>(bookedSeats, StringComparer.OrdinalIgnoreCase);

            // Generate all seats
            var seats = new List<SeatInfo>();
            int seatIndex = 0;

            for (int row = 1; row <= totalRows; row++)
            {
                for (int col = 0; col < seatsPerRow && seatIndex < totalSeats; col++)
                {
                    string columnLetter = SEAT_LETTERS[col];
                    string seatNumber = $"{row}{columnLetter}";
                    
                    bool isAvailable = !bookedSeatSet.Contains(seatNumber);
                    SeatPosition position = DetermineSeatPosition(col, seatsPerRow);

                    seats.Add(new SeatInfo
                    {
                        SeatNumber = seatNumber,
                        IsAvailable = isAvailable,
                        IsSelected = false,
                        Row = row,
                        Column = col,
                        ColumnLetter = columnLetter,
                        Position = position
                    });

                    seatIndex++;
                }
            }

            return new SeatMapViewModel
            {
                TripId = tripId,
                SeatClass = seatClass,
                Seats = seats,
                TotalRows = totalRows,
                SeatsPerRow = seatsPerRow,
                TotalSeats = totalSeats,
                AvailableSeats = seats.Count(s => s.IsAvailable),
                BookedSeats = bookedSeats.Count
            };
        }

        /// <summary>
        /// Checks if a specific seat is available for a trip
        /// </summary>
        public bool IsSeatAvailable(int tripId, string seatNumber)
        {
            if (string.IsNullOrWhiteSpace(seatNumber))
                return false;

            var ticketRepository = (TicketRepository)_ticketRepository;
            return ticketRepository.IsSeatAvailableAsync(tripId, seatNumber).Result;
        }

        /// <summary>
        /// Gets a list of all available seat numbers for a trip and seat class
        /// </summary>
        public List<string> GetAvailableSeats(int tripId, SeatClass seatClass)
        {
            var seatMap = GetSeatMap(tripId, seatClass);
            return seatMap.Seats
                .Where(s => s.IsAvailable)
                .Select(s => s.SeatNumber)
                .ToList();
        }

        #region Helper Methods

        /// <summary>
        /// Gets total number of seats for a specific seat class from trip configuration
        /// </summary>
        private int GetTotalSeatsForClass(Trip trip, SeatClass seatClass)
        {
            return seatClass switch
            {
                SeatClass.Economy => trip.EconomySeats,
                SeatClass.Business => trip.BusinessSeats,
                SeatClass.FirstClass => trip.FirstClassSeats,
                _ => 0
            };
        }

        /// <summary>
        /// Gets number of seats per row for a specific seat class
        /// </summary>
        private int GetSeatsPerRow(SeatClass seatClass)
        {
            return seatClass switch
            {
                SeatClass.Economy => ECONOMY_SEATS_PER_ROW,
                SeatClass.Business => BUSINESS_SEATS_PER_ROW,
                SeatClass.FirstClass => FIRST_CLASS_SEATS_PER_ROW,
                _ => 6
            };
        }

        /// <summary>
        /// Gets list of booked seat numbers for a trip and seat class
        /// </summary>
        private List<string> GetBookedSeats(int tripId, SeatClass seatClass)
        {
            var ticketRepository = (TicketRepository)_ticketRepository;
            return ticketRepository.GetBookedSeatsAsync(tripId, seatClass).Result;
        }

        /// <summary>
        /// Determines seat position (Window, Middle, Aisle) based on column index and total seats per row
        /// </summary>
        private SeatPosition DetermineSeatPosition(int columnIndex, int seatsPerRow)
        {
            // Window seats: First and last columns (A and last letter)
            if (columnIndex == 0 || columnIndex == seatsPerRow - 1)
            {
                return SeatPosition.Window;
            }

            // Aisle seats: Middle columns (typically C and D in 6-seat config, or B and C in 4-seat config)
            // For 6 seats: A(Window) B(Middle) C(Aisle) | D(Aisle) E(Middle) F(Window)
            // For 4 seats: A(Window) B(Aisle) | C(Aisle) D(Window)
            if (seatsPerRow == 6)
            {
                // Columns 2 (C) and 3 (D) are aisles
                if (columnIndex == 2 || columnIndex == 3)
                    return SeatPosition.Aisle;
                // Columns 1 (B) and 4 (E) are middle
                return SeatPosition.Middle;
            }
            else if (seatsPerRow == 4)
            {
                // Columns 1 (B) and 2 (C) are aisles
                if (columnIndex == 1 || columnIndex == 2)
                    return SeatPosition.Aisle;
                // Should not happen, but fallback
                return SeatPosition.Middle;
            }

            // Default: middle for other configurations
            return SeatPosition.Middle;
        }

        #endregion
    }
}


