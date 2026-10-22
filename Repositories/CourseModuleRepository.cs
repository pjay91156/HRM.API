using HRM.API.Data;
using HRM.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HRM.API.Repositories;

public interface ICourseModuleRepository
{
    Task<List<CourseModule>> GetByCourseAsync(Guid courseId, Guid companyId);

    Task<CourseModule?> GetByIdAsync(Guid id, Guid courseId, Guid companyId);

    Task<int> GetMaxSortOrderAsync(Guid courseId);

    Task AddAsync(CourseModule module);

    void Update(CourseModule module);

    Task SaveChangesAsync();
}

public class CourseModuleRepository : ICourseModuleRepository
{
    private readonly ApplicationDbContext _context;

    public CourseModuleRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // Created date and id break ties so the order is always deterministic.
    public async Task<List<CourseModule>> GetByCourseAsync(Guid courseId, Guid companyId)
    {
        return await _context.CourseModules
            .Where(x => x.CourseId == courseId && x.Course!.CompanyId == companyId)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.CreatedAt)
            .ThenBy(x => x.Id)
            .ToListAsync();
    }

    public async Task<CourseModule?> GetByIdAsync(Guid id, Guid courseId, Guid companyId)
    {
        return await _context.CourseModules
            .FirstOrDefaultAsync(x => x.Id == id && x.CourseId == courseId && x.Course!.CompanyId == companyId);
    }

    public async Task<int> GetMaxSortOrderAsync(Guid courseId)
    {
        return await _context.CourseModules
            .Where(x => x.CourseId == courseId)
            .MaxAsync(x => (int?)x.SortOrder) ?? 0;
    }

    public async Task AddAsync(CourseModule module)
    {
        await _context.CourseModules.AddAsync(module);
    }

    public void Update(CourseModule module)
    {
        _context.CourseModules.Update(module);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
