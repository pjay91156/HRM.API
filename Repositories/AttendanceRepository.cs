using HRM.API.Data;
using HRM.API.Responses;
using Microsoft.EntityFrameworkCore;
using HRM.API.Models;
using HRM.API.Enums;
namespace HRM.API.Repositories;

public interface IAttendanceRepository
{
    Task<Attendance?> GetTodayAttendanceAsync(Guid employeeId);

    Task<Attendance?> GetAttendanceByDateAsync(Guid employeeId, DateTime attendanceDate);

    Task<AttendanceSession?> GetOpenSessionAsync(Guid employeeId);

    Task<int> GetLatestSessionNumberAsync(Guid attendanceId);

    Task AddAttendanceAsync(Attendance attendance);

    Task AddAttendanceSessionAsync(AttendanceSession session);

    Task UpdateAttendanceAsync(Attendance attendance);

    Task UpdateAttendanceSessionAsync(AttendanceSession session);

    Task<List<AttendanceSession>> GetTodaySessionsAsync(Guid attendanceId);

    Task<List<Attendance>> GetAttendanceHistoryAsync(Guid employeeId);

    Task<List<Attendance>> GetAttendanceCalendarAsync(
        Guid employeeId,
        int month,
        int year);
    Task<AttendanceSummaryResponse> GetAttendanceSummaryAsync(Guid employeeId, DateTime attendanceDate);
    Task<WeeklyAttendanceSummaryResponse> GetWeeklySummaryAsync(Guid employeeId, DateTime attendanceDate);
    Task<TeamAttendanceSummaryResponse> GetTeamAttendanceSummaryAsync(Guid managerEmployeeId, DateTime attendanceDate, Guid companyId);
    Task<TeamAttendanceResponse> GetTeamAttendanceAsync(Guid userId, TeamAttendanceFilterDto request,Guid companyId);
}
public class AttendanceRepository : IAttendanceRepository
{
    private readonly ApplicationDbContext _context;

    public AttendanceRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Attendance?> GetTodayAttendanceAsync(Guid employeeId)
    {
        return await _context.Attendances
            .FirstOrDefaultAsync(x =>
                x.EmployeeId == employeeId &&
                x.AttendanceDate == DateTime.UtcNow.Date &&
                x.IsActive);
    }

    public async Task<Attendance?> GetAttendanceByDateAsync(Guid employeeId, DateTime attendanceDate)
    {
        return await _context.Attendances
            .Include(x => x.Sessions)
            .FirstOrDefaultAsync(x =>
                x.EmployeeId == employeeId &&
                x.AttendanceDate.Date == attendanceDate.Date &&
                x.IsActive);
    }

    public async Task<AttendanceSession?> GetOpenSessionAsync(Guid employeeId)
    {
        return await _context.AttendanceSessions
            .Include(x => x.Attendance)
            .FirstOrDefaultAsync(x =>
                x.Attendance.EmployeeId == employeeId &&
                x.Attendance.AttendanceDate == DateTime.UtcNow.Date &&
                x.CheckOutTime == null &&
                x.IsActive);
    }

    public async Task<int> GetLatestSessionNumberAsync(Guid attendanceId)
    {
        return await _context.AttendanceSessions
            .Where(x => x.AttendanceId == attendanceId && x.IsActive)
            .MaxAsync(x => (int?)x.SessionNumber) ?? 0;
    }

    public async Task AddAttendanceAsync(Attendance attendance)
    {
        await _context.Attendances.AddAsync(attendance);

        await _context.SaveChangesAsync();
    }

    public async Task AddAttendanceSessionAsync(
        AttendanceSession session)
    {
        await _context.AttendanceSessions.AddAsync(session);

        await _context.SaveChangesAsync();
    }

    public async Task UpdateAttendanceAsync(
        Attendance attendance)
    {
        _context.Attendances.Update(attendance);

        await _context.SaveChangesAsync();
    }

    public async Task UpdateAttendanceSessionAsync(
        AttendanceSession session)
    {
        _context.AttendanceSessions.Update(session);

        await _context.SaveChangesAsync();
    }

    public async Task<List<AttendanceSession>>
        GetTodaySessionsAsync(Guid attendanceId)
    {
        return await _context.AttendanceSessions
            .Where(x =>
                x.AttendanceId == attendanceId &&
                x.IsActive)
            .OrderBy(x => x.SessionNumber)
            .ToListAsync();
    }

    public async Task<List<Attendance>>
        GetAttendanceHistoryAsync(Guid employeeId)
    {
        return await _context.Attendances
            .Where(x =>
                x.EmployeeId == employeeId &&
                x.IsActive)
            .OrderByDescending(x =>
                x.AttendanceDate)
            .ToListAsync();
    }

