using System.Linq.Expressions;

namespace Ticket_Booking.Interfaces
{
    public interface IRepository<T> where T : class
    {
        // Get operations
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
        
        // Add operations
        Task<T> AddAsync(T entity);
        Task AddRangeAsync(IEnumerable<T> entities);
        
        // Update operations
        Task UpdateAsync(T entity);
        Task UpdateRangeAsync(IEnumerable<T> entities);
        
        // Delete operations
        Task DeleteAsync(T entity);
        Task DeleteRangeAsync(IEnumerable<T> entities);
        Task DeleteByIdAsync(int id);
        
        // Count operations
        Task<int> CountAsync();
        Task<int> CountAsync(Expression<Func<T, bool>> predicate);
        
        // Exists operations
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
        
        // Save changes
        Task<int> SaveChangesAsync();
    }
}
