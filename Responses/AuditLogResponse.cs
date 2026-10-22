namespace HRM.API.Responses;

public class AuditLogListItemResponse
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public Guid? TargetUserId { get; set; }
    public string? TargetUserName { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Status { get; set; }
}

public class AuditLogPagedResponse
{
    public int TotalRecords { get; set; }
    public List<AuditLogListItemResponse> Records { get; set; } = new();
}

public class AuditLogDetailResponse : AuditLogListItemResponse
{
    public string EntityType { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public Dictionary<string, string>? OldValues { get; set; }
    public Dictionary<string, string>? NewValues { get; set; }
}

public class AuditLogSummaryResponse
{
    public int TotalActivities { get; set; }
    public int TodayActivities { get; set; }
    public int EmployeeChanges { get; set; }
    public int LeaveActivities { get; set; }
}
