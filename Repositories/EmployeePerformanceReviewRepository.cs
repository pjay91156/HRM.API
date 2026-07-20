using HRM.API.Data;
using HRM.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HRM.API.Repositories;

public interface IEmployeePerformanceReviewRepository
{
    Task<List<EmployeePerformanceReview>> GetOrCreateReviewsForEmployeeAsync(Guid employeeId);
    Task<EmployeePerformanceReview?> GetByIdAsync(Guid id);
    Task<List<PerformanceCategory>> GetTemplateCategoriesAsync(Guid departmentId);
    Task<List<EmployeePerformanceSkillReview>> GetSkillReviewsAsync(Guid reviewId);
    Task<List<EmployeePerformanceReview>> GetTeamReviewsAsync(Guid managerId);
    Task SaveEmployeeReviewAsync(EmployeePerformanceReview review, List<SkillRatingDto> ratings, string? overallComment, bool isDraft);
    Task SaveManagerReviewAsync(EmployeePerformanceReview review, List<SkillRatingDto> ratings, string? overallComment, bool isDraft);
}

public class EmployeePerformanceReviewRepository : IEmployeePerformanceReviewRepository
{
    private readonly ApplicationDbContext _context;

    public EmployeePerformanceReviewRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<EmployeePerformanceReview>> GetOrCreateReviewsForEmployeeAsync(Guid employeeId)
    {
        var employee = await _context.Employees
            .FirstOrDefaultAsync(x => x.Id == employeeId && x.IsActive);

        if (employee == null || !employee.ManagerId.HasValue)
        {
            return new List<EmployeePerformanceReview>();
        }

        var hasTemplate = await _context.PerformanceTemplates
            .AnyAsync(x => x.IsActive && x.DepartmentId == employee.DepartmentId);

        if (!hasTemplate)
        {
            return new List<EmployeePerformanceReview>();
        }

        var cycles = await _context.PerformanceCycles
            .Where(x => x.IsActive)
            .ToListAsync();

        var existingCycleIds = await _context.EmployeePerformanceReviews
            .Where(x => x.EmployeeId == employeeId && x.IsActive)
            .Select(x => x.PerformanceCycleId)
            .ToListAsync();

        var missingCycles = cycles
            .Where(x => !existingCycleIds.Contains(x.Id))
            .ToList();

        if (missingCycles.Any())
        {
            foreach (var cycle in missingCycles)
            {
                await _context.EmployeePerformanceReviews.AddAsync(new EmployeePerformanceReview
                {
                    Id = Guid.NewGuid(),
                    PerformanceCycleId = cycle.Id,
                    EmployeeId = employeeId,
                    ManagerId = employee.ManagerId.Value,
                    Status = 0,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = employee.UserId
                });
            }

            await _context.SaveChangesAsync();
        }

        return await _context.EmployeePerformanceReviews
            .AsNoTracking()
            .Include(x => x.PerformanceCycle)
            .Include(x => x.Employee).ThenInclude(e => e.User)
            .Where(x => x.EmployeeId == employeeId && x.IsActive)
            .OrderByDescending(x => x.PerformanceCycle.ReviewPeriodStart)
            .ToListAsync();
    }

    public async Task<EmployeePerformanceReview?> GetByIdAsync(Guid id)
    {
        return await _context.EmployeePerformanceReviews
            .Include(x => x.PerformanceCycle)
            .Include(x => x.Employee).ThenInclude(e => e.User)
            .Include(x => x.Employee).ThenInclude(e => e.Department)
            .Include(x => x.Manager).ThenInclude(m => m.User)
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive);
    }

    public async Task<List<PerformanceCategory>> GetTemplateCategoriesAsync(Guid departmentId)
    {
        return await _context.PerformanceCategories
            .AsNoTracking()
            .Include(x => x.PerformanceSkills)
            .Where(x => x.IsActive &&
                        x.PerformanceTemplate.IsActive &&
                        x.PerformanceTemplate.DepartmentId == departmentId)
            .OrderBy(x => x.DisplayOrder)
            .ToListAsync();
    }

    public async Task<List<EmployeePerformanceSkillReview>> GetSkillReviewsAsync(Guid reviewId)
    {
        return await _context.EmployeePerformanceSkillReviews
            .AsNoTracking()
            .Where(x => x.EmployeePerformanceReviewId == reviewId && x.IsActive)
            .ToListAsync();
    }

    public async Task<List<EmployeePerformanceReview>> GetTeamReviewsAsync(Guid managerId)
    {
        return await _context.EmployeePerformanceReviews
            .AsNoTracking()
            .Include(x => x.PerformanceCycle)
            .Include(x => x.Employee).ThenInclude(e => e.User)
            .Where(x => x.ManagerId == managerId && x.IsActive)
            .OrderByDescending(x => x.PerformanceCycle.ReviewPeriodStart)
            .ToListAsync();
    }

    private async Task UpsertSkillRatingsAsync(
        Guid reviewId,
        List<SkillRatingDto> ratings,
        bool isEmployeeSide,
        Guid actingUserId)
    {
        var existing = await _context.EmployeePerformanceSkillReviews
            .Where(x => x.EmployeePerformanceReviewId == reviewId)
            .ToListAsync();

        foreach (var rating in ratings)
        {
            var row = existing.FirstOrDefault(x => x.PerformanceSkillId == rating.SkillId);

            if (row == null)
            {
                row = new EmployeePerformanceSkillReview
                {
                    Id = Guid.NewGuid(),
                    EmployeePerformanceReviewId = reviewId,
                    PerformanceSkillId = rating.SkillId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = actingUserId
                };

                await _context.EmployeePerformanceSkillReviews.AddAsync(row);
                existing.Add(row);
            }

            if (isEmployeeSide)
            {
                row.EmployeeRating = rating.Rating;
                row.EmployeeComment = rating.Comment;
            }
            else
            {
                row.ManagerRating = rating.Rating;
                row.ManagerComment = rating.Comment;
            }
        }
    }

    public async Task SaveEmployeeReviewAsync(EmployeePerformanceReview review, List<SkillRatingDto> ratings, string? overallComment, bool isDraft)
    {
        await UpsertSkillRatingsAsync(review.Id, ratings, isEmployeeSide: true, actingUserId: review.Employee.UserId);

        review.OverallEmployeeComment = overallComment;

        if (!isDraft)
        {
            review.SubmittedOn = DateTime.UtcNow;

            if (review.Status < 1)
            {
                review.Status = 1;
            }
        }

        _context.EmployeePerformanceReviews.Update(review);

        await _context.SaveChangesAsync();
    }

    public async Task SaveManagerReviewAsync(EmployeePerformanceReview review, List<SkillRatingDto> ratings, string? overallComment, bool isDraft)
    {
        await UpsertSkillRatingsAsync(review.Id, ratings, isEmployeeSide: false, actingUserId: review.Manager.UserId);

        review.OverallManagerComment = overallComment;

        if (!isDraft)
        {
            review.ManagerReviewedOn = DateTime.UtcNow;
            review.Status = 2;
        }

        _context.EmployeePerformanceReviews.Update(review);

        await _context.SaveChangesAsync();
    }
}
