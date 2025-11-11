using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Ticket_Booking.Data;
using Ticket_Booking.Interfaces;
using RouteModel = Ticket_Booking.Models.DomainModels.Route;

namespace Ticket_Booking.Repositories
{
    public class RouteRepository : IRepository<RouteModel>
    {
        private readonly AppDbContext _context;
        private readonly DbSet<RouteModel> _dbSet;

        public RouteRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<RouteModel>();
        }

        // Get operations
        public async Task<RouteModel?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<IEnumerable<RouteModel>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<IEnumerable<RouteModel>> FindAsync(Expression<Func<RouteModel, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public async Task<RouteModel?> FirstOrDefaultAsync(Expression<Func<RouteModel, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }

        // Add operations
        public async Task<RouteModel> AddAsync(RouteModel entity)
        {
            await _dbSet.AddAsync(entity);
            return entity;
        }

        public async Task AddRangeAsync(IEnumerable<RouteModel> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }

        // Update operations
        public Task UpdateAsync(RouteModel entity)
        {
            _dbSet.Update(entity);
            return Task.CompletedTask;
        }

        public Task UpdateRangeAsync(IEnumerable<RouteModel> entities)
        {
            _dbSet.UpdateRange(entities);
            return Task.CompletedTask;
        }

        // Delete operations
        public Task DeleteAsync(RouteModel entity)
        {
            _dbSet.Remove(entity);
            return Task.CompletedTask;
        }

        public Task DeleteRangeAsync(IEnumerable<RouteModel> entities)
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

        public async Task<int> CountAsync(Expression<Func<RouteModel, bool>> predicate)
        {
            return await _dbSet.CountAsync(predicate);
        }

        // Exists operations
        public async Task<bool> ExistsAsync(Expression<Func<RouteModel, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        // Save changes
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        // Route-specific methods
        public async Task<IEnumerable<RouteModel>> GetByFromCityAsync(string fromCity)
        {
            return await _dbSet
                .Where(r => r.FromCity.Contains(fromCity))
                .ToListAsync();
        }

        public async Task<IEnumerable<RouteModel>> GetByToCityAsync(string toCity)
        {
            return await _dbSet
                .Where(r => r.ToCity.Contains(toCity))
                .ToListAsync();
        }

        public async Task<IEnumerable<RouteModel>> SearchRoutesAsync(string fromCity, string toCity)
        {
            return await _dbSet
                .Where(r => r.FromCity.Contains(fromCity) && r.ToCity.Contains(toCity))
                .ToListAsync();
        }

        public async Task<RouteModel?> GetWithTripsAsync(int id)
        {
            return await _dbSet
                .Include(r => r.Trips)
                .ThenInclude(t => t.Vehicle)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IEnumerable<RouteModel>> GetPopularRoutesAsync(int topCount = 10)
        {
            return await _dbSet
                .Include(r => r.Trips)
                .OrderByDescending(r => r.Trips.Count)
                .Take(topCount)
                .ToListAsync();
        }
    }
}
