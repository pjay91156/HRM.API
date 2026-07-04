namespace HRM.API.Enums;
public class LeaveRequestResponse
{
    public Guid Id { get; set; }

    public Guid EmployeeId { get; set; }

    public string EmployeeName { get; set; } = string.Empty;

    public string LeaveType { get; set; }    

    public DateTime FromDate { get; set; }

    public DateTime ToDate { get; set; }

    public decimal TotalDays { get; set; }

    public LeaveDuration LeaveDuration { get; set; }

    public HalfDayPeriod? HalfDayPeriod { get; set; }

    public string Reason { get; set; } = string.Empty;

    public LeaveStatus Status { get; set; }

    public string? ApproverComments { get; set; }
  
}
public class TeamLeaveRequestResponse
{
    public Guid LeaveRequestId { get; set; }

    public Guid EmployeeId { get; set; }

    public string EmployeeName { get; set; } = string.Empty;

    public string LeaveType { get; set; } = string.Empty;

    public DateTime FromDate { get; set; }

    public DateTime ToDate { get; set; }

    public decimal TotalDays { get; set; }

    public string Status { get; set; } = string.Empty;

    public string? Reason { get; set; }

    public DateTime AppliedOn { get; set; }
}
public class TeamLeaveCalendarResponse
{
    public Guid LeaveRequestId { get; set; }

    public Guid EmployeeId { get; set; }

    public string EmployeeName { get; set; } = string.Empty;

    public string LeaveType { get; set; } = string.Empty;

    public DateTime FromDate { get; set; }

    public DateTime ToDate { get; set; }

    public decimal TotalDays { get; set; }
}