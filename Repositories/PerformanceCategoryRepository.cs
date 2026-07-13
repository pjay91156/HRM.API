
using HRM.API.Data;
using HRM.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HRM.API.Repositories;

public interface IPerformanceCategoryRepository
{
    Task<bool> TemplateBelongsToCompanyAsync(Guid templateId, Guid companyId);

    Task<bool> ExistsByNameAsync(Guid templateId, string categoryName, Guid? excludeId = null);

    Task<bool> ExistsByDisplayOrderAsync(Guid templateId, int displayOrder, Guid? excludeId = null);

    Task<decimal> GetTotalWeightageAsync(Guid templateId, Guid? excludeId = null);

    Task<PerformanceCategory?> GetByIdAsync(Guid id);

    Task<List<PerformanceCategory>> GetByTemplateIdAsync(Guid templateId);

    Task AddAsync(PerformanceCategory performanceCategory);

    void Update(PerformanceCategory performanceCategory);

    Task SaveChangesAsync();
}

public class PerformanceCategoryRepository : IPerformanceCategoryRepository
{
    private readonly ApplicationDbContext _context;

    public PerformanceCategoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> TemplateBelongsToCompanyAsync(Guid templateId, Guid companyId)
    {
        return await _context.PerformanceTemplates
            .AnyAsync(x => x.Id == templateId && x.IsActive && x.Department.CompanyId == companyId);
    }

    public async Task<bool> ExistsByNameAsync(Guid templateId, string categoryName, Guid? excludeId = null)
    {
        var query = _context.PerformanceCategories
            .Where(x => x.IsActive &&
                        x.PerformanceTemplateId == templateId &&
                        x.CategoryName.ToLower() == categoryName.ToLower());

        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<bool> ExistsByDisplayOrderAsync(Guid templateId, int displayOrder, Guid? excludeId = null)
    {
        var query = _context.PerformanceCategories
            .Where(x => x.IsActive &&
                        x.PerformanceTemplateId == templateId &&
                        x.DisplayOrder == displayOrder);

        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<decimal> GetTotalWeightageAsync(Guid templateId, Guid? excludeId = null)
    {
        var query = _context.PerformanceCategories
            .Where(x => x.IsActive && x.PerformanceTemplateId == templateId);

        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        return await query.SumAsync(x => (decimal?)x.Weightage) ?? 0m;
    }

    public async Task<PerformanceCategory?> GetByIdAsync(Guid id)
    {
        return await _context.PerformanceCategories
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive);
    }

    public async Task<List<PerformanceCategory>> GetByTemplateIdAsync(Guid templateId)
    {
        return await _context.PerformanceCategories
            .Where(x => x.IsActive && x.PerformanceTemplateId == templateId)
            .OrderBy(x => x.DisplayOrder)
            .ToListAsync();
    }

    public async Task AddAsync(PerformanceCategory performanceCategory)
    {
        await _context.PerformanceCategories.AddAsync(performanceCategory);
    }

    public void Update(PerformanceCategory performanceCategory)
    {
        _context.PerformanceCategories.Update(performanceCategory);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
