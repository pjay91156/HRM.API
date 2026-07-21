using HRM.API.Data;
using HRM.API.Enums;
using HRM.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HRM.API.Repositories;

public interface IMeetingRoomBookingRepository
{
    Task<List<MeetingRoomBooking>> GetAllAsync(
        Guid? floorId,
        Guid? meetingRoomId,
        Guid? employeeId,
        DateOnly? fromDate,
        DateOnly? toDate,
        MeetingRoomBookingStatus? status);

    Task<MeetingRoomBooking?> GetByIdAsync(Guid id);

    Task<bool> HasOverlappingBookingAsync(
        Guid meetingRoomId,
        DateOnly bookingDate,
        TimeOnly startTime,
        TimeOnly endTime,
        Guid? excludeId = null);

    Task AddAsync(MeetingRoomBooking booking);

    void Update(MeetingRoomBooking booking);

    Task SaveChangesAsync();
}

public class MeetingRoomBookingRepository : IMeetingRoomBookingRepository
{
    private readonly ApplicationDbContext _context;

    public MeetingRoomBookingRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    private IQueryable<MeetingRoomBooking> QueryWithIncludes()
    {
        return _context.MeetingRoomBookings
            .Include(x => x.MeetingRoom).ThenInclude(r => r.Floor)
            .Include(x => x.Employee).ThenInclude(e => e.User);
    }

    public async Task<List<MeetingRoomBooking>> GetAllAsync(
        Guid? floorId,
        Guid? meetingRoomId,
        Guid? employeeId,
        DateOnly? fromDate,
        DateOnly? toDate,
        MeetingRoomBookingStatus? status)
    {
        var query = QueryWithIncludes().Where(x => x.IsActive);

        if (floorId.HasValue)
        {
            query = query.Where(x => x.MeetingRoom.FloorId == floorId.Value);
        }

        if (meetingRoomId.HasValue)
        {
            query = query.Where(x => x.MeetingRoomId == meetingRoomId.Value);
        }

        if (employeeId.HasValue)
        {
            query = query.Where(x => x.EmployeeId == employeeId.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(x => x.BookingDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(x => x.BookingDate <= toDate.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        return await query
            .OrderByDescending(x => x.BookingDate)
            .ThenBy(x => x.StartTime)
            .ToListAsync();
    }

    public async Task<MeetingRoomBooking?> GetByIdAsync(Guid id)
    {
        return await QueryWithIncludes().FirstOrDefaultAsync(x => x.Id == id && x.IsActive);
    }

    public async Task<bool> HasOverlappingBookingAsync(
        Guid meetingRoomId,
        DateOnly bookingDate,
        TimeOnly startTime,
        TimeOnly endTime,
        Guid? excludeId = null)
    {
        var query = _context.MeetingRoomBookings
            .Where(x =>
                x.IsActive &&
                x.MeetingRoomId == meetingRoomId &&
                x.BookingDate == bookingDate &&
                x.Status == MeetingRoomBookingStatus.Confirmed &&
                x.StartTime < endTime &&
                x.EndTime > startTime);

        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task AddAsync(MeetingRoomBooking booking)
    {
        await _context.MeetingRoomBookings.AddAsync(booking);
    }

    public void Update(MeetingRoomBooking booking)
    {
        _context.MeetingRoomBookings.Update(booking);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
