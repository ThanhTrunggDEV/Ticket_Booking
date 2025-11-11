using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Ticket_Booking.Data;
using Ticket_Booking.Interfaces;
using Ticket_Booking.Models.DomainModels;

namespace Ticket_Booking.Repositories
{
    public class UserRepository : IRepository<User>
    {
        private readonly AppDbContext _context;
        private readonly DbSet<User> _dbSet;

        public UserRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<User>();
        }

 
        public async Task<User?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<IEnumerable<User>> FindAsync(Expression<Func<User, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public async Task<User?> FirstOrDefaultAsync(Expression<Func<User, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }

        public async Task<User> AddAsync(User entity)
        {
            await _dbSet.AddAsync(entity);
            return entity;
        }

        public async Task AddRangeAsync(IEnumerable<User> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }

        public Task UpdateAsync(User entity)
        {
            _dbSet.Update(entity);
            return Task.CompletedTask;
        }

        public Task UpdateRangeAsync(IEnumerable<User> entities)
        {
            _dbSet.UpdateRange(entities);
            return Task.CompletedTask;
        }

      
        public Task DeleteAsync(User entity)
        {
            _dbSet.Remove(entity);
            return Task.CompletedTask;
        }

        public Task DeleteRangeAsync(IEnumerable<User> entities)
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


        public async Task<int> CountAsync()
        {
            return await _dbSet.CountAsync();
        }

        public async Task<int> CountAsync(Expression<Func<User, bool>> predicate)
        {
            return await _dbSet.CountAsync(predicate);
        }

        // Exists operations
        public async Task<bool> ExistsAsync(Expression<Func<User, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        // Save changes
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        // Additional User-specific methods
        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<IEnumerable<User>> GetUsersByRoleAsync(Enums.Role role)
        {
            return await _dbSet.Where(u => u.Role == role).ToListAsync();
        }

        public async Task<User?> GetUserWithTicketsAsync(int userId)
        {
            return await _dbSet
                .Include(u => u.Tickets)
                .ThenInclude(t => t.Trip)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<User?> GetUserWithReviewsAsync(int userId)
        {
            return await _dbSet
                .Include(u => u.Reviews)
                .ThenInclude(r => r.Company)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }
    }
}
