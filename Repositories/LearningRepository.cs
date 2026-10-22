using HRM.API.Data;
using HRM.API.Enums;
using HRM.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HRM.API.Repositories;

// Data access for the employee learning experience. Only published, active
// courses and active modules of the given company are ever returned.
public interface ILearningRepository
{
    Task<List<Course>> GetAvailableCoursesAsync(Guid companyId);

    Task<Course?> GetAvailableCourseAsync(Guid courseId, Guid companyId);

    Task<List<CourseModule>> GetActiveModulesAsync(IEnumerable<Guid> courseIds);

    Task<CourseModule?> GetActiveModuleAsync(Guid moduleId, Guid courseId);

    Task<List<Guid>> GetModuleIdsWithActiveQuizAsync(IEnumerable<Guid> moduleIds);

    Task<ModuleQuiz?> GetActiveQuizAsync(Guid moduleId);

    Task<ModuleQuiz?> GetActiveFinalQuizAsync(Guid courseId);

    Task<List<Guid>> GetCourseIdsWithActiveFinalQuizAsync(IEnumerable<Guid> courseIds);

    Task<List<Guid>> GetModuleIdsWithFailedQuizAsync(IEnumerable<Guid> moduleIds, Guid employeeId);

    Task<List<CourseProgress>> GetCourseProgressAsync(IEnumerable<Guid> courseIds, Guid employeeId);

    Task<CourseProgress?> GetCourseProgressAsync(Guid courseId, Guid employeeId);

    Task<bool> TryAddCourseProgressAsync(CourseProgress progress);

    Task<List<ModuleProgress>> GetProgressAsync(IEnumerable<Guid> moduleIds, Guid employeeId);

    Task<ModuleProgress?> GetProgressAsync(Guid moduleId, Guid employeeId);

    Task<bool> TryAddProgressAsync(ModuleProgress progress);

    Task<List<ModuleQuizAttempt>> GetAttemptsAsync(Guid quizId, Guid employeeId);

    Task<ModuleQuizAttempt?> GetAttemptWithAnswersAsync(Guid attemptId, Guid quizId, Guid employeeId);

    Task<bool> TryAddAttemptAsync(ModuleQuizAttempt attempt);

    Task<bool> TrySaveSubmissionAsync(List<ModuleQuizAnswer> answers);
}

public class LearningRepository : ILearningRepository
{
    private readonly ApplicationDbContext _context;

    public LearningRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    private IQueryable<Course> AvailableCourses(Guid companyId)
    {
        var published = CourseStatus.Published.ToString();

        return _context.Courses
            .Where(x => x.CompanyId == companyId && x.IsActive && x.Status == published);
    }

    public async Task<List<Course>> GetAvailableCoursesAsync(Guid companyId)
    {
        return await AvailableCourses(companyId)
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public async Task<Course?> GetAvailableCourseAsync(Guid courseId, Guid companyId)
    {
        return await AvailableCourses(companyId)
            .FirstOrDefaultAsync(x => x.Id == courseId);
    }

    public async Task<List<CourseModule>> GetActiveModulesAsync(IEnumerable<Guid> courseIds)
    {
        return await _context.CourseModules
            .Where(x => courseIds.Contains(x.CourseId) && x.IsActive)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.CreatedAt)
            .ThenBy(x => x.Id)
            .ToListAsync();
    }

    public async Task<CourseModule?> GetActiveModuleAsync(Guid moduleId, Guid courseId)
    {
        return await _context.CourseModules
            .FirstOrDefaultAsync(x => x.Id == moduleId && x.CourseId == courseId && x.IsActive);
    }

    public async Task<List<Guid>> GetModuleIdsWithActiveQuizAsync(IEnumerable<Guid> moduleIds)
    {
        return await _context.ModuleQuizzes
            .Where(x => x.ModuleId != null && moduleIds.Contains(x.ModuleId.Value) && x.IsActive)
            .Select(x => x.ModuleId!.Value)
            .ToListAsync();
    }

    public async Task<ModuleQuiz?> GetActiveFinalQuizAsync(Guid courseId)
    {
        return await _context.ModuleQuizzes
            .Include(x => x.Questions)
                .ThenInclude(x => x.Options)
            .AsSplitQuery()
            .FirstOrDefaultAsync(x => x.CourseId == courseId && x.IsActive);
    }

    public async Task<List<Guid>> GetCourseIdsWithActiveFinalQuizAsync(IEnumerable<Guid> courseIds)
    {
        return await _context.ModuleQuizzes
            .Where(x => x.CourseId != null && courseIds.Contains(x.CourseId.Value) && x.IsActive)
            .Select(x => x.CourseId!.Value)
            .ToListAsync();
    }

