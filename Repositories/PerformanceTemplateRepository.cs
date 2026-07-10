
using HRM.API.Data;
using HRM.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HRM.API.Repositories;

public interface IPerformanceTemplateRepository
{
    Task<bool> DepartmentBelongsToCompanyAsync(Guid departmentId, Guid companyId);

    Task<bool> ExistsByDepartmentAsync(Guid departmentId, Guid? excludeId = null);

    Task<PerformanceTemplate?> GetByIdAsync(Guid id);

    Task<List<PerformanceTemplate>> GetAllByCompanyAsync(Guid companyId);

    Task AddAsync(PerformanceTemplate performanceTemplate);

    void Update(PerformanceTemplate performanceTemplate);

    Task SaveChangesAsync();
}

public class PerformanceTemplateRepository : IPerformanceTemplateRepository
{
    private readonly ApplicationDbContext _context;

    public PerformanceTemplateRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> DepartmentBelongsToCompanyAsync(Guid departmentId, Guid companyId)
    {
        return await _context.Departments
            .AnyAsync(x => x.Id == departmentId && x.CompanyId == companyId && x.IsActive);
    }

    public async Task<bool> ExistsByDepartmentAsync(Guid departmentId, Guid? excludeId = null)
    {
        var query = _context.PerformanceTemplates
            .Where(x => x.IsActive && x.DepartmentId == departmentId);

        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<PerformanceTemplate?> GetByIdAsync(Guid id)
    {
        return await _context.PerformanceTemplates
            .Include(x => x.Department)
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive);
    }

    public async Task<List<PerformanceTemplate>> GetAllByCompanyAsync(Guid companyId)
    {
        return await _context.PerformanceTemplates
            .Include(x => x.Department)
            .Where(x => x.IsActive && x.Department.CompanyId == companyId)
            .OrderBy(x => x.Department.DepartmentName)
            .ToListAsync();
    }

    public async Task AddAsync(PerformanceTemplate performanceTemplate)
    {
        await _context.PerformanceTemplates.AddAsync(performanceTemplate);
    }

    public void Update(PerformanceTemplate performanceTemplate)
    {
        _context.PerformanceTemplates.Update(performanceTemplate);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
