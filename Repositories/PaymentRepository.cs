using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Ticket_Booking.Data;
using Ticket_Booking.Enums;
using Ticket_Booking.Interfaces;
using Ticket_Booking.Models.DomainModels;

namespace Ticket_Booking.Repositories
{
    public class PaymentRepository : IRepository<Payment>
    {
        private readonly AppDbContext _context;
        private readonly DbSet<Payment> _dbSet;

        public PaymentRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<Payment>();
        }

        // Get operations
        public async Task<Payment?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<IEnumerable<Payment>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<IEnumerable<Payment>> FindAsync(Expression<Func<Payment, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public async Task<Payment?> FirstOrDefaultAsync(Expression<Func<Payment, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }

        // Add operations
        public async Task<Payment> AddAsync(Payment entity)
        {
            await _dbSet.AddAsync(entity);
            return entity;
        }

        public async Task AddRangeAsync(IEnumerable<Payment> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }

        // Update operations
        public Task UpdateAsync(Payment entity)
        {
            _dbSet.Update(entity);
            return Task.CompletedTask;
        }

        public Task UpdateRangeAsync(IEnumerable<Payment> entities)
        {
            _dbSet.UpdateRange(entities);
            return Task.CompletedTask;
        }

        // Delete operations
        public Task DeleteAsync(Payment entity)
        {
            _dbSet.Remove(entity);
            return Task.CompletedTask;
        }

        public Task DeleteRangeAsync(IEnumerable<Payment> entities)
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

        public async Task<int> CountAsync(Expression<Func<Payment, bool>> predicate)
        {
            return await _dbSet.CountAsync(predicate);
        }

        // Exists operations
        public async Task<bool> ExistsAsync(Expression<Func<Payment, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        // Save changes
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        // Payment-specific methods
        public async Task<Payment?> GetByTicketIdAsync(int ticketId)
        {
            return await _dbSet
                .Include(p => p.Ticket)
                .ThenInclude(t => t.User)
                .FirstOrDefaultAsync(p => p.TicketId == ticketId);
        }

        public async Task<Payment?> GetByTransactionCodeAsync(string transactionCode)
        {
            return await _dbSet
                .Include(p => p.Ticket)
                .FirstOrDefaultAsync(p => p.TransactionCode == transactionCode);
        }

        public async Task<IEnumerable<Payment>> GetByStatusAsync(PaymentStatus status)
        {
            return await _dbSet
                .Where(p => p.Status == status)
                .Include(p => p.Ticket)
                .ThenInclude(t => t.User)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetByMethodAsync(PaymentMethod method)
        {
            return await _dbSet
                .Where(p => p.Method == method)
                .Include(p => p.Ticket)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            return await _dbSet
                .Where(p => p.PaymentDate >= fromDate && p.PaymentDate <= toDate)
                .Include(p => p.Ticket)
                .ThenInclude(t => t.User)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<Payment?> GetCompleteAsync(int id)
        {
            return await _dbSet
                .Include(p => p.Ticket)
                .ThenInclude(t => t.User)
                .Include(p => p.Ticket)
                .ThenInclude(t => t.Trip)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<decimal> GetTotalRevenueAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _dbSet.Where(p => p.Status == PaymentStatus.Success);

            if (fromDate.HasValue)
                query = query.Where(p => p.PaymentDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(p => p.PaymentDate <= toDate.Value);

            return await query.SumAsync(p => p.Amount);
        }

        public async Task<Dictionary<string, decimal>> GetRevenueByMethodAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _dbSet.Where(p => p.Status == PaymentStatus.Success);

            if (fromDate.HasValue)
                query = query.Where(p => p.PaymentDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(p => p.PaymentDate <= toDate.Value);

            return await query
                .GroupBy(p => p.Method)
                .Select(g => new { Method = g.Key.ToString(), Total = g.Sum(p => p.Amount) })
                .ToDictionaryAsync(x => x.Method, x => x.Total);
        }

        public async Task<IEnumerable<Payment>> GetRecentPaymentsAsync(int count = 10)
        {
            return await _dbSet
                .Include(p => p.Ticket)
                .ThenInclude(t => t.User)
                .OrderByDescending(p => p.PaymentDate)
                .Take(count)
                .ToListAsync();
        }

        public async Task<int> GetSuccessfulPaymentCountAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _dbSet.Where(p => p.Status == PaymentStatus.Success);

            if (fromDate.HasValue)
                query = query.Where(p => p.PaymentDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(p => p.PaymentDate <= toDate.Value);

            return await query.CountAsync();
        }

        public Task<Payment?> GetByEmailAsync(string email)
        {
            throw new NotImplementedException();
        }
    }
}
