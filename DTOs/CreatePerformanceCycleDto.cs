

public class CreatePerformanceCycleDto
{
    public string CycleName { get; set; } = string.Empty;

    public DateOnly ReviewPeriodStart { get; set; }

    public DateOnly ReviewPeriodEnd { get; set; }

    public DateOnly EmployeeReviewStart { get; set; }

    public DateOnly EmployeeReviewEnd { get; set; }

    public DateOnly ManagerReviewStart { get; set; }

    public DateOnly ManagerReviewEnd { get; set; }

    public byte Status { get; set; }
}


public class UpdatePerformanceCycleDto
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
}