using HRM.API.Data;
using HRM.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HRM.API.Repositories;

public interface IModuleQuizRepository
{
    Task<ModuleQuiz?> GetByModuleAsync(Guid moduleId, Guid courseId, Guid companyId);

    Task<ModuleQuiz?> GetFinalByCourseAsync(Guid courseId, Guid companyId);

    Task<List<CourseProgress>> GetCourseCompletionsAsync(Guid courseId, Guid companyId);

    Task AddQuizAsync(ModuleQuiz quiz);

    Task AddQuestionAsync(ModuleQuizQuestion question);

    Task AddOptionAsync(ModuleQuizOption option);

    Task SaveChangesAsync();
}

public class ModuleQuizRepository : IModuleQuizRepository
{
    private readonly ApplicationDbContext _context;

    public ModuleQuizRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // Loads the quiz with all of its questions and options (inactive ones included)
    // and verifies ownership through module -> course -> company.
    public async Task<ModuleQuiz?> GetByModuleAsync(Guid moduleId, Guid courseId, Guid companyId)
    {
        return await _context.ModuleQuizzes
            .Include(x => x.Questions)
                .ThenInclude(x => x.Options)
            .AsSplitQuery()
            .FirstOrDefaultAsync(x => x.ModuleId == moduleId
                && x.Module!.CourseId == courseId
                && x.Module.Course!.CompanyId == companyId);
    }

    // The course's final quiz, with ownership verified through course -> company.
    public async Task<ModuleQuiz?> GetFinalByCourseAsync(Guid courseId, Guid companyId)
    {
        return await _context.ModuleQuizzes
            .Include(x => x.Questions)
                .ThenInclude(x => x.Options)
            .AsSplitQuery()
            .FirstOrDefaultAsync(x => x.CourseId == courseId && x.Course!.CompanyId == companyId);
    }

    public async Task<List<CourseProgress>> GetCourseCompletionsAsync(Guid courseId, Guid companyId)
    {
        return await _context.CourseProgress
            .Include(x => x.Employee!)
                .ThenInclude(x => x.User)
            .Where(x => x.CourseId == courseId && x.Course!.CompanyId == companyId && x.CompletedAt != null)
            .OrderByDescending(x => x.CompletedAt)
            .ToListAsync();
    }

    public async Task AddQuizAsync(ModuleQuiz quiz)
    {
        await _context.ModuleQuizzes.AddAsync(quiz);
    }

    public async Task AddQuestionAsync(ModuleQuizQuestion question)
    {
        await _context.ModuleQuizQuestions.AddAsync(question);
    }

    public async Task AddOptionAsync(ModuleQuizOption option)
    {
        await _context.ModuleQuizOptions.AddAsync(option);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
