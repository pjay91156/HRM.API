using HRM.API.Data;
using HRM.API.Models;
using HRM.API.Responses;
using Microsoft.EntityFrameworkCore;

public interface IDepartmentRepository
{
    Task<List<DepartmentResponse>> GetDepartmentsByCompanyIdAsync(Guid companyId);
    Task AddDepartmnetAsync(Department department);
    Task<bool> DeleteDepartmentAsync(Guid deparmentId);
    Task<Department?> GetByIdAsync(Guid id);
    Task<bool> ExistsByNameAsync(string departmentName, Guid companyId, Guid? excludeId = null);
    Task SaveChangesAsync();
}
public class DepartmentRepository : IDepartmentRepository
{
    private readonly ApplicationDbContext _context;

    public DepartmentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<DepartmentResponse>> GetDepartmentsByCompanyIdAsync(Guid companyId)
    {
        return await _context.Departments
            .Where(d => d.CompanyId == companyId && d.IsActive)
            .OrderBy(d => d.DepartmentName)
            .Select(d => new DepartmentResponse
            {
                Id = d.Id,
                DepartmentName = d.DepartmentName
            })
            .ToListAsync();
    }
    public async Task<bool> DeleteDepartmentAsync(Guid deparmentId)
    {
        var department = await _context.Departments
            .FirstOrDefaultAsync(x =>
                x.Id == deparmentId
                );

        if (department == null)
        {
            return false;
        }
        department.IsActive = false;
        await _context.SaveChangesAsync();

        return true;
    }
    public async Task AddDepartmnetAsync(Department department)
    {
        await _context.Departments.AddAsync(department);
        await _context.SaveChangesAsync();
    }

    public async Task<Department?> GetByIdAsync(Guid id)
    {
        return await _context.Departments
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<bool> ExistsByNameAsync(string departmentName, Guid companyId, Guid? excludeId = null)
    {
        var query = _context.Departments
            .Where(x => x.IsActive
                && x.CompanyId == companyId
                && x.DepartmentName.ToLower() == departmentName.ToLower());

        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}