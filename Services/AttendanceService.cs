using HRM.API.Models;
using HRM.API.Repositories;
using HRM.API.Responses;

public interface IAttendanceService
{
    Task<ApiResponse<string>> CheckInAsync(Guid userId
        );

    Task<ApiResponse<string>> CheckOutAsync(Guid userId
        );

    Task<ApiResponse<TodayAttendanceResponse>>
        GetTodayAttendanceAsync(Guid userId, DateTime attendanceDate);

    Task<ApiResponse<List<AttendanceResponse>>>
        GetAttendanceHistoryAsync(Guid employeeId);

    Task<ApiResponse<List<AttendanceResponse>>>
        GetAttendanceCalendarAsync(
            Guid employeeId,
            int month,
            int year);
    Task<AttendanceSummaryResponse> GetAttendanceSummaryAsync(Guid userId, DateTime attendanceDate);
    Task<WeeklyAttendanceSummaryResponse> GetWeeklySummaryAsync(Guid userId, DateTime attendanceDate);
    Task<ApiResponse<TeamAttendanceSummaryResponse>> GetTeamAttendanceSummaryAsync(Guid userId, DateTime attendanceDate, Guid companyId);
    Task<ApiResponse<TeamAttendanceResponse>> GetTeamAttendanceAsync(
       Guid userId,
       TeamAttendanceFilterDto request,
       Guid companyId);

}
public class AttendanceService : IAttendanceService
{
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly IEmployeeRepository _employeeRepository;

