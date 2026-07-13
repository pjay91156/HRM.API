namespace HRM.API.Responses;

public class PerformanceCategoryResponse
{
    public Guid Id { get; set; }

    public Guid PerformanceTemplateId { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public decimal Weightage { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; }
}
