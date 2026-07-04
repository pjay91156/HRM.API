public class AttendanceSessionsResponse
{
    public Guid AttendanceId { get; set; }

    public DateOnly AttendanceDate { get; set; }

    public decimal TotalWorkingHours { get; set; }

    public List<SessionResponse> Sessions { get; set; } = new();
}

public class SessionResponse
{
    public Guid SessionId { get; set; }

    public int SessionNumber { get; set; }

    public DateTime CheckIn { get; set; }

    public DateTime? CheckOut { get; set; }

    public decimal WorkingHours { get; set; }
}
public class ManagerPendingRegularizationResponse
{
    public Guid AttendanceId { get; set; }

    public Guid EmployeeId { get; set; }

    public string EmployeeCode { get; set; } = string.Empty;

    public string EmployeeName { get; set; } = string.Empty;

    public DateOnly AttendanceDate { get; set; }

    public DateTime RequestedOn { get; set; }

    public string Reason { get; set; } = string.Empty;

    public int TotalChanges { get; set; }

    public string ChangeTypes { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;
}