using HRM.API.Data;
using HRM.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HRM.API.Repositories;

public interface ICourseRepository
{
    Task<List<Course>> GetAllAsync(Guid companyId);

    Task<Course?> GetByIdAsync(Guid id, Guid companyId);

    Task<bool> ExistsByNameAsync(string name, Guid companyId, Guid? excludeId = null);

    Task AddAsync(Course course);

    void Update(Course course);

    Task SaveChangesAsync();
}

public class CourseRepository : ICourseRepository
{
    private readonly ApplicationDbContext _context;

    public CourseRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // Inactive courses are returned too so they can be reviewed and reactivated.
    public async Task<List<Course>> GetAllAsync(Guid companyId)
    {
        return await _context.Courses
            .Where(x => x.CompanyId == companyId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<Course?> GetByIdAsync(Guid id, Guid companyId)
    {
        return await _context.Courses
            .FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == companyId);
    }

    public async Task<bool> ExistsByNameAsync(string name, Guid companyId, Guid? excludeId = null)
    {
        var query = _context.Courses
            .Where(x => x.CompanyId == companyId && x.Name.ToLower() == name.ToLower());

        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task AddAsync(Course course)
    {
        await _context.Courses.AddAsync(course);
    }

    public void Update(Course course)
    {
        _context.Courses.Update(course);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
