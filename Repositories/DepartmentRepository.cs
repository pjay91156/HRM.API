using HRM.API.Data;
using HRM.API.Models;
using HRM.API.Responses;
using Microsoft.EntityFrameworkCore;

public interface IDepartmentRepository
{
    Task<List<DepartmentResponse>> GetDepartmentsByCompanyIdAsync(Guid companyId);
    Task AddDepartmnetAsync(Department department);
    Task<bool> DeleteDepartmentAsync(Guid deparmentId);
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
}