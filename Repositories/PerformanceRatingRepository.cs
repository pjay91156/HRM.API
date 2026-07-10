
using HRM.API.Data;
using HRM.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HRM.API.Repositories;

public interface IPerformanceRatingRepository
{
    Task<bool> ExistsByRatingAsync(byte rating, Guid? excludeId = null);

    Task<bool> ExistsByNameAsync(string ratingName, Guid? excludeId = null);

    Task<PerformanceRating?> GetByIdAsync(Guid id);

    Task AddAsync(PerformanceRating performanceRating);

    void Update(PerformanceRating performanceRating);

    Task SaveChangesAsync();

    Task<List<PerformanceRating>> GetAllAsync();
}

public class PerformanceRatingRepository : IPerformanceRatingRepository
{
    private readonly ApplicationDbContext _context;

    public PerformanceRatingRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExistsByRatingAsync(byte rating, Guid? excludeId = null)
    {
        var query = _context.PerformanceRatings
            .Where(x => x.IsActive && x.Rating == rating);

        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<bool> ExistsByNameAsync(string ratingName, Guid? excludeId = null)
    {
        var query = _context.PerformanceRatings
            .Where(x => x.IsActive && x.RatingName.ToLower() == ratingName.ToLower());

        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<PerformanceRating?> GetByIdAsync(Guid id)
    {
        return await _context.PerformanceRatings
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive);
    }

    public async Task<List<PerformanceRating>> GetAllAsync()
    {
        return await _context.PerformanceRatings
            .Where(x => x.IsActive)
            .OrderBy(x => x.DisplayOrder)
            .ThenByDescending(x => x.Rating)
            .ToListAsync();
    }

    public async Task AddAsync(PerformanceRating performanceRating)
    {
        await _context.PerformanceRatings.AddAsync(performanceRating);
    }

    public void Update(PerformanceRating performanceRating)
    {
        _context.PerformanceRatings.Update(performanceRating);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
