using HRM.API.Responses;
using HRM.API.Models;
using HRM.API.Repositories;

namespace HRM.API.Services;

public interface IEmployeeService
{
    Task<ApiResponse<List<EmployeeResponse>>> GetEmployeesAsync(Guid companyId);
    Task<ApiResponse<string>> AddEmployeeAsync(EmployeeDto request, Guid companyId, Guid userId);
    Task<ApiResponse<bool>> DeleteEmployeeAsync(Guid employeeId);
    Task<ApiResponse<EmployeeHierarchyResponse>> GetMyTeamHierarchyAsync(Guid userId, Guid companyId);
}
public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IAuthRepository _authRepository;

    public EmployeeService(IEmployeeRepository employeeRepository, IAuthRepository authRepository)
    {
        _employeeRepository = employeeRepository;
        _authRepository = authRepository;
    }

    public async Task<ApiResponse<List<EmployeeResponse>>> GetEmployeesAsync(Guid companyId)
    {
        var employees = await _employeeRepository.GetEmployeesAsync(companyId);

        return new ApiResponse<List<EmployeeResponse>>
        {
            Success = true,
            Message = "Employees retrieved successfully.",
            Data = employees
        };
    }
    public async Task<ApiResponse<EmployeeHierarchyResponse>> GetMyTeamHierarchyAsync(
     Guid userId,
     Guid companyId)
    {
       
          var allEmployees = await _employeeRepository.GetEmployeesForHierarchyAsync(companyId);
         var employee = await _employeeRepository.GetEmployeeHierarchyRootAsync(userId);

        if (employee == null)
        {
            return new ApiResponse<EmployeeHierarchyResponse>
            {
                Success = false,
                Message = "Employee not found.",
                Errors = new List<string> { "No employee found for the logged in user." }
            };
        }

      

        var hierarchy = BuildHierarchy(
            employee,
            allEmployees,
            employee.Id);

        return new ApiResponse<EmployeeHierarchyResponse>
        {
            Success = true,
            Message = "Team hierarchy retrieved successfully.",
            Data = hierarchy
        };
    }
    private EmployeeHierarchyResponse BuildHierarchy(
    Employee employee,
    List<Employee> allEmployees,
    Guid loggedInEmployeeId)
    {
        var children = allEmployees
            .Where(x => x.ManagerId == employee.Id)
            .ToList();

        return new EmployeeHierarchyResponse
        {
            Id = employee.Id,
            EmployeeCode = employee.EmployeeCode,
            Name = $"{employee.User.FirstName} {employee.User.LastName}",
            Email = employee.User.Email,
            Department = employee.Department.DepartmentName,
            Designation = employee.Designation.DesignationName,
            IsSelf = employee.Id == loggedInEmployeeId,

            Children = children
                .Select(child => BuildHierarchy(
                    child,
                    allEmployees,
                    loggedInEmployeeId))
                .ToList()
        };
    }
    public async Task<ApiResponse<string>> AddEmployeeAsync(EmployeeDto request, Guid companyId, Guid userId)
    {
        var Id = Guid.NewGuid();
        var user = new User
        {
            Id = Id,
            CompanyId = companyId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("1234"),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };
        await _authRepository.AddUserAsync(user);
        await _authRepository.SaveChangesAsync();
        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            PhoneNumber = request.Phone,
            EmployeeCode = request.EmpCode,
            DepartmentId = request.DepartmentId,
            DesignationId = request.DesignationId,
            ManagerId = request.ManagerId,
            IsActive = true,
            UserId = Id,
            JoiningDate = DateOnly.FromDateTime(DateTime.UtcNow),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        await _employeeRepository.AddEmployeeAsync(employee);

        return new ApiResponse<string>
        {
            Success = true,
            Message = "Employee added successfully.",
            Data = employee.Id.ToString()
        };
    }
    public async Task<ApiResponse<bool>> DeleteEmployeeAsync(Guid employeeId)
    {
        var deleted = await _employeeRepository.DeleteEmployeeAsync(employeeId);

        if (!deleted)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Employee not found.",
                Data = false
            };
        }

        return new ApiResponse<bool>
        {
            Success = true,
            Message = "Employee deleted successfully.",
            Data = true
        };
    }
}