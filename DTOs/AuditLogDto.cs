using HRM.API.Enums;

public class AuditLogEntryDto
{
    public Guid CompanyId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public AuditAction Action { get; set; }
    public AuditModule Module { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public Guid? TargetUserId { get; set; }
    public string? TargetUserName { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Status { get; set; }
    public object? OldValues { get; set; }
    public object? NewValues { get; set; }
}

public class AuditLogFilterDto
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public Guid? PerformedByEmployeeId { get; set; }
    public Guid? TargetUserId { get; set; }
    public AuditModule? Module { get; set; }
    public AuditAction? Action { get; set; }
    public string? Search { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageLength { get; set; } = 20;
}
