namespace HRM.API.Responses;
public class EmployeeResponse
{
    public Guid Id { get; set; }

    public string EmployeeCode { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string DepartmentName { get; set; } = string.Empty;

    public string DesignationName { get; set; } = string.Empty;

    public DateOnly JoiningDate { get; set; }

    public bool IsActive { get; set; }
}
public class EmployeeHierarchyResponse
{
    public Guid Id { get; set; }

    public string EmployeeCode { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Designation { get; set; } = string.Empty;

    public string Department { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public bool IsSelf { get; set; }

    public List<EmployeeHierarchyResponse> Children { get; set; } = [];
}