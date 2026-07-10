namespace HRM.API.Responses;

public class PerformanceTemplateResponse
{
    public Guid Id { get; set; }

    public string TemplateName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public Guid DepartmentId { get; set; }

    public string DepartmentName { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}
