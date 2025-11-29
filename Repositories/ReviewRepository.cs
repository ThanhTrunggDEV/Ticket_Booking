using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Ticket_Booking.Data;
using Ticket_Booking.Interfaces;
using Ticket_Booking.Models.DomainModels;

namespace Ticket_Booking.Repositories
{
    public class ReviewRepository : IRepository<Review>
    {
        private readonly AppDbContext _context;
        private readonly DbSet<Review> _dbSet;

        public ReviewRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<Review>();
        }

        // Get operations
        public async Task<Review?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<IEnumerable<Review>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<IEnumerable<Review>> FindAsync(Expression<Func<Review, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public async Task<Review?> FirstOrDefaultAsync(Expression<Func<Review, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }

        // Add operations
        public async Task<Review> AddAsync(Review entity)
        {
            await _dbSet.AddAsync(entity);
            return entity;
        }

        public async Task AddRangeAsync(IEnumerable<Review> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }

        // Update operations
        public Task UpdateAsync(Review entity)
        {
            _dbSet.Update(entity);
            return Task.CompletedTask;
        }

        public Task UpdateRangeAsync(IEnumerable<Review> entities)
        {
            _dbSet.UpdateRange(entities);
            return Task.CompletedTask;
        }

        // Delete operations
        public Task DeleteAsync(Review entity)
        {
            _dbSet.Remove(entity);
            return Task.CompletedTask;
        }

        public Task DeleteRangeAsync(IEnumerable<Review> entities)
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

        public async Task<int> CountAsync(Expression<Func<Review, bool>> predicate)
        {
            return await _dbSet.CountAsync(predicate);
        }

        // Exists operations
        public async Task<bool> ExistsAsync(Expression<Func<Review, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        // Save changes
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        // Review-specific methods
        public async Task<IEnumerable<Review>> GetByUserAsync(int userId)
        {
            return await _dbSet
                .Where(r => r.UserId == userId)
                .Include(r => r.Company)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetByCompanyAsync(int companyId)
        {
            return await _dbSet
                .Where(r => r.CompanyId == companyId)
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetByRatingAsync(int rating)
        {
            return await _dbSet
                .Where(r => r.Rating == rating)
                .Include(r => r.User)
                .Include(r => r.Company)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<Review?> GetCompleteAsync(int id)
        {
            return await _dbSet
                .Include(r => r.User)
                .Include(r => r.Company)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<double> GetAverageRatingByCompanyAsync(int companyId)
        {
            var reviews = await _dbSet
                .Where(r => r.CompanyId == companyId)
                .ToListAsync();

            if (!reviews.Any())
                return 0;

            return reviews.Average(r => r.Rating);
        }

        public async Task<Dictionary<int, int>> GetRatingDistributionAsync(int companyId)
        {
            return await _dbSet
                .Where(r => r.CompanyId == companyId)
                .GroupBy(r => r.Rating)
                .Select(g => new { Rating = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Rating, x => x.Count);
        }

        public async Task<IEnumerable<Review>> GetRecentReviewsAsync(int count = 10)
        {
            return await _dbSet
                .Include(r => r.User)
                .Include(r => r.Company)
                .OrderByDescending(r => r.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetTopRatedReviewsAsync(int companyId, int minRating = 4, int count = 10)
        {
            return await _dbSet
                .Where(r => r.CompanyId == companyId && r.Rating >= minRating)
                .Include(r => r.User)
                .OrderByDescending(r => r.Rating)
                .ThenByDescending(r => r.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<bool> HasUserReviewedCompanyAsync(int userId, int companyId)
        {
            return await _dbSet.AnyAsync(r => r.UserId == userId && r.CompanyId == companyId);
        }

        public async Task<IEnumerable<Review>> SearchReviewsAsync(string searchText)
        {
            return await _dbSet
                .Where(r => r.Comment != null && r.Comment.Contains(searchText))
                .Include(r => r.User)
                .Include(r => r.Company)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetTotalReviewCountByCompanyAsync(int companyId)
        {
            return await _dbSet.CountAsync(r => r.CompanyId == companyId);
        }

        public Task<Review?> GetByEmailAsync(string email)
        {
            throw new NotImplementedException();
        }
    }
}
