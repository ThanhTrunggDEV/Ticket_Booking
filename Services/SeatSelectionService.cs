using Microsoft.EntityFrameworkCore;
using Ticket_Booking.Data;
using Ticket_Booking.Enums;
using Ticket_Booking.Interfaces;
using Ticket_Booking.Models.DomainModels;
using Ticket_Booking.Repositories;

namespace Ticket_Booking.Services
{
    /// <summary>
    /// Service for seat selection and assignment during check-in
    /// Handles seat assignment with transaction safety to prevent race conditions
    /// </summary>
    public class SeatSelectionService : ISeatSelectionService
    {
        private readonly AppDbContext _context;
        private readonly IRepository<Ticket> _ticketRepository;
        private readonly IRepository<Trip> _tripRepository;
        private readonly ISeatMapService _seatMapService;

        public SeatSelectionService(
            AppDbContext context,
            IRepository<Ticket> ticketRepository,
            IRepository<Trip> tripRepository,
            ISeatMapService seatMapService)
        {
            _context = context;
            _ticketRepository = ticketRepository;
            _tripRepository = tripRepository;
            _seatMapService = seatMapService;
        }

        /// <summary>
        /// Assigns a seat to a ticket during check-in
        /// Uses database transaction to prevent race conditions
        /// </summary>
        public async Task<bool> AssignSeatAsync(int ticketId, string seatNumber)
        {
            if (string.IsNullOrWhiteSpace(seatNumber))
                return false;

            var ticket = await _ticketRepository.GetByIdAsync(ticketId);
            if (ticket == null)
                return false;

            // Validate seat availability
            if (!ValidateSeatAvailability(ticket.TripId, seatNumber, ticket.SeatClass))
                return false;

            // Use transaction to prevent race conditions
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Reload ticket within transaction to get latest state
                var ticketInTransaction = await _context.Tickets
                    .FirstOrDefaultAsync(t => t.Id == ticketId);

                if (ticketInTransaction == null)
                {
                    await transaction.RollbackAsync();
                    return false;
                }

                // Double-check seat availability within transaction
                var ticketRepository = (TicketRepository)_ticketRepository;
                var isSeatAvailable = await ticketRepository.IsSeatAvailableAsync(ticket.TripId, seatNumber);
                if (!isSeatAvailable)
                {
                    await transaction.RollbackAsync();
                    return false;
                }

                // Assign seat
                ticketInTransaction.SeatNumber = seatNumber.ToUpper();
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        /// <summary>
        /// Changes the seat for a ticket (if already assigned)
        /// </summary>
        public async Task<bool> ChangeSeatAsync(int ticketId, string newSeatNumber)
        {
            if (string.IsNullOrWhiteSpace(newSeatNumber))
                return false;

            var ticket = await _ticketRepository.GetByIdAsync(ticketId);
            if (ticket == null)
                return false;

            // If new seat is same as current, return true (no change needed)
            if (ticket.SeatNumber.Equals(newSeatNumber, StringComparison.OrdinalIgnoreCase))
                return true;

            // Validate new seat availability
            if (!ValidateSeatAvailability(ticket.TripId, newSeatNumber, ticket.SeatClass))
                return false;

            // Use transaction to prevent race conditions
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Reload ticket within transaction
                var ticketInTransaction = await _context.Tickets
                    .FirstOrDefaultAsync(t => t.Id == ticketId);

                if (ticketInTransaction == null)
                {
                    await transaction.RollbackAsync();
                    return false;
                }

                // Double-check new seat availability within transaction
                var ticketRepository = (TicketRepository)_ticketRepository;
                var isSeatAvailable = await ticketRepository.IsSeatAvailableAsync(ticket.TripId, newSeatNumber);
                if (!isSeatAvailable)
                {
                    await transaction.RollbackAsync();
                    return false;
                }

                // Change seat
                ticketInTransaction.SeatNumber = newSeatNumber.ToUpper();
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        /// <summary>
        /// Validates if a seat is available for assignment
        /// </summary>
        public bool ValidateSeatAvailability(int tripId, string seatNumber, SeatClass seatClass)
        {
            if (string.IsNullOrWhiteSpace(seatNumber))
                return false;

            // Use SeatMapService to check availability
            return _seatMapService.IsSeatAvailable(tripId, seatNumber);
        }
    }
}