    public async Task<List<Attendance>>
        GetAttendanceCalendarAsync(
            Guid employeeId,
            int month,
            int year)
    {
        return await _context.Attendances
            .Where(x =>
                x.EmployeeId == employeeId &&
                x.AttendanceDate.Month == month &&
                x.AttendanceDate.Year == year &&
                x.IsActive)
            .ToListAsync();
    }
    public async Task<AttendanceSummaryResponse> GetAttendanceSummaryAsync(Guid employeeId, DateTime attendanceDate)
    {
        var attendance = await _context.Attendances
            .Include(x => x.Sessions)
            .FirstOrDefaultAsync(x =>
                x.EmployeeId == employeeId &&
                x.AttendanceDate.Date == attendanceDate.Date &&
                x.IsActive);

        if (attendance == null)
        {
            return new AttendanceSummaryResponse();
        }

        return new AttendanceSummaryResponse
        {
            FirstCheckIn = attendance.Sessions
                .OrderBy(x => x.SessionNumber)
                .Select(x => (DateTime?)x.CheckInTime)
                .FirstOrDefault(),

            LastCheckOut = attendance.Sessions
                .Where(x => x.CheckOutTime != null)
                .OrderByDescending(x => x.SessionNumber)
                .Select(x => x.CheckOutTime)
                .FirstOrDefault(),

            TotalWorkingHours = attendance.TotalWorkingHours
        };
    }
    public async Task<WeeklyAttendanceSummaryResponse> GetWeeklySummaryAsync(Guid employeeId, DateTime attendanceDate)
    {
        var referenceDate = attendanceDate.Date;

        // Monday
        int diff = referenceDate.DayOfWeek == DayOfWeek.Sunday
            ? 6
            : (int)referenceDate.DayOfWeek - 1;

        var weekStart = referenceDate.AddDays(-diff);

        var weekEnd = weekStart.AddDays(6);

        var attendances = await _context.Attendances
            .Where(x =>
                x.EmployeeId == employeeId &&
                x.IsActive &&
                x.AttendanceDate >= weekStart &&
                x.AttendanceDate <= weekEnd)
            .ToListAsync();

        var response = new WeeklyAttendanceSummaryResponse();

        for (int i = 0; i < 7; i++)
        {
            var date = weekStart.AddDays(i);

            var attendance = attendances
                .FirstOrDefault(x => x.AttendanceDate.Date == date.Date);

            response.Days.Add(new WeeklyAttendanceDayResponse
            {
                Date = date,
                WorkingHours = attendance?.TotalWorkingHours ?? 0,
                IsPresent = attendance != null
            });
        }

        return response;
    }
    public async Task<TeamAttendanceSummaryResponse> GetTeamAttendanceSummaryAsync(
           Guid managerEmployeeId,
           DateTime attendanceDate,
           Guid companyId)
    {
        attendanceDate = attendanceDate.Date;

        // Query 1
        var employees = await _context.Employees
        .AsNoTracking()
        .Include(x => x.User)
        .Include(x => x.Department)
        .Include(x => x.Designation)
        .Where(x =>
            x.IsActive &&
            x.User.CompanyId == companyId)

            .Select(x => new EmployeeHierarchyDto
            {
                Id = x.Id,
                ManagerId = x.ManagerId
            })
            .ToListAsync();

        var employeeIds = GetAllReportingEmployees(managerEmployeeId, employees);

        if (!employeeIds.Any())
        {
            return new TeamAttendanceSummaryResponse();
        }

        // Query 2
        var presentEmployees = await _context.Attendances
            .Where(x =>
                employeeIds.Contains(x.EmployeeId) &&
                x.AttendanceDate.Date == attendanceDate)
            .Select(x => x.EmployeeId)
            .Distinct()
            .ToListAsync();

        // Query 3
        var leaveEmployees = await _context.LeaveRequests
            .Where(x =>
                employeeIds.Contains(x.EmployeeId) &&
                x.Status == LeaveStatus.Approved &&
                attendanceDate >= x.FromDate.Date &&
                attendanceDate <= x.ToDate.Date)
            .Select(x => x.EmployeeId)
            .Distinct()
            .ToListAsync();

        int total = employeeIds.Count;

        int leave = leaveEmployees.Count;

        int present = presentEmployees
            .Except(leaveEmployees)
            .Count();

        bool isWeeklyOff = attendanceDate.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;

        int absent = isWeeklyOff ? 0 : total - present - leave;

        return new TeamAttendanceSummaryResponse
        {
            TotalEmployees = total,
            PresentEmployees = present,
            OnLeaveEmployees = leave,
            AbsentEmployees = absent
        };
    }

    private List<Guid> GetAllReportingEmployees(
        Guid managerId,
        List<EmployeeHierarchyDto> employees)
    {
        var manager = employees.FirstOrDefault(x => x.Id == managerId);

        if (manager == null)
        {
            return new List<Guid>();
        }

        return GetReportingEmployees(manager, employees);
    }

