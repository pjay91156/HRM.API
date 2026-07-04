public class EmployeeDto
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
    public string EmpCode { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public Guid DepartmentId { get; set; }
    public Guid ManagerId { get; set; }

    public Guid DesignationId { get; set; }
}