    public AttendanceService(IAttendanceRepository attendanceRepository, IEmployeeRepository employeeRepository)
    {
        _attendanceRepository = attendanceRepository;
        _employeeRepository = employeeRepository;
    }
    public async Task<ApiResponse<TodayAttendanceResponse>>
    GetTodayAttendanceAsync(Guid userId, DateTime attendanceDate)
    {
        try
        {
            bool isToday = attendanceDate.Date == DateTime.UtcNow.Date;

            var employee = await _employeeRepository.GetByUserIdAsync(userId);
            var attendance =
                await _attendanceRepository
                    .GetAttendanceByDateAsync(employee.Id, attendanceDate);

            if (attendance == null)
            {
                return new ApiResponse<TodayAttendanceResponse>
                {
                    Success = true,
                    Message = "No attendance found for this date.",
                    Data = new TodayAttendanceResponse
                    {
                        AttendanceDate = attendanceDate.Date,
                        CanCheckIn = isToday,
                        CanCheckOut = false,
                        TotalWorkingHours = 0,
                        Sessions = new List<AttendanceSessionResponse>()
                    }
                };
            }

            var sessions =
                await _attendanceRepository
                    .GetTodaySessionsAsync(attendance.Id);

            bool hasOpenSession =
                sessions.Any(x => x.CheckOutTime == null);

            var response =
                new TodayAttendanceResponse
                {
                    AttendanceId = attendance.Id,
                    AttendanceDate = attendance.AttendanceDate,
                    TotalWorkingHours =
                        attendance.TotalWorkingHours,

                    CanCheckIn = isToday && !hasOpenSession,

                    CanCheckOut = isToday && hasOpenSession,

                    Sessions = sessions.Select(x =>
                        new AttendanceSessionResponse
                        {
                            Id = x.Id,
                            SessionNumber = x.SessionNumber,
                            CheckInTime = x.CheckInTime,
                            CheckOutTime = x.CheckOutTime,
                            WorkingHours = x.WorkingHours,
                            Status = x.Status
                        }).ToList()
                };

            return new ApiResponse<TodayAttendanceResponse>
            {
                Success = true,
                Message = "Attendance fetched successfully.",
                Data = response
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<TodayAttendanceResponse>
            {
                Success = false,
                Message = "Error while fetching attendance.",
                Errors = new List<string>
            {
                ex.Message
            }
            };
        }
    }
    public async Task<AttendanceSummaryResponse> GetAttendanceSummaryAsync(Guid userId, DateTime attendanceDate)

    {
        var employee = await _employeeRepository.GetByUserIdAsync(userId);
        return await _attendanceRepository.GetAttendanceSummaryAsync(employee.Id, attendanceDate);
    }
    public async Task<ApiResponse<List<AttendanceResponse>>>
        GetAttendanceHistoryAsync(Guid employeeId)
    {
        try
        {
            var attendances =
                await _attendanceRepository
                    .GetAttendanceHistoryAsync(employeeId);

            var response =
                new List<AttendanceResponse>();

            foreach (var attendance in attendances)
            {
                var sessions =
                    await _attendanceRepository
                        .GetTodaySessionsAsync(attendance.Id);

                response.Add(new AttendanceResponse
                {
                    Id = attendance.Id,

                    AttendanceDate =
                        attendance.AttendanceDate,

                    TotalWorkingHours =
                        attendance.TotalWorkingHours,

                    TotalSessions =
                        sessions.Count
                });
            }

            return new ApiResponse<List<AttendanceResponse>>
            {
                Success = true,
                Message = "Attendance history fetched successfully.",
                Data = response
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<List<AttendanceResponse>>
            {
                Success = false,
                Message = "Error while fetching attendance history.",
                Errors = new List<string>
            {
                ex.Message
            }
            };
        }
    }
    public async Task<ApiResponse<List<AttendanceResponse>>>
        GetAttendanceCalendarAsync(
            Guid employeeId,
            int month,
            int year)
    {
        try
        {
            var attendances =
                await _attendanceRepository
                    .GetAttendanceCalendarAsync(
                        employeeId,
                        month,
                        year);

            var response =
                new List<AttendanceResponse>();

            foreach (var attendance in attendances)
            {
                var sessions =
                    await _attendanceRepository
                        .GetTodaySessionsAsync(attendance.Id);

                response.Add(new AttendanceResponse
                {
                    Id = attendance.Id,

                    AttendanceDate =
                        attendance.AttendanceDate,

                    TotalWorkingHours =
                        attendance.TotalWorkingHours,

                    TotalSessions =
                        sessions.Count
                });
            }

            return new ApiResponse<List<AttendanceResponse>>
            {
                Success = true,
                Message = "Attendance calendar fetched successfully.",
                Data = response
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<List<AttendanceResponse>>
            {
                Success = false,
                Message = "Error while fetching attendance calendar.",
                Errors = new List<string>
            {
                ex.Message
            }
            };
        }
    }
    public async Task<ApiResponse<string>> CheckOutAsync(
      Guid userId)
    {
        try
        {
            var employee = await _employeeRepository.GetByUserIdAsync(userId);
            var session =
                await _attendanceRepository
                    .GetOpenSessionAsync(
                        employee.Id);

            if (session == null)
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "No active session found."
                };
            }

            session.CheckOutTime = DateTime.Now;

            session.WorkingHours =
                Convert.ToDecimal(
                    (session.CheckOutTime.Value -
                     session.CheckInTime)
                    .TotalHours);

            session.Status = "Completed";

            session.UpdatedAt = DateTime.Now;
            session.UpdatedBy = userId;

            await _attendanceRepository
                .UpdateAttendanceSessionAsync(session);

            var attendance =
                await _attendanceRepository
                    .GetTodayAttendanceAsync(
                        employee.Id);

            var sessions =
                await _attendanceRepository
                    .GetTodaySessionsAsync(
                        attendance!.Id);

            attendance.TotalWorkingHours =
                sessions.Sum(x => x.WorkingHours);

            attendance.UpdatedAt =
                DateTime.UtcNow;

            attendance.UpdatedBy =
                userId;

            await _attendanceRepository
                .UpdateAttendanceAsync(attendance);

            return new ApiResponse<string>
            {
                Success = true,
                Message = "Checked out successfully."
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<string>
            {
                Success = false,
                Message = "Error while checkout.",
                Errors = new List<string>
            {
                ex.Message
            }
            };
        }
    }
    public async Task<ApiResponse<string>> CheckInAsync(
       Guid userId)
    {
        try
        {
            var employee = await _employeeRepository.GetByUserIdAsync(userId);
            var openSession =
                await _attendanceRepository
                    .GetOpenSessionAsync(
                        employee.Id);

            if (openSession != null)
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    Message =
                        "Please checkout current session first."
                };
            }

            var attendance =
                await _attendanceRepository
                    .GetTodayAttendanceAsync(
                         employee.Id);

            if (attendance == null)
            {
                attendance = new Attendance
                {
                    Id = Guid.NewGuid(),
                    EmployeeId = employee.Id,
                    AttendanceDate = DateTime.Now,
                    TotalWorkingHours = 0,
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    CreatedBy = userId
                };

                await _attendanceRepository
                    .AddAttendanceAsync(attendance);
            }

            int sessionNumber =
                await _attendanceRepository
                    .GetLatestSessionNumberAsync(
                        attendance.Id);

            var session = new AttendanceSession
            {
                Id = Guid.NewGuid(),
                AttendanceId = attendance.Id,
                SessionNumber = sessionNumber + 1,
                CheckInTime = DateTime.Now,
                Status = "InProgress",
                IsActive = true,
                CreatedAt = DateTime.Now,
                CreatedBy = userId
            };

            await _attendanceRepository
                .AddAttendanceSessionAsync(session);

            return new ApiResponse<string>
            {
                Success = true,
                Message = "Checked in successfully."
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<string>
            {
                Success = false,
                Message = "Error while check in.",
                Errors = new List<string>
            {
                ex.Message
            }
            };
        }
    }
    public async Task<WeeklyAttendanceSummaryResponse> GetWeeklySummaryAsync(Guid userId, DateTime attendanceDate)
    {
        var employee = await _employeeRepository.GetByUserIdAsync(userId);
        return await _attendanceRepository.GetWeeklySummaryAsync(employee.Id, attendanceDate);
    }
    public async Task<ApiResponse<TeamAttendanceSummaryResponse>>
GetTeamAttendanceSummaryAsync(Guid userId, DateTime attendanceDate, Guid companyId)
    {
        try
        {
            var employee = await _employeeRepository.GetByUserIdAsync(userId);

            if (employee == null)
            {
                return new ApiResponse<TeamAttendanceSummaryResponse>
                {
                    Success = false,
                    Message = "Employee not found."
                };
            }

            var data = await _attendanceRepository
                .GetTeamAttendanceSummaryAsync(employee.Id, attendanceDate, companyId);

            return new ApiResponse<TeamAttendanceSummaryResponse>
            {
                Success = true,
                Message = "Team attendance summary retrieved successfully.",
                Data = data
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<TeamAttendanceSummaryResponse>
            {
                Success = false,
                Message = ex.Message,
                Errors = new List<string>
            {
                ex.ToString()
            }
            };
        }
    }
    public async Task<ApiResponse<TeamAttendanceResponse>>
GetTeamAttendanceAsync(
    Guid userId,
    TeamAttendanceFilterDto request,
    Guid companyId)
    {
        try
        {
            var result = await _attendanceRepository
                .GetTeamAttendanceAsync(userId, request,companyId);

            return new ApiResponse<TeamAttendanceResponse>
            {
                Success = true,
                Message = "Team attendance retrieved successfully.",
                Data = result
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<TeamAttendanceResponse>
            {
                Success = false,
                Message = "Failed to retrieve team attendance.",
                Errors = new List<string>
            {
                ex.Message
            }
            };
        }
    }
}