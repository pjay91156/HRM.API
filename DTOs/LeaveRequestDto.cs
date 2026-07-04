using HRM.API.Enums;
public class ApplyLeaveRequestDto
{

    public Guid LeaveTypeId { get; set; }

    public DateTime FromDate { get; set; }

    public DateTime ToDate { get; set; }

    public LeaveDuration LeaveDuration { get; set; }

    public string Reason { get; set; } = string.Empty;
}
public class LeaveApprovalRequestDto
{
    public Guid LeaveRequestId { get; set; }

    public LeaveStatus Status { get; set; }

    public string? Comments { get; set; }
}