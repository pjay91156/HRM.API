using HRM.API.Data;
using HRM.API.Enums;
using HRM.API.Responses;
using Microsoft.EntityFrameworkCore;

public interface IDashboardRepository
{
    Task<DashboardResponse> GetDashboardAsync(Guid companyId);
    Task<List<HeadcountChartResponse>> GetHeadcountTrendAsync(Guid companyId);
    Task<List<DepartmentEmployeeCountResponse>> GetDepartmentEmployeeCountsAsync();
    Task<DashboardTodayAttendanceResponse> GetTodayAttendanceSummaryAsync(Guid companyId);
    Task<PendingApprovalsResponse> GetPendingApprovalsSummaryAsync(Guid managerId);
}

public class DashboardRepository : IDashboardRepository
{
    private readonly ApplicationDbContext _context;

    public DashboardRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<List<DepartmentEmployeeCountResponse>>
    GetDepartmentEmployeeCountsAsync()
{
    return await _context.Employees
        .Where(e => e.IsActive)
        .Join(
            _context.Departments,
            employee => employee.DepartmentId,
            department => department.Id,
            (employee, department) => new
            {
                department.DepartmentName
            })
        .GroupBy(x => x.DepartmentName)
        .Select(g => new DepartmentEmployeeCountResponse
        {
            DepartmentName = g.Key,
            EmployeeCount = g.Count()
        })
        .OrderByDescending(x => x.EmployeeCount)
        .ToListAsync();
}

    public async Task<DashboardResponse> GetDashboardAsync(Guid companyId)
    {
        var today = DateTime.UtcNow.Date;

        var totalEmployees = await _context.Employees.CountAsync(e => e.User.CompanyId == companyId && e.IsActive);

        var totalDepartments = await _context.Departments
            .CountAsync(x => x.CompanyId == companyId);

        var totalDesignations = await _context.Designations
                                 .CountAsync(d =>
                                     d.IsActive &&
                                     d.Department.IsActive &&
                                     d.Department.CompanyId == companyId);

        // Employee count till today
        var currentEmployeeCount = totalEmployees;

        var oneYearAgo = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1));

        var employeeCountOneYearAgo = await _context.Employees
            .CountAsync(e =>
                e.IsActive &&
                e.User.CompanyId == companyId &&
                e.JoiningDate <= oneYearAgo);

        decimal growthRate = 0;

        if (employeeCountOneYearAgo > 0)
        {
            growthRate =
                ((decimal)(currentEmployeeCount - employeeCountOneYearAgo)
                / employeeCountOneYearAgo) * 100;
        }
        else
        {
            growthRate = currentEmployeeCount > 0 ? 100 : 0;
        }

        return new DashboardResponse
        {
            TotalEmployees = totalEmployees,
            TotalDepartments = totalDepartments,
            TotalDesignations = totalDesignations,
            GrowthRate = Math.Round(growthRate, 2),
        };
    }
    public async Task<List<HeadcountChartResponse>> GetHeadcountTrendAsync(Guid companyId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var chartData = new List<HeadcountChartResponse>();

        // Last 6 months including current month
        for (int i = 5; i >= 0; i--)
        {
            var month = today.AddMonths(-i);

            var monthEnd = new DateOnly(
                month.Year,
                month.Month,
                DateTime.DaysInMonth(month.Year, month.Month));

            var headcount = await _context.Employees
                .CountAsync(e =>
                    e.IsActive &&
                    e.User.CompanyId == companyId &&
                    e.JoiningDate <= monthEnd);

            chartData.Add(new HeadcountChartResponse
            {
                Name = month.ToDateTime(TimeOnly.MinValue).ToString("MMM"),
                Headcount = headcount
            });
        }

        return chartData;
    }

    public async Task<DashboardTodayAttendanceResponse> GetTodayAttendanceSummaryAsync(Guid companyId)
    {
        var today = DateTime.UtcNow.Date;

        var totalEmployees = await _context.Employees
            .CountAsync(e => e.IsActive && e.User.CompanyId == companyId);

        var presentCount = await _context.Attendances
            .Where(a =>
                a.AttendanceDate.Date == today &&
                a.Employee.IsActive &&
                a.Employee.User.CompanyId == companyId)
            .Select(a => a.EmployeeId)
            .Distinct()
            .CountAsync();

        var onLeaveCount = await _context.LeaveRequests
            .Join(
                _context.Employees,
                leaveRequest => leaveRequest.EmployeeId,
                employee => employee.Id,
                (leaveRequest, employee) => new { leaveRequest, employee })
            .Where(x =>
                x.leaveRequest.Status == LeaveStatus.Approved &&
                x.leaveRequest.FromDate.Date <= today &&
                x.leaveRequest.ToDate.Date >= today &&
                x.employee.IsActive &&
                x.employee.User.CompanyId == companyId)
            .Select(x => x.leaveRequest.EmployeeId)
            .Distinct()
            .CountAsync();

        var absentCount = Math.Max(0, totalEmployees - presentCount - onLeaveCount);

        var attendanceRate = totalEmployees > 0
            ? Math.Round((decimal)presentCount / totalEmployees * 100, 2)
            : 0;

        return new DashboardTodayAttendanceResponse
        {
            TotalEmployees = totalEmployees,
            PresentCount = presentCount,
            OnLeaveCount = onLeaveCount,
            AbsentCount = absentCount,
            AttendanceRate = attendanceRate
        };
    }

    public async Task<PendingApprovalsResponse> GetPendingApprovalsSummaryAsync(Guid managerId)
    {
        var pendingLeaveRequests = await _context.LeaveRequests
            .Join(
                _context.Employees,
                leaveRequest => leaveRequest.EmployeeId,
                employee => employee.Id,
                (leaveRequest, employee) => new { leaveRequest, employee })
            .CountAsync(x =>
                x.leaveRequest.Status == LeaveStatus.Pending &&
                x.employee.ManagerId == managerId);

        // Each regularization request can have multiple session-change rows
        // (one per Add/Edit/Delete), so count distinct attendance requests
        // rather than raw rows to match what the approvals list shows.
        var pendingRegularizations = await _context.AttendanceRegularizations
            .Where(r =>
                r.IsActive &&
                r.Status == (int)AttendanceRegularizationStatus.Pending &&
                r.Attendance.Employee.ManagerId == managerId)
            .Select(r => r.AttendanceId)
            .Distinct()
            .CountAsync();

        return new PendingApprovalsResponse
        {
            PendingLeaveRequests = pendingLeaveRequests,
            PendingRegularizations = pendingRegularizations
        };
    }
}