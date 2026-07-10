

public class CreatePerformanceTemplateDto
{
    public string TemplateName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public Guid DepartmentId { get; set; }
}


public class UpdatePerformanceTemplateDto
{
    public Guid Id { get; set; }

    public string TemplateName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public Guid DepartmentId { get; set; }
}
