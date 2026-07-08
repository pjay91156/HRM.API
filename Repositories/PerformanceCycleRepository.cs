
using HRM.API.Data;
using HRM.API.Models;
using HRM.API.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HRM.API.Repositories;
public interface IPerformanceCycleRepository
{
    Task<bool> ExistsAsync(string cycleName, Guid? excludeId = null);

    Task<PerformanceCycle?> GetByIdAsync(Guid id);

    Task AddAsync(PerformanceCycle performanceCycle);

    void Update(PerformanceCycle performanceCycle);

    Task SaveChangesAsync();
    Task<List<PerformanceCycle>> GetAllAsync();

}


public class PerformanceCycleRepository : IPerformanceCycleRepository
{
    private readonly ApplicationDbContext _context;

    public PerformanceCycleRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExistsAsync(string cycleName, Guid? excludeId = null)
    {
        var query = _context.PerformanceCycles
            .Where(x => x.IsActive &&
                        x.CycleName.ToLower() == cycleName.ToLower());

        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<PerformanceCycle?> GetByIdAsync(Guid id)
    {
        return await _context.PerformanceCycles
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive);
    }
    public async Task<List<PerformanceCycle>> GetAllAsync()
{
    return await _context.PerformanceCycles
        .Where(x => x.IsActive)
        .OrderByDescending(x => x.CreatedAt)
        .ToListAsync();
}

    public async Task AddAsync(PerformanceCycle performanceCycle)
    {
        await _context.PerformanceCycles.AddAsync(performanceCycle);
    }

    public void Update(PerformanceCycle performanceCycle)
    {
        _context.PerformanceCycles.Update(performanceCycle);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}