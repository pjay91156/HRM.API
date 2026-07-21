using HRM.API.Data;
using HRM.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HRM.API.Repositories;

public interface IFloorRepository
{
    Task<List<Floor>> GetAllAsync();

    Task<Floor?> GetByIdAsync(Guid id);

    Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null);

    Task AddAsync(Floor floor);

    void Update(Floor floor);

    Task SaveChangesAsync();
}

public class FloorRepository : IFloorRepository
{
    private readonly ApplicationDbContext _context;

    public FloorRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Floor>> GetAllAsync()
    {
        return await _context.Floors
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public async Task<Floor?> GetByIdAsync(Guid id)
    {
        return await _context.Floors
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive);
    }

    public async Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null)
    {
        var query = _context.Floors
            .Where(x => x.IsActive && x.Name.ToLower() == name.ToLower());

        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task AddAsync(Floor floor)
    {
        await _context.Floors.AddAsync(floor);
    }

    public void Update(Floor floor)
    {
        _context.Floors.Update(floor);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
