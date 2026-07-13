namespace HRM.API.Responses;

public class PerformanceSkillResponse
{
    public Guid Id { get; set; }

    public Guid PerformanceCategoryId { get; set; }

    public string SkillName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal Weightage { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; }
}