    private List<Guid> GetReportingEmployees(
        EmployeeHierarchyDto manager,
        List<EmployeeHierarchyDto> allEmployees)
    {
        var result = new List<Guid>();

        var children = allEmployees
            .Where(x => x.ManagerId == manager.Id)
            .ToList();

        foreach (var child in children)
        {
            result.Add(child.Id);

            result.AddRange(GetReportingEmployees(child, allEmployees));
        }

        return result;
    }
    public async Task<TeamAttendanceResponse> GetTeamAttendanceAsync(Guid userId, TeamAttendanceFilterDto request, Guid companyId)
    {
        Guid managerEmployeeId;

        if (request.ManagerId is null)
        {
            var loggedInEmployee = await _context.Employees
       .AsNoTracking()
       .FirstOrDefaultAsync(x =>
           x.UserId == userId &&
           x.IsActive);
            managerEmployeeId = loggedInEmployee.Id;
        }
        else
        {
            managerEmployeeId = request.ManagerId.Value;
        }
        var attendanceDate = request.AttendanceDate.ToDateTime(TimeOnly.MinValue);
        var employees = await _context.Employees
    .AsNoTracking()
    .Include(x => x.User)
    .Include(x => x.Department)
    .Include(x => x.Designation)
    .Where(x =>
        x.IsActive &&
        x.User.CompanyId == companyId)

        .Select(x => new EmployeeHierarchyDto
        {
            Id = x.Id,
            ManagerId = x.ManagerId
        })
        .ToListAsync();

        var employeeIds = GetAllReportingEmployees(managerEmployeeId, employees);
        var employeeDetails = await _context.Employees
    .Include(x => x.User)
    .Include(x => x.Manager)
        .ThenInclude(x => x!.User)
    .Where(x => employeeIds.Contains(x.Id))
    .ToListAsync();

        var attendances = await _context.Attendances
        .Include(x => x.Sessions)
        .Where(x =>
            employeeIds.Contains(x.EmployeeId) &&
            x.AttendanceDate.Date == attendanceDate)
        .ToListAsync();

        var leaves = await _context.LeaveRequests
        .Where(x =>
            employeeIds.Contains(x.EmployeeId) &&
            x.Status == LeaveStatus.Approved &&
           attendanceDate >= x.FromDate &&
            attendanceDate <= x.ToDate)
        .ToListAsync();

        var attendanceDictionary = attendances
        .ToDictionary(x => x.EmployeeId);

        var leaveDictionary = leaves
            .ToDictionary(x => x.EmployeeId);
        var response = employeeDetails
        .Select(employee =>
        {
            attendanceDictionary.TryGetValue(
                employee.Id,
                out var attendance);

            leaveDictionary.TryGetValue(
                employee.Id,
                out var leave);

            DateTime? firstCheckIn = attendance?
                .Sessions
                .OrderBy(x => x.CheckInTime)
                .FirstOrDefault()
                ?.CheckInTime;

            DateTime? lastCheckOut = attendance?
                .Sessions
                .Where(x => x.CheckOutTime.HasValue)
                .OrderByDescending(x => x.CheckOutTime)
                .FirstOrDefault()
                ?.CheckOutTime;

            string status;

            bool isWeeklyOff = attendanceDate.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;

            if (leave != null)
            {
                status = "On Leave";
            }
            else if (attendance != null)
            {
                status = "Present";
            }
            else if (isWeeklyOff)
            {
                status = "Weekly Off";
            }
            else
            {
                status = "Absent";
            }

            return new TeamAttendanceGridResponse
            {
                EmployeeId = employee.Id,
                EmployeeCode = employee.EmployeeCode,
                EmployeeName = $"{employee.User.FirstName} {employee.User.LastName}",
                ReportingManager = employee.Manager == null
                    ? ""
                    : $"{employee.Manager.User.FirstName} {employee.Manager.User.LastName}",
                FirstCheckIn = firstCheckIn,
                LastCheckOut = lastCheckOut,
                TotalWorkingHours = attendance?.TotalWorkingHours ?? 0,
                Status = status
            };
        })
        .ToList();
        if (!string.IsNullOrWhiteSpace(request.SearchText))
{
    response = response
        .Where(x =>
            x.EmployeeName.Contains(request.SearchText, StringComparison.OrdinalIgnoreCase) ||
            x.EmployeeCode.Contains(request.SearchText, StringComparison.OrdinalIgnoreCase))
        .ToList();
}

var totalRecords = response.Count;

response = response
    .OrderBy(x => x.EmployeeName)
    .Skip((request.PageNumber - 1) * request.PageLength)
    .Take(request.PageLength)
    .ToList();

return new TeamAttendanceResponse
{
    TotalRecords = totalRecords,
    Records = response
};

    }

    private class EmployeeHierarchyDto
    {
        public Guid Id { get; set; }

        public Guid? ManagerId { get; set; }
    }
}