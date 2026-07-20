using HRM.API.Data;
using HRM.API.Responses;
using Microsoft.EntityFrameworkCore;
using HRM.API.Models;
using HRM.API.Enums;
public interface ILeaveRequestRepository
{
    Task AddAsync(LeaveRequest leaveRequest);

    Task<LeaveRequest?> GetByIdAsync(Guid id);

    Task<List<LeaveRequestResponse>> GetAllAsync();

    Task<List<LeaveRequestResponse>> GetByEmployeeIdAsync(Guid employeeId);

    Task UpdateAsync(LeaveRequest leaveRequest);
    Task<List<TeamLeaveRequestResponse>> GetTeamLeaveRequestsAsync(Guid managerId);
    Task<List<TeamLeaveCalendarResponse>> GetTeamLeaveCalendarAsync(Guid managerId);
    Task<bool> HasOverlappingRequestAsync(Guid employeeId, DateTime fromDate, DateTime toDate);
    Task<List<LeaveBalanceResponse>> GetLeaveBalanceAsync(Guid employeeId);

}
public class LeaveRequestRepository : ILeaveRequestRepository
{
    private readonly ApplicationDbContext _context;

    public LeaveRequestRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(LeaveRequest leaveRequest)
    {
        await _context.LeaveRequests.AddAsync(leaveRequest);
        await _context.SaveChangesAsync();
    }
    private async Task<List<Guid>>
        GetAllSubordinateIdsAsync(Guid managerId)
    {
        var employees = await _context.Employees
            .Select(x => new
            {
                x.Id,
                x.ManagerId
            })
            .ToListAsync();

        var result = new List<Guid>();

        void FindChildren(Guid manager)
        {
            var children = employees
                .Where(x => x.ManagerId == manager)
                .Select(x => x.Id)
                .ToList();

            foreach (var child in children)
            {
                result.Add(child);

                FindChildren(child);
            }
        }

        FindChildren(managerId);

        return result;
    }
    public async Task<List<TeamLeaveCalendarResponse>>
        GetTeamLeaveCalendarAsync(Guid managerId)
    {
        var employeeIds =
            await GetAllSubordinateIdsAsync(managerId);

        return await (
            from lr in _context.LeaveRequests
            join e in _context.Employees
                on lr.EmployeeId equals e.Id
            join u in _context.Users
                 on e.UserId equals u.Id
            join lt in _context.LeaveTypes
                on lr.LeaveTypeId equals lt.Id

            where employeeIds.Contains(e.Id)
                  && lr.Status == LeaveStatus.Approved
                  && lr.IsActive

            select new TeamLeaveCalendarResponse
            {
                LeaveRequestId = lr.Id,
                EmployeeId = e.Id,
                EmployeeName =
                    u.FirstName + " " + u.LastName,

                LeaveType = lt.LeaveName,

                FromDate = lr.FromDate,

                ToDate = lr.ToDate,

                TotalDays = lr.TotalDays,

                LeaveDuration = lr.LeaveDuration,

                HalfDayPeriod = lr.HalfDayPeriod
            })
            .ToListAsync();
    }
    public async Task<LeaveRequest?> GetByIdAsync(Guid id)
    {
        return await _context.LeaveRequests
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive);
    }

    public async Task<List<LeaveRequestResponse>> GetAllAsync()
    {
        return await (
              from lr in _context.LeaveRequests
              join e in _context.Employees
                  on lr.EmployeeId equals e.Id
              join lt in _context.LeaveTypes
                  on lr.LeaveTypeId equals lt.Id
              where
                   lr.IsActive
              select new LeaveRequestResponse
              {
                  Id = lr.Id,
                  EmployeeId = lr.EmployeeId,
                  EmployeeName = " ",

                  LeaveType = lt.LeaveCode,

                  FromDate = lr.FromDate,
                  ToDate = lr.ToDate,
                  TotalDays = lr.TotalDays,

                  LeaveDuration = lr.LeaveDuration,

                  HalfDayPeriod = lr.HalfDayPeriod,

                  Reason = lr.Reason,
                  Status = lr.Status,
                  ApproverComments = lr.ApproverComments
              })
              .ToListAsync();
    }

    public async Task<List<LeaveRequestResponse>> GetByEmployeeIdAsync(Guid employeeId)
    {
        return await (
            from lr in _context.LeaveRequests
            join e in _context.Employees
                on lr.EmployeeId equals e.Id
            join lt in _context.LeaveTypes
                on lr.LeaveTypeId equals lt.Id
            where lr.EmployeeId == employeeId
                && lr.IsActive
            orderby lr.CreatedAt descending
            select new LeaveRequestResponse
            {
                Id = lr.Id,
                EmployeeId = lr.EmployeeId,
                EmployeeName = " ",

                LeaveType = lt.LeaveCode,

                FromDate = lr.FromDate,
                ToDate = lr.ToDate,
                TotalDays = lr.TotalDays,

                LeaveDuration = lr.LeaveDuration,

                HalfDayPeriod = lr.HalfDayPeriod,

                Reason = lr.Reason,
                Status = lr.Status,
                ApproverComments = lr.ApproverComments
            })
            .ToListAsync();
    }

    public async Task<bool> HasOverlappingRequestAsync(Guid employeeId, DateTime fromDate, DateTime toDate)
    {
        return await _context.LeaveRequests
            .AnyAsync(x =>
                x.EmployeeId == employeeId &&
                x.IsActive &&
                (x.Status == LeaveStatus.Pending || x.Status == LeaveStatus.Approved) &&
                x.FromDate.Date <= toDate.Date &&
                x.ToDate.Date >= fromDate.Date);
    }

    public async Task<List<LeaveBalanceResponse>> GetLeaveBalanceAsync(Guid employeeId)
    {
        var leaveTypes = await _context.LeaveTypes
            .AsNoTracking()
            .Where(x => x.IsActive)
            .ToListAsync();

        var usedDaysByType = await _context.LeaveRequests
            .Where(x =>
                x.EmployeeId == employeeId &&
                x.IsActive &&
                x.Status == LeaveStatus.Approved)
            .GroupBy(x => x.LeaveTypeId)
            .Select(g => new { LeaveTypeId = g.Key, UsedDays = g.Sum(x => x.TotalDays) })
            .ToListAsync();

        return leaveTypes
            .Select(lt =>
            {
                var usedDays = usedDaysByType
                    .FirstOrDefault(x => x.LeaveTypeId == lt.Id)
                    ?.UsedDays ?? 0;

                return new LeaveBalanceResponse
                {
                    LeaveTypeId = lt.Id,
                    LeaveName = lt.LeaveName,
                    DefaultDays = lt.DefaultDays,
                    UsedDays = usedDays,
                    RemainingDays = lt.DefaultDays - usedDays
                };
            })
            .ToList();
    }


    public async Task UpdateAsync(LeaveRequest leaveRequest)
    {
        _context.LeaveRequests.Update(leaveRequest);
        await _context.SaveChangesAsync();
    }
    public async Task<List<TeamLeaveRequestResponse>> GetTeamLeaveRequestsAsync(Guid managerId)
    {
        return await (
            from lr in _context.LeaveRequests
            join e in _context.Employees
                on lr.EmployeeId equals e.Id
            join u in _context.Users
                 on e.UserId equals u.Id
            join lt in _context.LeaveTypes
                on lr.LeaveTypeId equals lt.Id
            where e.ManagerId == managerId
                && lr.IsActive
            orderby lr.CreatedAt descending
            select new TeamLeaveRequestResponse
            {
                LeaveRequestId = lr.Id,
                EmployeeId = e.Id,
                EmployeeName = u.FirstName + " " + u.LastName,
                LeaveType = lt.LeaveName,
                FromDate = lr.FromDate,
                ToDate = lr.ToDate,
                TotalDays = lr.TotalDays,
                Status = lr.Status.ToString(),
                Reason = lr.Reason,
                AppliedOn = lr.CreatedAt
            }
        ).ToListAsync();
    }
}