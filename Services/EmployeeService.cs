using HRM.API.Responses;
using HRM.API.Models;
using HRM.API.Repositories;
using HRM.API.Enums;

namespace HRM.API.Services;

public interface IEmployeeService
{
    Task<ApiResponse<List<EmployeeResponse>>> GetEmployeesAsync(Guid companyId);
    Task<ApiResponse<List<EmployeeResponse>>> GetEmployeesByDepartmentAsync(Guid companyId, Guid departmentId);
    Task<ApiResponse<string>> AddEmployeeAsync(EmployeeDto request, Guid companyId, Guid userId);
    Task<ApiResponse<bool>> DeleteEmployeeAsync(Guid employeeId);
    Task<ApiResponse<EmployeeHierarchyResponse>> GetMyTeamHierarchyAsync(Guid userId, Guid companyId);
    Task<ApiResponse<string>> UpdateUserRoleAsync(Guid actingUserId, Guid targetEmployeeId, UserRole newRole);
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
    public async Task<ApiResponse<List<EmployeeResponse>>> GetEmployeesByDepartmentAsync(Guid companyId, Guid departmentId)
    {
        var employees = await _employeeRepository.GetEmployeesByDepartmentAsync(companyId, departmentId);

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
            ProfilePictureUrl = employee.User.ProfilePictureUrl,

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

    public async Task<ApiResponse<string>> UpdateUserRoleAsync(Guid actingUserId, Guid targetEmployeeId, UserRole newRole)
    {
        var actingEmployee = await _employeeRepository.GetByUserIdAsync(actingUserId);
        var targetEmployee = await _employeeRepository.GetByIdAsync(targetEmployeeId);

        if (actingEmployee == null || targetEmployee == null)
        {
            return new ApiResponse<string> { Success = false, Message = "Employee not found." };
        }

        if (actingEmployee.User.CompanyId != targetEmployee.User.CompanyId)
        {
            return new ApiResponse<string> { Success = false, Message = "Employee not found." };
        }

        var actingRole = actingEmployee.User.Role;

        if (actingRole != UserRole.Admin && actingRole != UserRole.SuperAdmin)
        {
            return new ApiResponse<string> { Success = false, Message = "You do not have permission to change roles." };
        }

        var targetCurrentRole = targetEmployee.User.Role;
        var isSelf = actingEmployee.Id == targetEmployee.Id;

        if (targetCurrentRole == UserRole.SuperAdmin || newRole == UserRole.SuperAdmin)
        {
            return new ApiResponse<string> { Success = false, Message = "The Super Admin role cannot be changed." };
        }

        if (newRole == UserRole.Admin)
        {
            if (targetCurrentRole != UserRole.Employee)
            {
                return new ApiResponse<string> { Success = false, Message = "This user is already an Admin." };
            }
        }
        else if (newRole == UserRole.Employee)
        {
            if (targetCurrentRole != UserRole.Admin)
            {
                return new ApiResponse<string> { Success = false, Message = "This user is not an Admin." };
            }

            if (!isSelf && actingRole != UserRole.SuperAdmin)
            {
                return new ApiResponse<string> { Success = false, Message = "Only a Super Admin can demote another Admin. You can demote yourself." };
            }
        }

        targetEmployee.User.Role = newRole;
        targetEmployee.User.UpdatedAt = DateTime.UtcNow;
        targetEmployee.User.UpdatedBy = actingUserId;

        await _employeeRepository.SaveChangesAsync();

        return new ApiResponse<string>
        {
            Success = true,
            Message = $"Role updated to {newRole} successfully."
        };
    }
}