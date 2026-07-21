using HRM.API.Data;
using HRM.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HRM.API.Repositories;

public interface IMeetingRoomRepository
{
    Task<List<MeetingRoom>> GetAllAsync(Guid? floorId);

    Task<MeetingRoom?> GetByIdAsync(Guid id);

    Task<bool> ExistsByFloorAndNameAsync(Guid floorId, string name, Guid? excludeId = null);

    Task<List<MeetingRoomAmenityDetail>> GetAmenityDetailsByRoomIdAsync(Guid roomId);

    Task AddAsync(MeetingRoom meetingRoom);

    void Update(MeetingRoom meetingRoom);

    Task AddAmenityDetailAsync(MeetingRoomAmenityDetail detail);

    void UpdateAmenityDetail(MeetingRoomAmenityDetail detail);

    Task SaveChangesAsync();
}

public class MeetingRoomRepository : IMeetingRoomRepository
{
    private readonly ApplicationDbContext _context;

    public MeetingRoomRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    private IQueryable<MeetingRoom> QueryWithIncludes()
    {
        return _context.MeetingRooms
            .Include(x => x.Floor)
            .Include(x => x.MeetingRoomAmenityDetails.Where(d => d.IsActive))
                .ThenInclude(d => d.MeetingRoomAmenity);
    }

    public async Task<List<MeetingRoom>> GetAllAsync(Guid? floorId)
    {
        var query = QueryWithIncludes().Where(x => x.IsActive);

        if (floorId.HasValue)
        {
            query = query.Where(x => x.FloorId == floorId.Value);
        }

        return await query.OrderBy(x => x.Floor.Name).ThenBy(x => x.Name).ToListAsync();
    }

    public async Task<MeetingRoom?> GetByIdAsync(Guid id)
    {
        return await QueryWithIncludes().FirstOrDefaultAsync(x => x.Id == id && x.IsActive);
    }

    public async Task<bool> ExistsByFloorAndNameAsync(Guid floorId, string name, Guid? excludeId = null)
    {
        var query = _context.MeetingRooms
            .Where(x => x.IsActive && x.FloorId == floorId && x.Name.ToLower() == name.ToLower());

        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<List<MeetingRoomAmenityDetail>> GetAmenityDetailsByRoomIdAsync(Guid roomId)
    {
        return await _context.MeetingRoomAmenityDetails
            .Where(x => x.MeetingRoomId == roomId)
            .ToListAsync();
    }

    public async Task AddAsync(MeetingRoom meetingRoom)
    {
        await _context.MeetingRooms.AddAsync(meetingRoom);
    }

    public void Update(MeetingRoom meetingRoom)
    {
        _context.MeetingRooms.Update(meetingRoom);
    }

    public async Task AddAmenityDetailAsync(MeetingRoomAmenityDetail detail)
    {
        await _context.MeetingRoomAmenityDetails.AddAsync(detail);
    }

    public void UpdateAmenityDetail(MeetingRoomAmenityDetail detail)
    {
        _context.MeetingRoomAmenityDetails.Update(detail);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
