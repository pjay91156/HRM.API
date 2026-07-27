public class DepartmentDto
{
     public string DepartmentName { get; set; } = string.Empty;
}

public class UpdateDepartmentDto
{
    public Guid Id { get; set; }

    public string DepartmentName { get; set; } = string.Empty;
}