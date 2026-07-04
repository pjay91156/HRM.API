using HRM.API.Data;
using HRM.API.Responses;
using Microsoft.EntityFrameworkCore;
using HRM.API.Models;


namespace HRM.API.Repositories;

public interface IEmployeeRepository
{
    Task<List<EmployeeResponse>> GetEmployeesAsync(Guid companyId);
    Task AddEmployeeAsync(Employee employee);
    Task<bool> DeleteEmployeeAsync(Guid employeeId);
    Task<Employee?> GetEmployeeHierarchyRootAsync(Guid userId);
    Task<List<Employee>> GetEmployeesForHierarchyAsync(Guid companyId);
    Task<Employee?> GetByUserIdAsync(Guid userId);

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
            .FirstOrDefaultAsync(e => e.UserId == userId);
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
                JoiningDate = e.JoiningDate,
                IsActive = e.IsActive
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
    public async Task AddEmployeeAsync(Employee employee)
    {
        await _context.Employees.AddAsync(employee);
        await _context.SaveChangesAsync();
    }
}