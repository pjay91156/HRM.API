using HRM.API.Data;
using HRM.API.Responses;
using Microsoft.EntityFrameworkCore;
using HRM.API.Models;


namespace HRM.API.Repositories;

public interface IEmployeeRepository
{
    Task<List<EmployeeResponse>> GetEmployeesAsync(Guid companyId);
    Task<List<EmployeeResponse>> GetEmployeesByDepartmentAsync(Guid companyId, Guid departmentId);
    Task AddEmployeeAsync(Employee employee);
    Task<bool> DeleteEmployeeAsync(Guid employeeId);
    Task<Employee?> GetEmployeeHierarchyRootAsync(Guid userId);
    Task<List<Employee>> GetEmployeesForHierarchyAsync(Guid companyId);
    Task<Employee?> GetByUserIdAsync(Guid userId);
    Task<Employee?> GetByIdAsync(Guid employeeId);
    Task<Employee?> GetProfileByUserIdAsync(Guid userId);
    Task<bool> ActiveUserEmailExistsAsync(string email, Guid? excludeUserId = null);
    Task<bool> EmployeeCodeExistsAsync(string employeeCode, Guid companyId, Guid? excludeEmployeeId = null);
    Task SaveChangesAsync();

}
public class EmployeeRepository : IEmployeeRepository
{
    private readonly ApplicationDbContext _context;

    public EmployeeRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<Employee?> GetByUserIdAsync(Guid userId)
    {
        return await _context.Employees
            .Include(e => e.User)
            .FirstOrDefaultAsync(e => e.UserId == userId);
    }

    public async Task<Employee?> GetByIdAsync(Guid employeeId)
    {
        return await _context.Employees
            .Include(e => e.User)
            .FirstOrDefaultAsync(e => e.Id == employeeId);
    }

    public async Task<Employee?> GetProfileByUserIdAsync(Guid userId)
    {
        return await _context.Employees
            .Include(e => e.User).ThenInclude(u => u.Company)
            .Include(e => e.Department)
            .Include(e => e.Designation)
            .Include(e => e.Manager).ThenInclude(m => m!.User)
            .FirstOrDefaultAsync(e => e.UserId == userId);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
    public async Task<List<EmployeeResponse>> GetEmployeesAsync(Guid companyId)
    {
        return await _context.Employees
            .AsNoTracking()
            .Where(e =>
                e.IsActive &&
                e.User.CompanyId == companyId)
            .OrderBy(e => e.User.FirstName)
            .Select(e => new EmployeeResponse
            {
                Id = e.Id,
                EmployeeCode = e.EmployeeCode,
                FirstName = e.User.FirstName,
                LastName = e.User.LastName,
                Email = e.User.Email,
                DepartmentName = e.Department.DepartmentName,
                DesignationName = e.Designation.DesignationName,
                DepartmentId = e.DepartmentId,
                DesignationId = e.DesignationId,
                ManagerId = e.ManagerId,
                Phone = e.PhoneNumber,
                JoiningDate = e.JoiningDate,
                IsActive = e.IsActive,
                Role = e.User.Role.ToString()
            })
            .ToListAsync();
    }
    public async Task<List<EmployeeResponse>> GetEmployeesByDepartmentAsync(Guid companyId, Guid departmentId)
    {
        return await _context.Employees
            .AsNoTracking()
            .Where(e =>
                e.IsActive &&
                e.User.CompanyId == companyId &&
                e.DepartmentId == departmentId)
            .OrderBy(e => e.User.FirstName)
            .Select(e => new EmployeeResponse
            {
                Id = e.Id,
                EmployeeCode = e.EmployeeCode,
                FirstName = e.User.FirstName,
                LastName = e.User.LastName,
                Email = e.User.Email,
                DepartmentName = e.Department.DepartmentName,
                DesignationName = e.Designation.DesignationName,
                DepartmentId = e.DepartmentId,
                DesignationId = e.DesignationId,
                ManagerId = e.ManagerId,
                Phone = e.PhoneNumber,
                JoiningDate = e.JoiningDate,
                IsActive = e.IsActive,
                Role = e.User.Role.ToString()
            })
            .ToListAsync();
    }
    public async Task<List<Employee>> GetEmployeesForHierarchyAsync(Guid companyId)
{
    return await _context.Employees
        .AsNoTracking()
        .Include(x => x.User)
        .Include(x => x.Department)
        .Include(x => x.Designation)
        .Where(x =>
            x.IsActive &&
            x.User.CompanyId == companyId)
        .ToListAsync();
}
    public async Task<Employee?> GetEmployeeHierarchyRootAsync(Guid userId)
    {
        return await _context.Employees
            .Include(x => x.User)
            .Include(x => x.Department)
            .Include(x => x.Designation)
            .FirstOrDefaultAsync(x => x.UserId == userId && x.IsActive);
    }

    public async Task<bool> DeleteEmployeeAsync(Guid employeeId)
    {
        var employee = await _context.Employees
            .FirstOrDefaultAsync(x =>
                x.Id == employeeId
                );

        if (employee == null)
        {
            return false;
        }

        _context.Employees.Remove(employee);

        await _context.SaveChangesAsync();

        return true;
    }
    // Email is the login identifier, so it must be unique across all active users (not just this company)
    public async Task<bool> ActiveUserEmailExistsAsync(string email, Guid? excludeUserId = null)
    {
        var query = _context.Users
            .Where(x => x.IsActive && x.Email.ToLower() == email.ToLower());

        if (excludeUserId.HasValue)
        {
            query = query.Where(x => x.Id != excludeUserId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<bool> EmployeeCodeExistsAsync(string employeeCode, Guid companyId, Guid? excludeEmployeeId = null)
    {
        var query = _context.Employees
            .Where(x => x.IsActive
                && x.User.IsActive
                && x.User.CompanyId == companyId
                && x.EmployeeCode.ToLower() == employeeCode.ToLower());

        if (excludeEmployeeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeEmployeeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task AddEmployeeAsync(Employee employee)
    {
        await _context.Employees.AddAsync(employee);
        await _context.SaveChangesAsync();
    }
}