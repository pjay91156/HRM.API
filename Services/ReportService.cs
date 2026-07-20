using HRM.API.Data;
using HRM.API.Enums;
using HRM.API.Helpers;
using HRM.API.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HRM.API.Services;

public interface IReportService
{
    Task<string> GetEmployeeDirectoryReportAsync(Guid companyId);
    Task<string> GetAttendanceReportAsync(Guid userId, Guid companyId, DateOnly fromDate, DateOnly toDate);
    Task<string> GetLeaveReportAsync(Guid userId, DateOnly? fromDate, DateOnly? toDate, string? status);
    Task<string> GetPerformanceReviewReportAsync(Guid userId, Guid? cycleId);
}

public class ReportService : IReportService
{
    private readonly ApplicationDbContext _context;
    private readonly IEmployeeRepository _employeeRepository;

    public ReportService(ApplicationDbContext context, IEmployeeRepository employeeRepository)
    {
        _context = context;
        _employeeRepository = employeeRepository;
    }

    private static List<Guid> GetSelfAndReportingEmployeeIds(Guid rootEmployeeId, List<(Guid Id, Guid? ManagerId)> employees)
    {
        var result = new List<Guid> { rootEmployeeId };

        void FindChildren(Guid managerId)
        {
            var children = employees.Where(x => x.ManagerId == managerId).Select(x => x.Id).ToList();

            foreach (var child in children)
            {
                result.Add(child);
                FindChildren(child);
            }
        }

        FindChildren(rootEmployeeId);

        return result;
    }

    public async Task<string> GetEmployeeDirectoryReportAsync(Guid companyId)
    {
        var rows = await _context.Employees
            .AsNoTracking()
            .Include(e => e.User)
            .Include(e => e.Department)
            .Include(e => e.Designation)
            .Include(e => e.Manager).ThenInclude(m => m!.User)
            .Where(e => e.User.CompanyId == companyId)
            .OrderBy(e => e.User.FirstName)
            .Select(e => new object?[]
            {
                e.EmployeeCode,
                e.User.FirstName,
                e.User.LastName,
                e.User.Email,
                e.PhoneNumber,
                e.Department.DepartmentName,
                e.Designation.DesignationName,
                e.JoiningDate.ToString(),
                e.Manager == null ? "" : $"{e.Manager.User.FirstName} {e.Manager.User.LastName}",
                e.IsActive ? "Active" : "Inactive"
            })
            .ToListAsync();

        return CsvWriter.Write(
            new[] { "Employee Code", "First Name", "Last Name", "Email", "Phone", "Department", "Designation", "Joining Date", "Reporting Manager", "Status" },
            rows);
    }

    public async Task<string> GetAttendanceReportAsync(Guid userId, Guid companyId, DateOnly fromDate, DateOnly toDate)
    {
        var requester = await _employeeRepository.GetByUserIdAsync(userId);

        if (requester == null)
        {
            return CsvWriter.Write(new[] { "Message" }, new[] { new object?[] { "Employee not found." } });
        }

        var allEmployees = await _context.Employees
            .AsNoTracking()
            .Where(x => x.IsActive && x.User.CompanyId == companyId)
            .Select(x => new { x.Id, x.ManagerId })
            .ToListAsync();

        var employeeIds = GetSelfAndReportingEmployeeIds(
            requester.Id,
            allEmployees.Select(x => (x.Id, x.ManagerId)).ToList());

        var employeeDetails = await _context.Employees
            .AsNoTracking()
            .Include(e => e.User)
            .Include(e => e.Department)
            .Where(e => employeeIds.Contains(e.Id))
            .ToListAsync();

        var fromDateTime = fromDate.ToDateTime(TimeOnly.MinValue);
        var toDateTime = toDate.ToDateTime(TimeOnly.MinValue);

        var attendances = await _context.Attendances
            .AsNoTracking()
            .Include(a => a.Sessions)
            .Where(a => employeeIds.Contains(a.EmployeeId) && a.AttendanceDate >= fromDateTime && a.AttendanceDate <= toDateTime)
            .ToListAsync();

        var leaves = await _context.LeaveRequests
            .AsNoTracking()
            .Where(l => employeeIds.Contains(l.EmployeeId) && l.Status == LeaveStatus.Approved &&
                        l.FromDate <= toDateTime && l.ToDate >= fromDateTime)
            .ToListAsync();

        var rows = new List<object?[]>();

        for (var date = fromDate; date <= toDate; date = date.AddDays(1))
        {
            var dateTime = date.ToDateTime(TimeOnly.MinValue);
            var isWeeklyOff = date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;

            foreach (var employee in employeeDetails)
            {
                var attendance = attendances.FirstOrDefault(a => a.EmployeeId == employee.Id && a.AttendanceDate.Date == dateTime.Date);
                var onLeave = leaves.Any(l => l.EmployeeId == employee.Id && dateTime >= l.FromDate.Date && dateTime <= l.ToDate.Date);

                string status;
                if (onLeave) status = "On Leave";
                else if (attendance != null) status = "Present";
                else if (isWeeklyOff) status = "Weekly Off";
                else status = "Absent";

                var firstCheckIn = attendance?.Sessions.OrderBy(s => s.CheckInTime).FirstOrDefault()?.CheckInTime;
                var lastCheckOut = attendance?.Sessions.Where(s => s.CheckOutTime.HasValue).OrderByDescending(s => s.CheckOutTime).FirstOrDefault()?.CheckOutTime;

                rows.Add(new object?[]
                {
                    date.ToString(),
                    employee.EmployeeCode,
                    $"{employee.User.FirstName} {employee.User.LastName}",
                    employee.Department.DepartmentName,
                    status,
                    firstCheckIn?.ToString("HH:mm") ?? "",
                    lastCheckOut?.ToString("HH:mm") ?? "",
                    attendance?.TotalWorkingHours ?? 0
                });
            }
        }

        return CsvWriter.Write(
            new[] { "Date", "Employee Code", "Employee Name", "Department", "Status", "First Check-In", "Last Check-Out", "Total Working Hours" },
            rows);
    }

