
using HRM.API.Data;
using HRM.API.Responses;
using Microsoft.EntityFrameworkCore;
using HRM.API.Models;



public interface IRegularizeAttendanceRepository
{
    Task<AttendanceSessionsResponse?> GetAttendanceSessionsAsync(
        Guid employeeId,
        DateOnly attendanceDate);
    Task<bool> CreateRegularizationRequestAsync(
        RegularizationRequest request,
        Guid userId);
    Task<List<ManagerPendingRegularizationResponse>> GetPendingRequestsAsync(Guid managerId);
}
public class RegularizeAttendanceRepository : IRegularizeAttendanceRepository
{
    private readonly ApplicationDbContext _context;

    public RegularizeAttendanceRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<bool> CreateRegularizationRequestAsync(
           RegularizationRequest request,
           Guid userId)
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            foreach (var change in request.SessionChanges)
            {
                var entity = new AttendanceRegularization
                {
                    Id = Guid.NewGuid(),

                    AttendanceId = request.AttendanceId,

                    SessionId =
change.ChangeType.Equals("add",
    StringComparison.OrdinalIgnoreCase)
    ? null
    : change.SessionId,

                    ChangeType = change.ChangeType.ToLower() switch
                    {
                        "add" => (int)AttendanceRegularizationChangeType.Add,
                        "edit" => (int)AttendanceRegularizationChangeType.Edit,
                        "delete" => (int)AttendanceRegularizationChangeType.Delete,
                        _ => throw new ArgumentException($"Invalid ChangeType: {change.ChangeType}")
                    },

                    AfterCheckIn =
                        string.IsNullOrWhiteSpace(change.After.CheckIn)
                            ? null
                            : TimeOnly.Parse(change.After.CheckIn),

                    AfterCheckOut =
                        string.IsNullOrWhiteSpace(change.After.CheckOut)
                            ? null
                            : TimeOnly.Parse(change.After.CheckOut),

                    Reason = request.Reason,

                    Status = (int)AttendanceRegularizationStatus.Pending,

                    IsActive = true,

                    CreatedAt = DateTime.UtcNow,

                    CreatedBy = userId
                };

                _context.AttendanceRegularizations.Add(entity);
            }

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

    }

    public async Task<AttendanceSessionsResponse?> GetAttendanceSessionsAsync(
    Guid employeeId,
    DateOnly attendanceDate)
    {
        var attendance = await _context.Attendances
            .AsNoTracking()
            .Where(x =>
                x.EmployeeId == employeeId &&
                x.AttendanceDate == attendanceDate.ToDateTime(TimeOnly.MinValue).Date &&
                x.IsActive)
            .Select(x => new AttendanceSessionsResponse
            {
                AttendanceId = x.Id,
                AttendanceDate = DateOnly.FromDateTime(x.AttendanceDate),
                TotalWorkingHours = x.TotalWorkingHours,

                Sessions = x.Sessions
                    .Where(s => s.IsActive)
                    .OrderBy(s => s.SessionNumber)
                    .Select(s => new SessionResponse
                    {
                        SessionId = s.Id,
                        SessionNumber = s.SessionNumber,
                        CheckIn = s.CheckInTime,
                        CheckOut = s.CheckOutTime,
                        WorkingHours = s.WorkingHours
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        return attendance;
    }
    public async Task<List<ManagerPendingRegularizationResponse>> GetPendingRequestsAsync(Guid managerId)
    {
        // Fetch flat data from SQL
        var regularizations = await _context.AttendanceRegularizations
            .AsNoTracking()
            .Where(x =>
                x.Status == (int)AttendanceRegularizationStatus.Pending &&
                x.IsActive &&
                x.Attendance.Employee.ManagerId == managerId)
            .Select(x => new
            {
                x.AttendanceId,
                EmployeeId = x.Attendance.EmployeeId,
                EmployeeCode = x.Attendance.Employee.EmployeeCode,
                EmployeeName = x.Attendance.Employee.User.FirstName + " " +
                               x.Attendance.Employee.User.LastName,
                x.Attendance.AttendanceDate,
                x.CreatedAt,
                x.Reason,
                x.Status,
                x.ChangeType
            })
            .ToListAsync();

        // Group and project in memory
        var requests = regularizations
            .GroupBy(x => new
            {
                x.AttendanceId,
                x.EmployeeId,
                x.EmployeeCode,
                x.EmployeeName,
                x.AttendanceDate
            })
            .Select(g => new ManagerPendingRegularizationResponse
            {
                AttendanceId = g.Key.AttendanceId,
                EmployeeId = g.Key.EmployeeId,
                EmployeeCode = g.Key.EmployeeCode,
                EmployeeName = g.Key.EmployeeName,
                AttendanceDate = DateOnly.FromDateTime(g.Key.AttendanceDate),

                RequestedOn = g.Max(x => x.CreatedAt),

                // Assuming all rows for the attendance have the same reason
                Reason = g.First().Reason,

                TotalChanges = g.Count(),

                ChangeTypes = string.Join(", ",
                    g.Select(x => (AttendanceRegularizationChangeType)x.ChangeType)
                     .Distinct()
                     .Select(x => x.ToString())),

                Status = ((AttendanceRegularizationStatus)g.First().Status).ToString()
            })
            .OrderByDescending(x => x.RequestedOn)
            .ToList();

        return requests;
    }
}