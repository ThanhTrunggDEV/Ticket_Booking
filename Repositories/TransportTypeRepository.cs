using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Ticket_Booking.Data;
using Ticket_Booking.Interfaces;
using Ticket_Booking.Models.DomainModels;

namespace Ticket_Booking.Repositories
{
    public class TransportTypeRepository : IRepository<TransportType>
    {
        private readonly AppDbContext _context;
        private readonly DbSet<TransportType> _dbSet;

        public TransportTypeRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<TransportType>();
        }

        // Get operations
        public async Task<TransportType?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<IEnumerable<TransportType>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<IEnumerable<TransportType>> FindAsync(Expression<Func<TransportType, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public async Task<TransportType?> FirstOrDefaultAsync(Expression<Func<TransportType, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }

        // Add operations
        public async Task<TransportType> AddAsync(TransportType entity)
        {
            await _dbSet.AddAsync(entity);
            return entity;
        }

        public async Task AddRangeAsync(IEnumerable<TransportType> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }

        // Update operations
        public Task UpdateAsync(TransportType entity)
        {
            _dbSet.Update(entity);
            return Task.CompletedTask;
        }

        public Task UpdateRangeAsync(IEnumerable<TransportType> entities)
        {
            _dbSet.UpdateRange(entities);
            return Task.CompletedTask;
        }

        // Delete operations
        public Task DeleteAsync(TransportType entity)
        {
            _dbSet.Remove(entity);
            return Task.CompletedTask;
        }

        public Task DeleteRangeAsync(IEnumerable<TransportType> entities)
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

        public async Task<int> CountAsync(Expression<Func<TransportType, bool>> predicate)
        {
            return await _dbSet.CountAsync(predicate);
        }

        // Exists operations
        public async Task<bool> ExistsAsync(Expression<Func<TransportType, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        // Save changes
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        // TransportType-specific methods
        public async Task<TransportType?> GetByNameAsync(string name)
        {
            return await _dbSet.FirstOrDefaultAsync(t => t.Name == name);
        }

        public async Task<TransportType?> GetWithCompaniesAsync(int id)
        {
            return await _dbSet
                .Include(t => t.Companies)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<TransportType?> GetWithVehiclesAsync(int id)
        {
            return await _dbSet
                .Include(t => t.Vehicles)
                .ThenInclude(v => v.Company)
                .FirstOrDefaultAsync(t => t.Id == id);
        }
    }
}