    // Modules whose active quiz the employee has submitted at least once without passing.
    public async Task<List<Guid>> GetModuleIdsWithFailedQuizAsync(IEnumerable<Guid> moduleIds, Guid employeeId)
    {
        return await _context.ModuleQuizAttempts
            .Where(x => x.EmployeeId == employeeId
                && x.SubmittedAt != null
                && !x.IsPassed
                && x.Quiz!.IsActive
                && x.Quiz.ModuleId != null
                && moduleIds.Contains(x.Quiz.ModuleId.Value))
            .Select(x => x.Quiz!.ModuleId!.Value)
            .Distinct()
            .ToListAsync();
    }

    public async Task<List<CourseProgress>> GetCourseProgressAsync(IEnumerable<Guid> courseIds, Guid employeeId)
    {
        return await _context.CourseProgress
            .Where(x => courseIds.Contains(x.CourseId) && x.EmployeeId == employeeId)
            .ToListAsync();
    }

    public async Task<CourseProgress?> GetCourseProgressAsync(Guid courseId, Guid employeeId)
    {
        return await _context.CourseProgress
            .FirstOrDefaultAsync(x => x.CourseId == courseId && x.EmployeeId == employeeId);
    }

    // Returns false when another request created the row first (unique course + employee).
    public async Task<bool> TryAddCourseProgressAsync(CourseProgress progress)
    {
        try
        {
            await _context.CourseProgress.AddAsync(progress);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException)
        {
            _context.Entry(progress).State = EntityState.Detached;
            return false;
        }
    }

    public async Task<ModuleQuiz?> GetActiveQuizAsync(Guid moduleId)
    {
        return await _context.ModuleQuizzes
            .Include(x => x.Questions)
                .ThenInclude(x => x.Options)
            .AsSplitQuery()
            .FirstOrDefaultAsync(x => x.ModuleId == moduleId && x.IsActive);
    }

    public async Task<List<ModuleProgress>> GetProgressAsync(IEnumerable<Guid> moduleIds, Guid employeeId)
    {
        return await _context.ModuleProgress
            .Where(x => moduleIds.Contains(x.ModuleId) && x.EmployeeId == employeeId)
            .ToListAsync();
    }

    public async Task<ModuleProgress?> GetProgressAsync(Guid moduleId, Guid employeeId)
    {
        return await _context.ModuleProgress
            .FirstOrDefaultAsync(x => x.ModuleId == moduleId && x.EmployeeId == employeeId);
    }

    // Returns false when another request created the row first (unique module + employee).
    public async Task<bool> TryAddProgressAsync(ModuleProgress progress)
    {
        try
        {
            await _context.ModuleProgress.AddAsync(progress);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException)
        {
            _context.Entry(progress).State = EntityState.Detached;
            return false;
        }
    }

    public async Task<List<ModuleQuizAttempt>> GetAttemptsAsync(Guid quizId, Guid employeeId)
    {
        return await _context.ModuleQuizAttempts
            .Where(x => x.QuizId == quizId && x.EmployeeId == employeeId)
            .OrderBy(x => x.AttemptNumber)
            .ToListAsync();
    }

    public async Task<ModuleQuizAttempt?> GetAttemptWithAnswersAsync(Guid attemptId, Guid quizId, Guid employeeId)
    {
        return await _context.ModuleQuizAttempts
            .Include(x => x.Answers)
                .ThenInclude(x => x.Options)
            .AsSplitQuery()
            .FirstOrDefaultAsync(x => x.Id == attemptId && x.QuizId == quizId && x.EmployeeId == employeeId);
    }

    // Returns false when a concurrent request already took this attempt number
    // (unique quiz + employee + attempt number).
    public async Task<bool> TryAddAttemptAsync(ModuleQuizAttempt attempt)
    {
        try
        {
            await _context.ModuleQuizAttempts.AddAsync(attempt);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException)
        {
            _context.Entry(attempt).State = EntityState.Detached;
            return false;
        }
    }

    // Saves the answers together with the tracked changes to the attempt and the module or course progress row
    // in one transaction. Returns false when a concurrent request already submitted the
    // attempt (unique attempt + question), in which case nothing is saved.
    public async Task<bool> TrySaveSubmissionAsync(List<ModuleQuizAnswer> answers)
    {
        try
        {
            await _context.ModuleQuizAnswers.AddRangeAsync(answers);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException)
        {
            _context.ChangeTracker.Clear();
            return false;
        }
    }
}
