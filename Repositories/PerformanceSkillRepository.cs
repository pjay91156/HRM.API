
using HRM.API.Data;
using HRM.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HRM.API.Repositories;

public interface IPerformanceSkillRepository
{
    Task<bool> CategoryBelongsToCompanyAsync(Guid categoryId, Guid companyId);

    Task<bool> ExistsByNameAsync(Guid categoryId, string skillName, Guid? excludeId = null);

    Task<bool> ExistsByDisplayOrderAsync(Guid categoryId, int displayOrder, Guid? excludeId = null);

    Task<decimal> GetTotalWeightageAsync(Guid categoryId, Guid? excludeId = null);

    Task<PerformanceSkill?> GetByIdAsync(Guid id);

    Task<List<PerformanceSkill>> GetByCategoryIdAsync(Guid categoryId);

    Task AddAsync(PerformanceSkill performanceSkill);

    void Update(PerformanceSkill performanceSkill);

    Task SaveChangesAsync();
}

public class PerformanceSkillRepository : IPerformanceSkillRepository
{
    private readonly ApplicationDbContext _context;

    public PerformanceSkillRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> CategoryBelongsToCompanyAsync(Guid categoryId, Guid companyId)
    {
        return await _context.PerformanceCategories
            .AnyAsync(x => x.Id == categoryId &&
                           x.IsActive &&
                           x.PerformanceTemplate.IsActive &&
                           x.PerformanceTemplate.Department.CompanyId == companyId);
    }

    public async Task<bool> ExistsByNameAsync(Guid categoryId, string skillName, Guid? excludeId = null)
    {
        var query = _context.PerformanceSkills
            .Where(x => x.IsActive &&
                        x.PerformanceCategoryId == categoryId &&
                        x.SkillName.ToLower() == skillName.ToLower());

        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<bool> ExistsByDisplayOrderAsync(Guid categoryId, int displayOrder, Guid? excludeId = null)
    {
        var query = _context.PerformanceSkills
            .Where(x => x.IsActive &&
                        x.PerformanceCategoryId == categoryId &&
                        x.DisplayOrder == displayOrder);

        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<decimal> GetTotalWeightageAsync(Guid categoryId, Guid? excludeId = null)
    {
        var query = _context.PerformanceSkills
            .Where(x => x.IsActive && x.PerformanceCategoryId == categoryId);

        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        return await query.SumAsync(x => (decimal?)x.Weightage) ?? 0m;
    }

    public async Task<PerformanceSkill?> GetByIdAsync(Guid id)
    {
        return await _context.PerformanceSkills
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive);
    }

    public async Task<List<PerformanceSkill>> GetByCategoryIdAsync(Guid categoryId)
    {
        return await _context.PerformanceSkills
            .Where(x => x.IsActive && x.PerformanceCategoryId == categoryId)
            .OrderBy(x => x.DisplayOrder)
            .ToListAsync();
    }

    public async Task AddAsync(PerformanceSkill performanceSkill)
    {
        await _context.PerformanceSkills.AddAsync(performanceSkill);
    }

    public void Update(PerformanceSkill performanceSkill)
    {
        _context.PerformanceSkills.Update(performanceSkill);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
