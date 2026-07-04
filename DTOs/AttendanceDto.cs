public class CheckInRequest
{
    public Guid EmployeeId { get; set; }

    public Guid LoggedInUserId { get; set; }
}
public class CheckOutRequest
{
    public Guid EmployeeId { get; set; }

    public Guid LoggedInUserId { get; set; }
}
public class TeamAttendanceFilterDto
{
    public Guid? ManagerId { get; set; }
    public string? SearchText { get; set; }
   public DateOnly AttendanceDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public int PageNumber { get; set; } = 1;
    public int PageLength { get; set; } = 10;
}
public class RegularizationRequest
{
    public Guid AttendanceId { get; set; }

    public string Reason { get; set; } = string.Empty;

    public List<SessionChangeRequest> SessionChanges { get; set; } = new();
}
public class SessionChangeRequest
{
    public Guid? SessionId { get; set; }

    public string ChangeType { get; set; } = string.Empty;

    public TimeChange Before { get; set; } = new();

    public TimeChange After { get; set; } = new();
}
public class TimeChange
{
    public string? CheckIn { get; set; }

    public string? CheckOut { get; set; }
}