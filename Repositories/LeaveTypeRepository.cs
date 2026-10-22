using HRM.API.Data;
using HRM.API.Responses;
using Microsoft.EntityFrameworkCore;
using HRM.API.Models;
public interface ILeaveTypeRepository
{
    Task<List<LeaveTypeResponse>> GetLeaveTypesAsync();

    Task AddLeaveTypeAsync(LeaveType leaveType);

    Task<bool> DeleteLeaveTypeAsync(Guid leaveTypeId);

    Task<LeaveType?> GetByIdAsync(Guid id);

    Task<bool> ExistsByCodeAsync(string leaveCode, Guid? excludeId = null);

    Task<bool> ExistsByNameAsync(string leaveName, Guid? excludeId = null);

    Task SaveChangesAsync();
}
public class LeaveTypeRepository : ILeaveTypeRepository
{
    private readonly ApplicationDbContext _context;

    public LeaveTypeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<LeaveTypeResponse>> GetLeaveTypesAsync()
    {
        return await _context.LeaveTypes
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.LeaveName)
            .Select(x => new LeaveTypeResponse
            {
                Id = x.Id,
                LeaveName = x.LeaveName,
                LeaveCode = x.LeaveCode,
                Description = x.Description,
                DefaultDays = x.DefaultDays,
                IsActive = x.IsActive
            })
            .ToListAsync();
    }

    public async Task AddLeaveTypeAsync(LeaveType leaveType)
    {
        await _context.LeaveTypes.AddAsync(leaveType);
    }

    public async Task<bool> DeleteLeaveTypeAsync(Guid leaveTypeId)
    {
        var leaveType = await _context.LeaveTypes
            .FirstOrDefaultAsync(x => x.Id == leaveTypeId);

        if (leaveType == null)
            return false;

        leaveType.IsActive = false;

        return true;
    }

    public async Task<LeaveType?> GetByIdAsync(Guid id)
    {
        return await _context.LeaveTypes
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<bool> ExistsByCodeAsync(string leaveCode, Guid? excludeId = null)
    {
        var query = _context.LeaveTypes
            .Where(x => x.IsActive && x.LeaveCode.ToLower() == leaveCode.ToLower());

        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<bool> ExistsByNameAsync(string leaveName, Guid? excludeId = null)
    {
        var query = _context.LeaveTypes
            .Where(x => x.IsActive && x.LeaveName.ToLower() == leaveName.ToLower());

        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}