    public async Task<string> GetLeaveReportAsync(Guid userId, DateOnly? fromDate, DateOnly? toDate, string? status)
    {
        var requester = await _employeeRepository.GetByUserIdAsync(userId);

        if (requester == null)
        {
            return CsvWriter.Write(new[] { "Message" }, new[] { new object?[] { "Employee not found." } });
        }

        var query =
            from lr in _context.LeaveRequests.AsNoTracking()
            join e in _context.Employees.AsNoTracking() on lr.EmployeeId equals e.Id
            join u in _context.Users.AsNoTracking() on e.UserId equals u.Id
            join lt in _context.LeaveTypes.AsNoTracking() on lr.LeaveTypeId equals lt.Id
            where lr.IsActive && (e.ManagerId == requester.Id || lr.EmployeeId == requester.Id)
            select new
            {
                lr.FromDate,
                lr.ToDate,
                lr.TotalDays,
                lr.LeaveDuration,
                lr.HalfDayPeriod,
                lr.Status,
                lr.Reason,
                lr.CreatedAt,
                EmployeeName = u.FirstName + " " + u.LastName,
                LeaveTypeName = lt.LeaveName
            };

        if (fromDate.HasValue)
        {
            var from = fromDate.Value.ToDateTime(TimeOnly.MinValue);
            query = query.Where(l => l.ToDate >= from);
        }

        if (toDate.HasValue)
        {
            var to = toDate.Value.ToDateTime(TimeOnly.MinValue);
            query = query.Where(l => l.FromDate <= to);
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<LeaveStatus>(status, true, out var parsedStatus))
        {
            query = query.Where(l => l.Status == parsedStatus);
        }

        var leaves = await query.OrderByDescending(l => l.CreatedAt).ToListAsync();

        var rows = leaves.Select(l => new object?[]
        {
            l.EmployeeName,
            l.LeaveTypeName,
            l.FromDate.ToString("yyyy-MM-dd"),
            l.ToDate.ToString("yyyy-MM-dd"),
            l.TotalDays,
            l.LeaveDuration.ToString(),
            l.HalfDayPeriod?.ToString() ?? "",
            l.Status.ToString(),
            l.Reason,
            l.CreatedAt.ToString("yyyy-MM-dd")
        });

        return CsvWriter.Write(
            new[] { "Employee Name", "Leave Type", "From Date", "To Date", "Total Days", "Duration", "Half Day Period", "Status", "Reason", "Applied On" },
            rows);
    }

    public async Task<string> GetPerformanceReviewReportAsync(Guid userId, Guid? cycleId)
    {
        var requester = await _employeeRepository.GetByUserIdAsync(userId);

        if (requester == null)
        {
            return CsvWriter.Write(new[] { "Message" }, new[] { new object?[] { "Employee not found." } });
        }

        var query = _context.EmployeePerformanceReviews
            .AsNoTracking()
            .Include(r => r.Employee).ThenInclude(e => e.User)
            .Include(r => r.PerformanceCycle)
            .Include(r => r.SkillReviews)
            .Where(r => r.IsActive && (r.ManagerId == requester.Id || r.EmployeeId == requester.Id));

        if (cycleId.HasValue)
        {
            query = query.Where(r => r.PerformanceCycleId == cycleId.Value);
        }

        var reviews = await query.OrderByDescending(r => r.PerformanceCycle.ReviewPeriodStart).ToListAsync();

        var rows = reviews.Select(r =>
        {
            var selfScore = r.SkillReviews.Where(s => s.EmployeeRating.HasValue).Select(s => s.EmployeeRating!.Value).ToList();
            var managerScore = r.SkillReviews.Where(s => s.ManagerRating.HasValue).Select(s => s.ManagerRating!.Value).ToList();

            var statusLabel = r.Status switch { 1 => "Submitted", 2 => "Completed", _ => "Not Started" };

            return new object?[]
            {
                $"{r.Employee.User.FirstName} {r.Employee.User.LastName}",
                r.PerformanceCycle.CycleName,
                statusLabel,
                selfScore.Count == 0 ? "" : Math.Round(selfScore.Average(), 1),
                managerScore.Count == 0 ? "" : Math.Round(managerScore.Average(), 1),
                r.SubmittedOn?.ToString("yyyy-MM-dd") ?? "",
                r.ManagerReviewedOn?.ToString("yyyy-MM-dd") ?? ""
            };
        });

        return CsvWriter.Write(
            new[] { "Employee Name", "Cycle Name", "Status", "Self Score", "Manager Score", "Submitted On", "Manager Reviewed On" },
            rows);
    }
}
