using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Ticket_Booking.Data;
using Ticket_Booking.Interfaces;
using Ticket_Booking.Models.DomainModels;

namespace Ticket_Booking.Repositories
{
    public class CompanyRepository : IRepository<Company>
    {
        private readonly AppDbContext _context;
        private readonly DbSet<Company> _dbSet;

        public CompanyRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<Company>();
        }

        // Get operations
        public async Task<Company?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<IEnumerable<Company>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<IEnumerable<Company>> FindAsync(Expression<Func<Company, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public async Task<Company?> FirstOrDefaultAsync(Expression<Func<Company, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }

        // Add operations
        public async Task<Company> AddAsync(Company entity)
        {
            await _dbSet.AddAsync(entity);
            return entity;
        }

        public async Task AddRangeAsync(IEnumerable<Company> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }

        // Update operations
        public Task UpdateAsync(Company entity)
        {
            _dbSet.Update(entity);
            return Task.CompletedTask;
        }

        public Task UpdateRangeAsync(IEnumerable<Company> entities)
        {
            _dbSet.UpdateRange(entities);
            return Task.CompletedTask;
        }

        // Delete operations
        public Task DeleteAsync(Company entity)
        {
            _dbSet.Remove(entity);
            return Task.CompletedTask;
        }

        public Task DeleteRangeAsync(IEnumerable<Company> entities)
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

        public async Task<int> CountAsync(Expression<Func<Company, bool>> predicate)
        {
            return await _dbSet.CountAsync(predicate);
        }

        // Exists operations
        public async Task<bool> ExistsAsync(Expression<Func<Company, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        // Save changes
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        // Company-specific methods
        public async Task<IEnumerable<Company>> GetByTransportTypeAsync(int transportTypeId)
        {
            return await _dbSet
                .Where(c => c.TransportTypeId == transportTypeId)
                .Include(c => c.TransportType)
                .ToListAsync();
        }

        public async Task<Company?> GetWithVehiclesAsync(int id)
        {
            return await _dbSet
                .Include(c => c.Vehicles)
                .Include(c => c.TransportType)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Company?> GetWithReviewsAsync(int id)
        {
            return await _dbSet
                .Include(c => c.Reviews)
                .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Company?> GetCompleteAsync(int id)
        {
            return await _dbSet
                .Include(c => c.TransportType)
                .Include(c => c.Vehicles)
                .Include(c => c.Reviews)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Company>> SearchByNameAsync(string name)
        {
            return await _dbSet
                .Where(c => c.Name.Contains(name))
                .Include(c => c.TransportType)
                .ToListAsync();
        }

        public async Task<double> GetAverageRatingAsync(int companyId)
        {
            var company = await _dbSet
                .Include(c => c.Reviews)
                .FirstOrDefaultAsync(c => c.Id == companyId);

            if (company == null || !company.Reviews.Any())
                return 0;

            return company.Reviews.Average(r => r.Rating);
        }

        public Task<Company?> GetByEmailAsync(string email)
        {
            throw new NotImplementedException();
        }
    }
}
