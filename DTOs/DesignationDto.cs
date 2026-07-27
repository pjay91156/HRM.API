public class DesignationDto
{
    public string Name { get; set; } = string.Empty;
    public Guid DepartmentId { get; set; }
}

public class UpdateDesignationDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid DepartmentId { get; set; }
}