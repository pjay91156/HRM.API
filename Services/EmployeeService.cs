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
    Task<ApiResponse<bool>> UpdateEmployeeAsync(UpdateEmployeeDto request, Guid userId);
    Task<ApiResponse<bool>> DeleteEmployeeAsync(Guid employeeId, Guid userId);
    Task<ApiResponse<EmployeeHierarchyResponse>> GetMyTeamHierarchyAsync(Guid userId, Guid companyId);
    Task<ApiResponse<string>> UpdateUserRoleAsync(Guid actingUserId, Guid targetEmployeeId, UserRole newRole);
}
public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IAuthRepository _authRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IDesignationRepository _designationRepository;
    private readonly IAuditLogService _auditLogService;

    public EmployeeService(
        IEmployeeRepository employeeRepository,
        IAuthRepository authRepository,
        IDepartmentRepository departmentRepository,
        IDesignationRepository designationRepository,
        IAuditLogService auditLogService)
    {
        _employeeRepository = employeeRepository;
        _authRepository = authRepository;
        _departmentRepository = departmentRepository;
        _designationRepository = designationRepository;
        _auditLogService = auditLogService;
    }

    private async Task<string> ResolveDepartmentNameAsync(Guid departmentId)
    {
        var department = await _departmentRepository.GetByIdAsync(departmentId);
        return department?.DepartmentName ?? "Unknown";
    }

    private async Task<string> ResolveDesignationNameAsync(Guid designationId)
    {
        var designation = await _designationRepository.GetByIdAsync(designationId);
        return designation?.DesignationName ?? "Unknown";
    }

    private async Task<string> ResolveManagerNameAsync(Guid? managerId)
    {
        if (!managerId.HasValue)
        {
            return "None";
        }

        var manager = await _employeeRepository.GetByIdAsync(managerId.Value);
        return manager == null ? "Unknown" : $"{manager.User?.FirstName} {manager.User?.LastName}".Trim();
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
        var email = request.Email.Trim();
        var empCode = request.EmpCode.Trim();

        if (await _employeeRepository.ActiveUserEmailExistsAsync(email))
        {
            return new ApiResponse<string>
            {
                Success = false,
                Message = "An active user with this email already exists."
            };
        }

        if (await _employeeRepository.EmployeeCodeExistsAsync(empCode, companyId))
        {
            return new ApiResponse<string>
            {
                Success = false,
                Message = "An employee with this employee code already exists."
            };
        }

        var Id = Guid.NewGuid();
        var user = new User
        {
            Id = Id,
            CompanyId = companyId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = email,
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
            EmployeeCode = empCode,
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

        var addedName = $"{user.FirstName} {user.LastName}".Trim();
        var actingEmployee = await _employeeRepository.GetByUserIdAsync(userId);
        var actingName = actingEmployee == null
            ? "Admin"
            : $"{actingEmployee.User?.FirstName} {actingEmployee.User?.LastName}".Trim();

        await _auditLogService.LogAsync(new AuditLogEntryDto
        {
            CompanyId = companyId,
            UserId = userId,
            UserName = actingName,
            Action = AuditAction.Created,
            Module = AuditModule.Employee,
            EntityType = nameof(Employee),
            EntityId = employee.Id,
            TargetUserId = employee.Id,
            TargetUserName = addedName,
            Description = $"{actingName} added a new employee, {addedName}.",
            Status = "Active"
        });

        return new ApiResponse<string>
        {
            Success = true,
            Message = "Employee added successfully.",
            Data = employee.Id.ToString()
        };
    }
    public async Task<ApiResponse<bool>> UpdateEmployeeAsync(UpdateEmployeeDto request, Guid userId)
    {
        var employee = await _employeeRepository.GetByIdAsync(request.Id);

        if (employee == null)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Employee not found.",
                Data = false
            };
        }

        var email = request.Email.Trim();
        // Older clients don't send the code; keep the existing one in that case
        var empCode = string.IsNullOrWhiteSpace(request.EmpCode) ? employee.EmployeeCode : request.EmpCode.Trim();

        if (await _employeeRepository.ActiveUserEmailExistsAsync(email, employee.UserId))
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "An active user with this email already exists.",
                Data = false
            };
        }

        if (await _employeeRepository.EmployeeCodeExistsAsync(empCode, employee.User.CompanyId, employee.Id))
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "An employee with this employee code already exists.",
                Data = false
            };
        }

        var oldName = $"{employee.User.FirstName} {employee.User.LastName}".Trim();
        var oldEmail = employee.User.Email;
        var oldEmpCode = employee.EmployeeCode;
        var oldPhone = employee.PhoneNumber;
        var oldDepartmentId = employee.DepartmentId;
        var oldDesignationId = employee.DesignationId;
        var oldManagerId = employee.ManagerId;

        employee.User.FirstName = request.FirstName;
        employee.User.LastName = request.LastName;
        employee.User.Email = email;
        employee.User.UpdatedAt = DateTime.UtcNow;
        employee.User.UpdatedBy = userId;

        employee.EmployeeCode = empCode;
        employee.PhoneNumber = request.Phone;
        employee.DepartmentId = request.DepartmentId;
        employee.DesignationId = request.DesignationId;
        employee.ManagerId = request.ManagerId;
        employee.UpdatedAt = DateTime.UtcNow;
        employee.UpdatedBy = userId;

        await _employeeRepository.SaveChangesAsync();

        var newName = $"{request.FirstName} {request.LastName}".Trim();
        var actingEmployee = await _employeeRepository.GetByUserIdAsync(userId);
        var actingName = actingEmployee == null
            ? "Admin"
            : $"{actingEmployee.User?.FirstName} {actingEmployee.User?.LastName}".Trim();

        var oldValues = new Dictionary<string, string>();
        var newValues = new Dictionary<string, string>();
        var changeSummaries = new List<string>();

        if (oldDepartmentId != request.DepartmentId)
        {
            var oldDept = await ResolveDepartmentNameAsync(oldDepartmentId);
            var newDept = await ResolveDepartmentNameAsync(request.DepartmentId);
            oldValues["department"] = oldDept;
            newValues["department"] = newDept;
            changeSummaries.Add($"department from {oldDept} to {newDept}");
        }

        if (oldDesignationId != request.DesignationId)
        {
            var oldDesig = await ResolveDesignationNameAsync(oldDesignationId);
            var newDesig = await ResolveDesignationNameAsync(request.DesignationId);
            oldValues["designation"] = oldDesig;
            newValues["designation"] = newDesig;
            changeSummaries.Add($"designation from {oldDesig} to {newDesig}");
        }

        if (oldManagerId != request.ManagerId)
        {
            var oldManager = await ResolveManagerNameAsync(oldManagerId);
            var newManager = await ResolveManagerNameAsync(request.ManagerId);
            oldValues["manager"] = oldManager;
            newValues["manager"] = newManager;
            changeSummaries.Add($"manager from {oldManager} to {newManager}");
        }

        if (oldEmail != email)
        {
            oldValues["email"] = oldEmail;
            newValues["email"] = email;
        }

        if (oldEmpCode != empCode)
        {
            oldValues["employeeCode"] = oldEmpCode;
            newValues["employeeCode"] = empCode;
        }

        if (oldPhone != request.Phone)
        {
            oldValues["phone"] = oldPhone ?? string.Empty;
            newValues["phone"] = request.Phone ?? string.Empty;
        }

        var description = changeSummaries.Count > 0
            ? $"{actingName} updated {newName}'s {string.Join(", ", changeSummaries)}."
            : $"{actingName} updated {newName}'s information.";

        await _auditLogService.LogAsync(new AuditLogEntryDto
        {
            CompanyId = employee.User.CompanyId,
            UserId = userId,
            UserName = actingName,
            Action = AuditAction.Updated,
            Module = AuditModule.Employee,
            EntityType = nameof(Employee),
            EntityId = employee.Id,
            TargetUserId = employee.Id,
            TargetUserName = newName,
            Description = description,
            Status = "Active",
            OldValues = oldValues.Count > 0 ? oldValues : null,
            NewValues = newValues.Count > 0 ? newValues : null
        });

        return new ApiResponse<bool>
        {
            Success = true,
            Message = "Employee updated successfully.",
            Data = true
        };
    }

    public async Task<ApiResponse<bool>> DeleteEmployeeAsync(Guid employeeId, Guid userId)
    {
        var employee = await _employeeRepository.GetByIdAsync(employeeId);

        if (employee == null)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Employee not found.",
                Data = false
            };
        }

        var deletedName = $"{employee.User?.FirstName} {employee.User?.LastName}".Trim();
        var companyId = employee.User!.CompanyId;

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

        var actingEmployee = await _employeeRepository.GetByUserIdAsync(userId);
        var actingName = actingEmployee == null
            ? "Admin"
            : $"{actingEmployee.User?.FirstName} {actingEmployee.User?.LastName}".Trim();

        await _auditLogService.LogAsync(new AuditLogEntryDto
        {
            CompanyId = companyId,
            UserId = userId,
            UserName = actingName,
            Action = AuditAction.Deleted,
            Module = AuditModule.Employee,
            EntityType = nameof(Employee),
            EntityId = employeeId,
            TargetUserId = employeeId,
            TargetUserName = deletedName,
            Description = $"{actingName} deleted employee {deletedName}.",
            Status = "Deleted"
        });

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

        var actingName = $"{actingEmployee.User?.FirstName} {actingEmployee.User?.LastName}".Trim();
        var targetName = $"{targetEmployee.User?.FirstName} {targetEmployee.User?.LastName}".Trim();

        await _auditLogService.LogAsync(new AuditLogEntryDto
        {
            CompanyId = actingEmployee.User!.CompanyId,
            UserId = actingUserId,
            UserName = actingName,
            Action = AuditAction.Updated,
            Module = AuditModule.RolePermission,
            EntityType = nameof(User),
            EntityId = targetEmployee.UserId,
            TargetUserId = targetEmployee.Id,
            TargetUserName = targetName,
            Description = $"{actingName} assigned the {newRole} role to {targetName}.",
            Status = newRole.ToString(),
            OldValues = new Dictionary<string, string> { ["role"] = targetCurrentRole.ToString() },
            NewValues = new Dictionary<string, string> { ["role"] = newRole.ToString() }
        });

        return new ApiResponse<string>
        {
            Success = true,
            Message = $"Role updated to {newRole} successfully."
        };
    }
}