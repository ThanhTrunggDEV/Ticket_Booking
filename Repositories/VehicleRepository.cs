using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Ticket_Booking.Data;
using Ticket_Booking.Interfaces;
using Ticket_Booking.Models.DomainModels;

namespace Ticket_Booking.Repositories
{
    public class VehicleRepository : IRepository<Vehicle>
    {
        private readonly AppDbContext _context;
        private readonly DbSet<Vehicle> _dbSet;

        public VehicleRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<Vehicle>();
        }

        // Get operations
        public async Task<Vehicle?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<IEnumerable<Vehicle>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<IEnumerable<Vehicle>> FindAsync(Expression<Func<Vehicle, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public async Task<Vehicle?> FirstOrDefaultAsync(Expression<Func<Vehicle, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }

        // Add operations
        public async Task<Vehicle> AddAsync(Vehicle entity)
        {
            await _dbSet.AddAsync(entity);
            return entity;
        }

        public async Task AddRangeAsync(IEnumerable<Vehicle> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }

        // Update operations
        public Task UpdateAsync(Vehicle entity)
        {
            _dbSet.Update(entity);
            return Task.CompletedTask;
        }

        public Task UpdateRangeAsync(IEnumerable<Vehicle> entities)
        {
            _dbSet.UpdateRange(entities);
            return Task.CompletedTask;
        }

        // Delete operations
        public Task DeleteAsync(Vehicle entity)
        {
            _dbSet.Remove(entity);
            return Task.CompletedTask;
        }

        public Task DeleteRangeAsync(IEnumerable<Vehicle> entities)
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

        public async Task<int> CountAsync(Expression<Func<Vehicle, bool>> predicate)
        {
            return await _dbSet.CountAsync(predicate);
        }

        // Exists operations
        public async Task<bool> ExistsAsync(Expression<Func<Vehicle, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        // Save changes
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        // Vehicle-specific methods
        public async Task<IEnumerable<Vehicle>> GetByCompanyAsync(int companyId)
        {
            return await _dbSet
                .Where(v => v.CompanyId == companyId)
                .Include(v => v.Company)
                .Include(v => v.TransportType)
                .ToListAsync();
        }

        public async Task<IEnumerable<Vehicle>> GetByTransportTypeAsync(int transportTypeId)
        {
            return await _dbSet
                .Where(v => v.TransportTypeId == transportTypeId)
                .Include(v => v.Company)
                .Include(v => v.TransportType)
                .ToListAsync();
        }

        public async Task<Vehicle?> GetByCodeAsync(string code)
        {
            return await _dbSet
                .Include(v => v.Company)
                .Include(v => v.TransportType)
                .FirstOrDefaultAsync(v => v.Code == code);
        }

        public async Task<Vehicle?> GetWithTripsAsync(int id)
        {
            return await _dbSet
                .Include(v => v.Company)
                .Include(v => v.TransportType)
                .Include(v => v.Trips)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<IEnumerable<Vehicle>> GetAvailableVehiclesAsync()
        {
            var now = DateTime.Now;
            return await _dbSet
                .Include(v => v.Company)
                .Include(v => v.TransportType)
                .Include(v => v.Trips)
                .Where(v => !v.Trips.Any(t => t.DepartureTime <= now && t.ArrivalTime >= now))
                .ToListAsync();
        }

        public async Task<IEnumerable<Vehicle>> SearchByNameAsync(string name)
        {
            return await _dbSet
                .Where(v => v.VehicleName.Contains(name))
                .Include(v => v.Company)
                .Include(v => v.TransportType)
                .ToListAsync();
        }

        public Task<Vehicle?> GetByEmailAsync(string email)
        {
            throw new NotImplementedException();
        }
    }
}
