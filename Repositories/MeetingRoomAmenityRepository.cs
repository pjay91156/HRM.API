using HRM.API.Data;
using HRM.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HRM.API.Repositories;

public interface IMeetingRoomAmenityRepository
{
    Task<List<MeetingRoomAmenity>> GetAllAsync();

    Task<MeetingRoomAmenity?> GetByIdAsync(Guid id);

    Task<List<MeetingRoomAmenity>> GetByIdsAsync(List<Guid> ids);

    Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null);

    Task AddAsync(MeetingRoomAmenity amenity);

    void Update(MeetingRoomAmenity amenity);

    Task SaveChangesAsync();
}

public class MeetingRoomAmenityRepository : IMeetingRoomAmenityRepository
{
    private readonly ApplicationDbContext _context;

    public MeetingRoomAmenityRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<MeetingRoomAmenity>> GetAllAsync()
    {
        return await _context.MeetingRoomAmenities
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public async Task<MeetingRoomAmenity?> GetByIdAsync(Guid id)
    {
        return await _context.MeetingRoomAmenities
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive);
    }

    public async Task<List<MeetingRoomAmenity>> GetByIdsAsync(List<Guid> ids)
    {
        return await _context.MeetingRoomAmenities
            .Where(x => ids.Contains(x.Id) && x.IsActive)
            .ToListAsync();
    }

    public async Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null)
    {
        var query = _context.MeetingRoomAmenities
            .Where(x => x.IsActive && x.Name.ToLower() == name.ToLower());

        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task AddAsync(MeetingRoomAmenity amenity)
    {
        await _context.MeetingRoomAmenities.AddAsync(amenity);
    }

    public void Update(MeetingRoomAmenity amenity)
    {
        _context.MeetingRoomAmenities.Update(amenity);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
