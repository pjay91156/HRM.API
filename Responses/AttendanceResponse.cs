public class AttendanceSessionResponse
{
    public Guid Id { get; set; }

    public int SessionNumber { get; set; }

    public DateTime CheckInTime { get; set; }

    public DateTime? CheckOutTime { get; set; }

    public decimal WorkingHours { get; set; }

    public string Status { get; set; } = string.Empty;
}
public class TodayAttendanceResponse
{
    public Guid AttendanceId { get; set; }

    public DateTime AttendanceDate { get; set; }

    public decimal TotalWorkingHours { get; set; }

    public bool CanCheckIn { get; set; }

    public bool CanCheckOut { get; set; }

    public List<AttendanceSessionResponse> Sessions
        { get; set; } = new();
}
public class AttendanceResponse
{
    public Guid Id { get; set; }

    public DateTime AttendanceDate { get; set; }

    public decimal TotalWorkingHours { get; set; }

    public int TotalSessions { get; set; }
}
public class AttendanceSummaryResponse
{
    public DateTime? FirstCheckIn { get; set; }

    public DateTime? LastCheckOut { get; set; }

    public decimal TotalWorkingHours { get; set; }
}
public class WeeklyAttendanceSummaryResponse
{
    public List<WeeklyAttendanceDayResponse> Days { get; set; } = new();
}
public class WeeklyAttendanceDayResponse
{
    public DateTime Date { get; set; }

    public decimal WorkingHours { get; set; }

    public bool IsPresent { get; set; }
}
public class TeamAttendanceSummaryResponse
{
    public int TotalEmployees { get; set; }

    public int PresentEmployees { get; set; }

    public int AbsentEmployees { get; set; }

    public int OnLeaveEmployees { get; set; }
}
public class TeamAttendanceGridResponse
{
    public Guid EmployeeId { get; set; }

    public string EmployeeCode { get; set; } = string.Empty;

    public string EmployeeName { get; set; } = string.Empty;

    public string ReportingManager { get; set; } = string.Empty;

    public DateTime? FirstCheckIn { get; set; }

    public DateTime? LastCheckOut { get; set; }

    public decimal TotalWorkingHours { get; set; }

    public string Status { get; set; } = string.Empty;
}
public class TeamAttendanceResponse
{
    public int TotalRecords { get; set; }

    public List<TeamAttendanceGridResponse> Records
    {
        get;
        set;
    } = new();
}