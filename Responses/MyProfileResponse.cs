namespace HRM.API.Responses;

public class MyProfileResponse
{
    public Guid UserId { get; set; }

    public Guid EmployeeId { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string? ProfilePictureUrl { get; set; }

    public string Role { get; set; } = string.Empty;

    public string EmployeeCode { get; set; } = string.Empty;

    public string DepartmentName { get; set; } = string.Empty;

    public string DesignationName { get; set; } = string.Empty;

    public DateOnly JoiningDate { get; set; }

    public string? ManagerName { get; set; }

    public string CompanyName { get; set; } = string.Empty;
}
