using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Ticket_Booking.Data;
using Ticket_Booking.Enums;
using Ticket_Booking.Interfaces;
using Ticket_Booking.Models.DomainModels;

namespace Ticket_Booking.Repositories
{
    public class TicketRepository : IRepository<Ticket>
    {
        private readonly AppDbContext _context;
        private readonly DbSet<Ticket> _dbSet;

        public TicketRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<Ticket>();
        }

        // Get operations
        public async Task<Ticket?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<IEnumerable<Ticket>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<IEnumerable<Ticket>> FindAsync(Expression<Func<Ticket, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public async Task<Ticket?> FirstOrDefaultAsync(Expression<Func<Ticket, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }

        // Add operations
        public async Task<Ticket> AddAsync(Ticket entity)
        {
            await _dbSet.AddAsync(entity);
            return entity;
        }

        public async Task AddRangeAsync(IEnumerable<Ticket> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }

        // Update operations
        public Task UpdateAsync(Ticket entity)
        {
            _dbSet.Update(entity);
            return Task.CompletedTask;
        }

        public Task UpdateRangeAsync(IEnumerable<Ticket> entities)
        {
            _dbSet.UpdateRange(entities);
            return Task.CompletedTask;
        }

        // Delete operations
        public Task DeleteAsync(Ticket entity)
        {
            _dbSet.Remove(entity);
            return Task.CompletedTask;
        }

        public Task DeleteRangeAsync(IEnumerable<Ticket> entities)
        {
            _dbSet.RemoveRange(entities);
            return Task.CompletedTask;
        }

        public async Task DeleteByIdAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
            }
        }

        // Count operations
        public async Task<int> CountAsync()
        {
            return await _dbSet.CountAsync();
        }

        public async Task<int> CountAsync(Expression<Func<Ticket, bool>> predicate)
        {
            return await _dbSet.CountAsync(predicate);
        }

        // Exists operations
        public async Task<bool> ExistsAsync(Expression<Func<Ticket, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        // Save changes
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        // Ticket-specific methods
        public async Task<IEnumerable<Ticket>> GetByUserAsync(int userId)
        {
            return await _dbSet
                .Where(t => t.UserId == userId)
                .Include(t => t.Trip)
                .ThenInclude(tr => tr.Company)
                .OrderByDescending(t => t.BookingDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Ticket>> GetByTripAsync(int tripId)
        {
            return await _dbSet
                .Where(t => t.TripId == tripId)
                .Include(t => t.User)
                .OrderBy(t => t.SeatNumber)
                .ToListAsync();
        }

        public async Task<IEnumerable<Ticket>> GetByPaymentStatusAsync(PaymentStatus paymentStatus)
        {
            return await _dbSet
                .Where(t => t.PaymentStatus == paymentStatus)
                .Include(t => t.Trip)
                .Include(t => t.User)
                .ToListAsync();
        }

        public async Task<Ticket?> GetByQrCodeAsync(string qrCode)
        {
            return await _dbSet
                .Include(t => t.Trip)
                .ThenInclude(tr => tr.Company)
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.QrCode == qrCode);
        }

        public async Task<Ticket?> GetCompleteAsync(int id)
        {
            return await _dbSet
                .Include(t => t.Trip)
                .ThenInclude(tr => tr.Company)
                .Include(t => t.User)
                .Include(t => t.Payment)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<bool> IsSeatAvailableAsync(int tripId, string seatNumber)
        {
            return !await _dbSet.AnyAsync(t => t.TripId == tripId && t.SeatNumber == seatNumber);
        }

        public async Task<IEnumerable<string>> GetBookedSeatsAsync(int tripId)
        {
            return await _dbSet
                .Where(t => t.TripId == tripId)
                .Select(t => t.SeatNumber)
                .ToListAsync();
        }

        public async Task<IEnumerable<Ticket>> GetUpcomingTicketsByUserAsync(int userId)
        {
            var now = DateTime.Now;
            return await _dbSet
                .Where(t => t.UserId == userId && t.Trip.DepartureTime > now)
                .Include(t => t.Trip)
                .ThenInclude(tr => tr.Company)
                .OrderBy(t => t.Trip.DepartureTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Ticket>> GetPastTicketsByUserAsync(int userId)
        {
            var now = DateTime.Now;
            return await _dbSet
                .Where(t => t.UserId == userId && t.Trip.DepartureTime <= now)
                .Include(t => t.Trip)
                .ThenInclude(tr => tr.Company)
                .OrderByDescending(t => t.Trip.DepartureTime)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalRevenueAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _dbSet.Where(t => t.PaymentStatus == PaymentStatus.Success);

            if (fromDate.HasValue)
                query = query.Where(t => t.BookingDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(t => t.BookingDate <= toDate.Value);

            return await query.SumAsync(t => t.TotalPrice);
        }

        public Task<Ticket?> GetByEmailAsync(string email)
        {
            throw new NotImplementedException();
        }

        // PNR-specific methods
        /// <summary>
        /// Gets a ticket by PNR code (case-insensitive)
        /// </summary>
        /// <param name="pnr">PNR code to search for</param>
        /// <returns>Ticket if found, null otherwise</returns>
        public async Task<Ticket?> GetByPNRAsync(string pnr)
        {
            if (string.IsNullOrWhiteSpace(pnr))
                return null;

            var upperPnr = pnr.ToUpper();
            return await _dbSet
                .Where(t => t.PNR != null && t.PNR.ToUpper() == upperPnr)
                .Include(t => t.Trip)
                .ThenInclude(tr => tr.Company)
                .Include(t => t.User)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Gets a ticket by PNR code and email (case-insensitive)
        /// Used for secure PNR lookup requiring email validation
        /// </summary>
        /// <param name="pnr">PNR code to search for</param>
        /// <param name="email">Email address to validate</param>
        /// <returns>Ticket if found and email matches, null otherwise</returns>
        public async Task<Ticket?> GetByPNRAndEmailAsync(string pnr, string email)
        {
            if (string.IsNullOrWhiteSpace(pnr) || string.IsNullOrWhiteSpace(email))
                return null;

            var upperPnr = pnr.ToUpper();
            var lowerEmail = email.ToLower();
            
            return await _dbSet
                .Where(t => t.PNR != null && 
                           t.PNR.ToUpper() == upperPnr &&
                           t.User.Email.ToLower() == lowerEmail)
                .Include(t => t.Trip)
                .ThenInclude(tr => tr.Company)
                .Include(t => t.User)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Checks if a PNR code already exists (case-insensitive)
        /// </summary>
        /// <param name="pnr">PNR code to check</param>
        /// <returns>True if PNR exists, false otherwise</returns>
        public async Task<bool> PNRExistsAsync(string pnr)
        {
            if (string.IsNullOrWhiteSpace(pnr))
                return false;

            var upperPnr = pnr.ToUpper();
            return await _dbSet
                .AnyAsync(t => t.PNR != null && t.PNR.ToUpper() == upperPnr);
        }
    }
}
