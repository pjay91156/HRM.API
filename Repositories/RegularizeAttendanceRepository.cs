
using HRM.API.Data;
using HRM.API.Responses;
using Microsoft.EntityFrameworkCore;
using HRM.API.Models;
using HRM.API.DTOs;


public interface IRegularizeAttendanceRepository
{
    Task<AttendanceSessionsResponse?> GetAttendanceSessionsAsync(
        Guid employeeId,
        DateOnly attendanceDate);
    Task<bool> CreateRegularizationRequestAsync(
        RegularizationRequest request,
        Guid userId);
    Task<List<ManagerPendingRegularizationResponse>> GetPendingRequestsAsync(Guid managerId);
    Task<RegularizationDetailsResponseDto?> GetRegularizationDetailsAsync(Guid managerId, Guid attendanceId, Guid employeeId);
    Task<bool> ApproveRejectRegularizationAsync(Guid approverId, RegularizationApprovalRequestDto request);
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

    public async Task<RegularizationDetailsResponseDto?> GetRegularizationDetailsAsync(Guid managerId, Guid attendanceId, Guid employeeId)
    {
        var regularizations = await _context.AttendanceRegularizations
            .AsNoTracking()
            .Include(x => x.Attendance)
                .ThenInclude(x => x.Employee)
                    .ThenInclude(x => x.User)
            .Include(x => x.Attendance)
                .ThenInclude(x => x.Employee)
                    .ThenInclude(x => x.Department)
            .Include(x => x.Attendance)
                .ThenInclude(x => x.Sessions)
            .Include(x => x.AttendanceSession)
            .Where(x =>
                x.AttendanceId == attendanceId &&
                x.Attendance.EmployeeId == employeeId &&
                x.IsActive &&
                x.Attendance.Employee.ManagerId == managerId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();

        if (!regularizations.Any())
            return null;

        var first = regularizations.First();
        var attendance = first.Attendance;
        var employee = attendance.Employee;

        var existingSessions = attendance.Sessions
            .Where(s => s.IsActive)
            .OrderBy(s => s.SessionNumber)
            .ToList();

        // Simulate the session list after the requested changes to compute the proposed total.
        var simulatedHours = existingSessions.ToDictionary(s => s.Id, s => s.WorkingHours);

        var newSessionIndex = 0;
        var changes = new List<RegularizationDetailItemDto>();

        foreach (var change in regularizations)
        {
            var changeType = (AttendanceRegularizationChangeType)change.ChangeType;

            switch (changeType)
            {
                case AttendanceRegularizationChangeType.Edit when change.AttendanceSession != null:
                    simulatedHours[change.AttendanceSession.Id] =
                        CalculateWorkingHours(change.AfterCheckIn, change.AfterCheckOut);
                    break;

                case AttendanceRegularizationChangeType.Delete when change.AttendanceSession != null:
                    simulatedHours.Remove(change.AttendanceSession.Id);
                    break;

                case AttendanceRegularizationChangeType.Add:
                    simulatedHours[Guid.NewGuid()] =
                        CalculateWorkingHours(change.AfterCheckIn, change.AfterCheckOut);
                    break;
            }

            changes.Add(BuildChangeItem(change, changeType, existingSessions.Count, ref newSessionIndex));
        }

        var currentWorkingHours = existingSessions.Sum(s => s.WorkingHours);
        var proposedWorkingHours = simulatedHours.Values.Sum();

        return new RegularizationDetailsResponseDto
        {
            AttendanceId = attendanceId,
            EmployeeId = employeeId,
            EmployeeCode = employee.EmployeeCode,
            EmployeeName = $"{employee.User.FirstName} {employee.User.LastName}".Trim(),
            DepartmentName = employee.Department?.DepartmentName ?? string.Empty,
            AttendanceDate = DateOnly.FromDateTime(attendance.AttendanceDate),
            RequestedOn = regularizations.Max(x => x.CreatedAt),
            Reason = first.Reason,
            Status = ((AttendanceRegularizationStatus)first.Status).ToString(),
            CurrentWorkingTime = FormatWorkingTime(currentWorkingHours),
            ProposedWorkingTime = FormatWorkingTime(proposedWorkingHours),
            ExistingSessions = existingSessions.Select(s => new ExistingSessionDto
            {
                SessionNumber = s.SessionNumber,
                CheckIn = s.CheckInTime.ToString("hh:mm tt"),
                CheckOut = s.CheckOutTime?.ToString("hh:mm tt") ?? "-",
                WorkingTime = FormatWorkingTime(s.WorkingHours)
            }).ToList(),
            Changes = changes
        };
    }

    private static RegularizationDetailItemDto BuildChangeItem(
        AttendanceRegularization change,
        AttendanceRegularizationChangeType changeType,
        int existingSessionCount,
        ref int newSessionIndex)
    {
        var session = change.AttendanceSession;

        var beforeRange = session != null
            ? FormatRange(session.CheckInTime, session.CheckOutTime)
            : "-";

        var afterRange = change.AfterCheckIn.HasValue
            ? FormatRange(change.AfterCheckIn, change.AfterCheckOut)
            : "-";

        switch (changeType)
        {
            case AttendanceRegularizationChangeType.Edit:
                return new RegularizationDetailItemDto
                {
                    ChangeType = changeType.ToString(),
                    SessionNumber = session?.SessionNumber,
                    IsNewSession = false,
                    Current = beforeRange,
                    Requested = afterRange,
                    ChangeDescription = BuildEditDescription(session, change)
                };

            case AttendanceRegularizationChangeType.Delete:
                return new RegularizationDetailItemDto
                {
                    ChangeType = changeType.ToString(),
                    SessionNumber = session?.SessionNumber,
                    IsNewSession = false,
                    Current = beforeRange,
                    Requested = "-",
                    ChangeDescription = "Session will be deleted"
                };

            default: // Add
                newSessionIndex++;
                return new RegularizationDetailItemDto
                {
                    ChangeType = changeType.ToString(),
                    SessionNumber = existingSessionCount + newSessionIndex,
                    IsNewSession = true,
                    Current = "-",
                    Requested = afterRange,
                    ChangeDescription = "New session"
                };
        }
    }

    private static string BuildEditDescription(AttendanceSession? session, AttendanceRegularization change)
    {
        var parts = new List<string>();

        if (session != null)
        {
            var beforeCheckIn = TimeOnly.FromDateTime(session.CheckInTime);
            if (change.AfterCheckIn.HasValue && change.AfterCheckIn.Value != beforeCheckIn)
            {
                parts.Add($"Check In {beforeCheckIn:HH:mm} → {change.AfterCheckIn:HH:mm}");
            }

            if (session.CheckOutTime.HasValue)
            {
                var beforeCheckOut = TimeOnly.FromDateTime(session.CheckOutTime.Value);
                if (change.AfterCheckOut.HasValue && change.AfterCheckOut.Value != beforeCheckOut)
                {
                    parts.Add($"Check Out {beforeCheckOut:HH:mm} → {change.AfterCheckOut:HH:mm}");
                }
            }
        }

        return parts.Count > 0 ? string.Join(", ", parts) : "Session updated";
    }

    private static string FormatRange(DateTime checkIn, DateTime? checkOut)
    {
        var start = checkIn.ToString("hh:mm tt");
        var end = checkOut?.ToString("hh:mm tt") ?? "-";
        return $"{start} - {end}";
    }

    private static string FormatRange(TimeOnly? checkIn, TimeOnly? checkOut)
    {
        var start = checkIn?.ToString("hh:mm tt") ?? "-";
        var end = checkOut?.ToString("hh:mm tt") ?? "-";
        return $"{start} - {end}";
    }

    private static decimal CalculateWorkingHours(TimeOnly? checkIn, TimeOnly? checkOut)
    {
        if (!checkIn.HasValue || !checkOut.HasValue)
            return 0;

        var diff = checkOut.Value.ToTimeSpan() - checkIn.Value.ToTimeSpan();
        return diff.TotalHours > 0 ? (decimal)diff.TotalHours : 0;
    }

    private static string FormatWorkingTime(decimal hours)
    {
        var totalMinutes = (int)Math.Round(hours * 60);
        var h = totalMinutes / 60;
        var m = totalMinutes % 60;
        return $"{h}h {m:00}m";
    }

    public async Task<bool> ApproveRejectRegularizationAsync(Guid approverId, RegularizationApprovalRequestDto request)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var regularizations = await _context.AttendanceRegularizations
                .Include(x => x.AttendanceSession)
                .Include(x => x.Attendance)
                    .ThenInclude(x => x.Sessions)
                .Where(x => x.AttendanceId == request.AttendanceId
                    && x.Attendance.EmployeeId == request.EmployeeId
                    && x.IsActive
                    && x.Status == (int)AttendanceRegularizationStatus.Pending)
                .ToListAsync();

            if (!regularizations.Any())
                return false;

            if (request.Status == (int)AttendanceRegularizationStatus.Approved)
            {
                ApplyApprovedChanges(regularizations, approverId);
            }

            foreach (var item in regularizations)
            {
                item.Status = request.Status;
                item.UpdatedAt = DateTime.UtcNow;
                item.UpdatedBy = approverId;
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

    private void ApplyApprovedChanges(List<AttendanceRegularization> regularizations, Guid approverId)
    {
        var attendance = regularizations.First().Attendance;
        var attendanceDate = attendance.AttendanceDate.Date;

        var nextSessionNumber = attendance.Sessions
            .Where(s => s.IsActive)
            .Select(s => (int?)s.SessionNumber)
            .Max() ?? 0;

        foreach (var change in regularizations)
        {
            var changeType = (AttendanceRegularizationChangeType)change.ChangeType;

            switch (changeType)
            {
                case AttendanceRegularizationChangeType.Edit when change.AttendanceSession != null:
                    {
                        var session = change.AttendanceSession;

                        if (change.AfterCheckIn.HasValue)
                            session.CheckInTime = attendanceDate + change.AfterCheckIn.Value.ToTimeSpan();

                        if (change.AfterCheckOut.HasValue)
                            session.CheckOutTime = attendanceDate + change.AfterCheckOut.Value.ToTimeSpan();

                        session.WorkingHours = CalculateWorkingHours(session.CheckInTime, session.CheckOutTime);
                        session.Status = session.CheckOutTime.HasValue ? "Completed" : "InProgress";
                        session.UpdatedAt = DateTime.UtcNow;
                        session.UpdatedBy = approverId;
                        break;
                    }

                case AttendanceRegularizationChangeType.Delete when change.AttendanceSession != null:
                    {
                        change.AttendanceSession.IsActive = false;
                        change.AttendanceSession.UpdatedAt = DateTime.UtcNow;
                        change.AttendanceSession.UpdatedBy = approverId;
                        break;
                    }

                case AttendanceRegularizationChangeType.Add when change.AfterCheckIn.HasValue:
                    {
                        nextSessionNumber++;

                        var checkIn = attendanceDate + change.AfterCheckIn.Value.ToTimeSpan();
                        var checkOut = change.AfterCheckOut.HasValue
                            ? attendanceDate + change.AfterCheckOut.Value.ToTimeSpan()
                            : (DateTime?)null;

                        var newSession = new AttendanceSession
                        {
                            Id = Guid.NewGuid(),
                            AttendanceId = attendance.Id,
                            SessionNumber = nextSessionNumber,
                            CheckInTime = checkIn,
                            CheckOutTime = checkOut,
                            WorkingHours = CalculateWorkingHours(checkIn, checkOut),
                            Status = checkOut.HasValue ? "Completed" : "InProgress",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = approverId
                        };

                        _context.AttendanceSessions.Add(newSession);
                        attendance.Sessions.Add(newSession);
                        break;
                    }
            }
        }

        attendance.TotalWorkingHours = attendance.Sessions
            .Where(s => s.IsActive)
            .Sum(s => s.WorkingHours);
        attendance.UpdatedAt = DateTime.UtcNow;
        attendance.UpdatedBy = approverId;
    }

    private static decimal CalculateWorkingHours(DateTime checkIn, DateTime? checkOut)
    {
        if (!checkOut.HasValue)
            return 0;

        var diff = checkOut.Value - checkIn;
        return diff.TotalHours > 0 ? (decimal)diff.TotalHours : 0;
    }
}