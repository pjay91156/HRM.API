namespace HRM.API.Responses;

public class PerformanceReviewListItemResponse
{
    public Guid Id { get; set; }

    public Guid PerformanceCycleId { get; set; }

    public string CycleName { get; set; } = string.Empty;

    public DateOnly ReviewPeriodStart { get; set; }

    public DateOnly ReviewPeriodEnd { get; set; }

    public DateOnly EmployeeReviewStart { get; set; }

    public DateOnly EmployeeReviewEnd { get; set; }

    public DateOnly ManagerReviewStart { get; set; }

    public DateOnly ManagerReviewEnd { get; set; }

    public string EmployeeName { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public bool IsWindowOpen { get; set; }

    public bool IsHistory { get; set; }
}

public class PerformanceReviewSkillResponse
{
    public Guid SkillId { get; set; }

    public string SkillName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal Weightage { get; set; }

    public decimal? EmployeeRating { get; set; }

    public string? EmployeeComment { get; set; }

    public decimal? ManagerRating { get; set; }

    public string? ManagerComment { get; set; }
}

public class PerformanceReviewCategoryResponse
{
    public Guid CategoryId { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public decimal Weightage { get; set; }

    public List<PerformanceReviewSkillResponse> Skills { get; set; } = new();
}

public class PerformanceReviewDetailResponse
{
    public Guid Id { get; set; }

    public string CycleName { get; set; } = string.Empty;

    public DateOnly ReviewPeriodStart { get; set; }

    public DateOnly ReviewPeriodEnd { get; set; }

    public DateOnly EmployeeReviewStart { get; set; }

    public DateOnly EmployeeReviewEnd { get; set; }

    public DateOnly ManagerReviewStart { get; set; }

    public DateOnly ManagerReviewEnd { get; set; }

    public string EmployeeName { get; set; } = string.Empty;

    public string TemplateName { get; set; } = string.Empty;

    public bool IsEmployeeWindowOpen { get; set; }

    public bool IsManagerWindowOpen { get; set; }

    public bool IsEmployeeSubmitted { get; set; }

    public bool IsManagerCompleted { get; set; }

    public string? OverallEmployeeComment { get; set; }

    public string? OverallManagerComment { get; set; }

    public List<PerformanceReviewCategoryResponse> Categories { get; set; } = new();
}
