using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Ticket_Booking.Data;
using Ticket_Booking.Enums;
using Ticket_Booking.Interfaces;
using Ticket_Booking.Models.DomainModels;

namespace Ticket_Booking.Repositories
{
    public class TripRepository : IRepository<Trip>
    {
        private readonly AppDbContext _context;
        private readonly DbSet<Trip> _dbSet;

        public TripRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<Trip>();
        }

        // Get operations
        public async Task<Trip?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<IEnumerable<Trip>> GetAllAsync()
        {
            return await _dbSet
                .Include(t => t.Company)
                .ToListAsync();
        }

        public async Task<IEnumerable<Trip>> FindAsync(Expression<Func<Trip, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public async Task<Trip?> FirstOrDefaultAsync(Expression<Func<Trip, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }

        // Add operations
        public async Task<Trip> AddAsync(Trip entity)
        {
            await _dbSet.AddAsync(entity);
            return entity;
        }

        public async Task AddRangeAsync(IEnumerable<Trip> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }

        // Update operations
        public Task UpdateAsync(Trip entity)
        {
            _dbSet.Update(entity);
            return Task.CompletedTask;
        }

        public Task UpdateRangeAsync(IEnumerable<Trip> entities)
        {
            _dbSet.UpdateRange(entities);
            return Task.CompletedTask;
        }

        // Delete operations
        public Task DeleteAsync(Trip entity)
        {
            _dbSet.Remove(entity);
            return Task.CompletedTask;
        }

        public Task DeleteRangeAsync(IEnumerable<Trip> entities)
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

        public async Task<int> CountAsync(Expression<Func<Trip, bool>> predicate)
        {
            return await _dbSet.CountAsync(predicate);
        }

        // Exists operations
        public async Task<bool> ExistsAsync(Expression<Func<Trip, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        // Save changes
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        // Trip-specific methods
        public async Task<IEnumerable<Trip>> GetByCompanyAsync(int companyId)
        {
            return await _dbSet
                .Where(t => t.CompanyId == companyId)
                .Include(t => t.Company)
                .OrderBy(t => t.DepartureTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Trip>> GetByStatusAsync(TripStatus status)
        {
            return await _dbSet
                .Where(t => t.Status == status)
                .Include(t => t.Company)
                .ToListAsync();
        }

        public async Task<IEnumerable<Trip>> SearchTripsAsync(string fromCity, string toCity, DateTime? date = null)
        {
            var query = _dbSet
                .Include(t => t.Company)
                .Where(t => t.FromCity.Contains(fromCity) && t.ToCity.Contains(toCity));

            if (date.HasValue)
            {
                var startOfDay = date.Value.Date;
                var endOfDay = startOfDay.AddDays(1);
                query = query.Where(t => t.DepartureTime >= startOfDay && t.DepartureTime < endOfDay);
            }

            return await query
                .OrderBy(t => t.DepartureTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Trip>> SearchAndSortTripsAsync(string fromCity, string toCity, DateTime? date = null, SortCriteria sortBy = SortCriteria.DepartureTimeAsc, SeatClass? seatClass = null)
        {
            var query = _dbSet
                .Include(t => t.Company)
                .Where(t => t.FromCity.Contains(fromCity) && t.ToCity.Contains(toCity));

            if (date.HasValue)
            {
                var startOfDay = date.Value.Date;
                var endOfDay = startOfDay.AddDays(1);
                query = query.Where(t => t.DepartureTime >= startOfDay && t.DepartureTime < endOfDay);
            }

            // Load data from database first
            var trips = await query.ToListAsync();

            // Apply sorting on client side (needed because SQLite doesn't support decimal ORDER BY)
            IEnumerable<Trip> sortedTrips = sortBy switch
            {
                SortCriteria.PriceAsc => seatClass switch
                {
                    SeatClass.Economy => trips.OrderBy(t => t.EconomyPrice),
                    SeatClass.Business => trips.OrderBy(t => t.BusinessPrice),
                    SeatClass.FirstClass => trips.OrderBy(t => t.FirstClassPrice),
                    _ => trips.OrderBy(t => t.EconomyPrice) // Default to Economy
                },
                SortCriteria.PriceDesc => seatClass switch
                {
                    SeatClass.Economy => trips.OrderByDescending(t => t.EconomyPrice),
                    SeatClass.Business => trips.OrderByDescending(t => t.BusinessPrice),
                    SeatClass.FirstClass => trips.OrderByDescending(t => t.FirstClassPrice),
                    _ => trips.OrderByDescending(t => t.EconomyPrice)
                },
                SortCriteria.DepartureTimeAsc => trips.OrderBy(t => t.DepartureTime),
                SortCriteria.DepartureTimeDesc => trips.OrderByDescending(t => t.DepartureTime),
                SortCriteria.DurationAsc => trips.OrderBy(t => t.EstimatedDuration),
                SortCriteria.DurationDesc => trips.OrderByDescending(t => t.EstimatedDuration),
                SortCriteria.DistanceAsc => trips.OrderBy(t => t.Distance),
                SortCriteria.DistanceDesc => trips.OrderByDescending(t => t.Distance),
                _ => trips.OrderBy(t => t.DepartureTime)
            };

            return sortedTrips;
        }

        public async Task<Trip?> GetCompleteAsync(int id)
        {
            return await _dbSet
                .Include(t => t.Company)
                .Include(t => t.Tickets)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<Trip>> GetAvailableTripsAsync(DateTime fromDate, DateTime toDate)
        {
            return await _dbSet
                .Where(t => t.DepartureTime >= fromDate && 
                           t.DepartureTime <= toDate && 
                           (t.EconomySeats > 0 || t.BusinessSeats > 0 || t.FirstClassSeats > 0) &&
                           t.Status == TripStatus.Active)
                .Include(t => t.Company)
                .OrderBy(t => t.DepartureTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Trip>> GetUpcomingTripsAsync(int hours = 24)
        {
            var now = DateTime.Now;
            var futureTime = now.AddHours(hours);

            return await _dbSet
                .Where(t => t.DepartureTime >= now && t.DepartureTime <= futureTime)
                .Include(t => t.Company)
                .OrderBy(t => t.DepartureTime)
                .ToListAsync();
        }

        public async Task<bool> UpdateAvailableSeatsAsync(int tripId, int seatsToBook, SeatClass seatClass)
        {
            var trip = await GetByIdAsync(tripId);
            if (trip == null) return false;

            switch (seatClass)
            {
                case SeatClass.Economy:
                    if (trip.EconomySeats < seatsToBook) return false;
                    trip.EconomySeats -= seatsToBook;
                    break;
                case SeatClass.Business:
                    if (trip.BusinessSeats < seatsToBook) return false;
                    trip.BusinessSeats -= seatsToBook;
                    break;
                case SeatClass.FirstClass:
                    if (trip.FirstClassSeats < seatsToBook) return false;
                    trip.FirstClassSeats -= seatsToBook;
                    break;
                default:
                    return false;
            }

            await UpdateAsync(trip);
            return true;
        }

        public Task<Trip?> GetByEmailAsync(string email)
        {
            throw new NotImplementedException();
        }
    }
}
