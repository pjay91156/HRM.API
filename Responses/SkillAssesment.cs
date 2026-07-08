namespace HRM.API.Responses;

public class PerformanceCycleResponse
{
    public Guid Id { get; set; }

    public string CycleName { get; set; } = string.Empty;

    public DateOnly ReviewPeriodStart { get; set; }

    public DateOnly ReviewPeriodEnd { get; set; }

    public DateOnly EmployeeReviewStart { get; set; }

    public DateOnly EmployeeReviewEnd { get; set; }

    public DateOnly ManagerReviewStart { get; set; }

    public DateOnly ManagerReviewEnd { get; set; }

    public byte Status { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }
}