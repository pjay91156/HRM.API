using HRM.API.Models;
using HRM.API.Repositories;
using HRM.API.Responses;

namespace HRM.API.Services;

// The employee learning experience: published courses, module content, learning-content
// completion, module quizzes, the final course quiz and course completion.
// In the quiz methods a null module id means the course's final quiz.
public interface ILearningService
{
    Task<ApiResponse<List<LearningCourseResponse>>> GetCoursesAsync(Guid companyId, Guid userId);

    Task<ApiResponse<LearningCourseDetailResponse>> GetCourseAsync(Guid courseId, Guid companyId, Guid userId);

    Task<ApiResponse<CourseThumbnailFile>> GetCourseThumbnailAsync(Guid courseId, Guid companyId);

    Task<ApiResponse<LearningModuleResponse>> GetModuleAsync(Guid courseId, Guid moduleId, Guid companyId, Guid userId);

    Task<ApiResponse<string>> GetVideoTokenAsync(Guid courseId, Guid moduleId, Guid companyId, Guid userId);

    Task<ApiResponse<bool>> CompleteLearningAsync(Guid courseId, Guid moduleId, Guid companyId, Guid userId);

    Task<ApiResponse<QuizAttemptForTakingResponse>> StartAttemptAsync(Guid courseId, Guid? moduleId, Guid companyId, Guid userId);

    Task<ApiResponse<QuizAttemptResultResponse>> SubmitAttemptAsync(Guid courseId, Guid? moduleId, Guid attemptId, SubmitModuleQuizDto request, Guid companyId, Guid userId);

    Task<ApiResponse<List<QuizAttemptSummaryResponse>>> GetAttemptsAsync(Guid courseId, Guid? moduleId, Guid companyId, Guid userId);

    Task<ApiResponse<QuizAttemptResultResponse>> GetAttemptResultAsync(Guid courseId, Guid? moduleId, Guid attemptId, Guid companyId, Guid userId);
}

public class LearningService : ILearningService
{
    public const string EmployeeNotFoundMessage = "Employee profile not found.";
    public const string CourseNotFoundMessage = CourseService.NotFoundMessage;
    public const string ModuleNotFoundMessage = CourseModuleService.ModuleNotFoundMessage;
    public const string QuizNotFoundMessage = ModuleQuizService.QuizNotFoundMessage;
    public const string AttemptNotFoundMessage = "Quiz attempt not found.";

    public const string StatusNotStarted = "NotStarted";
    public const string StatusInProgress = "InProgress";
    public const string StatusFinalQuizAvailable = "FinalQuizAvailable";
    public const string StatusCompleted = "Completed";

    private const string LearningNotCompletedMessage = "Complete the module's learning content before taking the quiz.";
    private const string FinalQuizUnavailableMessage = "The final quiz for this course is not available yet.";
    private const string FinalQuizLockedMessage = "Complete every module, including its quiz, before taking the final quiz.";

    private readonly ILearningRepository _learningRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ICourseService _courseService;
    private readonly ICourseModuleService _courseModuleService;
    private readonly ILogger<LearningService> _logger;

    public LearningService(
        ILearningRepository learningRepository,
        IEmployeeRepository employeeRepository,
        ICourseService courseService,
        ICourseModuleService courseModuleService,
        ILogger<LearningService> logger)
    {
        _learningRepository = learningRepository;
        _employeeRepository = employeeRepository;
        _courseService = courseService;
        _courseModuleService = courseModuleService;
        _logger = logger;
    }

    private static ApiResponse<T> Fail<T>(string message)
    {
        return new ApiResponse<T> { Success = false, Message = message };
    }

    // Score = fully correct questions / total questions * 100, rounded to two
    // decimals with midpoints rounded away from zero (e.g. 2 of 3 = 66.67).
    public static decimal CalculateScore(int correctAnswers, int totalQuestions)
    {
        if (totalQuestions <= 0)
        {
            return 0;
        }

        return Math.Round(correctAnswers * 100m / totalQuestions, 2, MidpointRounding.AwayFromZero);
    }

    public async Task<ApiResponse<List<LearningCourseResponse>>> GetCoursesAsync(Guid companyId, Guid userId)
    {
        try
        {
            var employee = await GetEmployeeAsync(userId, companyId);

            if (employee == null)
            {
                return Fail<List<LearningCourseResponse>>(EmployeeNotFoundMessage);
            }

            var courses = await _learningRepository.GetAvailableCoursesAsync(companyId);
            var courseIds = courses.Select(x => x.Id).ToList();
            var states = await LoadModuleStatesAsync(courseIds, employee.Id);
            var courseProgress = await _learningRepository.GetCourseProgressAsync(courseIds, employee.Id);
            var finalQuizCourseIds = await _learningRepository.GetCourseIdsWithActiveFinalQuizAsync(courseIds);

            return new ApiResponse<List<LearningCourseResponse>>
            {
                Success = true,
                Message = "Courses retrieved successfully.",
                Data = courses
                    .Select(course => FillCourse(
                        new LearningCourseResponse(),
                        course,
                        states,
                        courseProgress.FirstOrDefault(x => x.CourseId == course.Id),
                        finalQuizCourseIds.Contains(course.Id)))
                    .ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving learning courses for company {CompanyId}", companyId);

            return Fail<List<LearningCourseResponse>>("An error occurred while retrieving courses.");
        }
    }

    // The employee's full progress in one course: module states, final quiz eligibility
    // and completion. Everything the frontend shows comes from here.
    public async Task<ApiResponse<LearningCourseDetailResponse>> GetCourseAsync(Guid courseId, Guid companyId, Guid userId)
    {
        try
        {
            var employee = await GetEmployeeAsync(userId, companyId);

            if (employee == null)
            {
                return Fail<LearningCourseDetailResponse>(EmployeeNotFoundMessage);
            }

            var course = await _learningRepository.GetAvailableCourseAsync(courseId, companyId);

            if (course == null)
            {
                return Fail<LearningCourseDetailResponse>(CourseNotFoundMessage);
            }

            var states = await LoadModuleStatesAsync(new[] { courseId }, employee.Id);
            var courseProgress = await _learningRepository.GetCourseProgressAsync(courseId, employee.Id);
            var finalQuiz = await _learningRepository.GetActiveFinalQuizAsync(courseId);

            var response = FillCourse(new LearningCourseDetailResponse(), course, states, courseProgress, finalQuiz != null);
            response.Modules = states.Modules
                .Select(module => FillSummary(new LearningModuleSummaryResponse(), module, states))
                .ToList();
            response.FinalQuiz = await BuildFinalQuizStateAsync(course.Id, employee.Id, states, courseProgress, finalQuiz);

            return new ApiResponse<LearningCourseDetailResponse>
            {
                Success = true,
                Message = "Course retrieved successfully.",
                Data = response
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving learning course {CourseId}", courseId);

            return Fail<LearningCourseDetailResponse>("An error occurred while retrieving the course.");
        }
    }

    public async Task<ApiResponse<CourseThumbnailFile>> GetCourseThumbnailAsync(Guid courseId, Guid companyId)
    {
        try
        {
            var course = await _learningRepository.GetAvailableCourseAsync(courseId, companyId);

            if (course == null)
            {
                return Fail<CourseThumbnailFile>(CourseNotFoundMessage);
            }

            return await _courseService.GetThumbnailAsync(courseId, companyId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving thumbnail of learning course {CourseId}", courseId);

            return Fail<CourseThumbnailFile>("An error occurred while retrieving the course thumbnail.");
        }
    }

    public async Task<ApiResponse<LearningModuleResponse>> GetModuleAsync(Guid courseId, Guid moduleId, Guid companyId, Guid userId)
    {
        try
        {
            var context = await ResolveAsync(courseId, moduleId, companyId, userId);

            if (context.Error != null)
            {
                return Fail<LearningModuleResponse>(context.Error);
            }

            var states = await LoadModuleStatesAsync(new[] { courseId }, context.Employee!.Id);
            var progress = states.Progress.FirstOrDefault(x => x.ModuleId == moduleId);
            var quiz = await _learningRepository.GetActiveQuizAsync(moduleId);

            var response = FillSummary(new LearningModuleResponse(), context.Module!, states);
            response.CourseId = context.Course!.Id;
            response.CourseName = context.Course.Name;
            response.Content = context.Module!.Content;

            // Quiz details are shown only after the learning content is completed.
            if (quiz != null && response.LearningCompleted)
            {
                var attempts = await _learningRepository.GetAttemptsAsync(quiz.Id, context.Employee.Id);
                response.Quiz = BuildQuizSummary(quiz, attempts, progress?.QuizPassedAt != null);
            }

            return new ApiResponse<LearningModuleResponse>
            {
                Success = true,
                Message = "Module retrieved successfully.",
                Data = response
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving learning module {ModuleId}", moduleId);

            return Fail<LearningModuleResponse>("An error occurred while retrieving the module.");
        }
    }

    public async Task<ApiResponse<string>> GetVideoTokenAsync(Guid courseId, Guid moduleId, Guid companyId, Guid userId)
    {
        try
        {
            var context = await ResolveAsync(courseId, moduleId, companyId, userId);

            if (context.Error != null)
            {
                return Fail<string>(context.Error);
            }

            return await _courseModuleService.CreateVideoTokenAsync(courseId, moduleId, companyId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error preparing video of learning module {ModuleId}", moduleId);

            return Fail<string>("An error occurred while preparing the video.");
        }
    }

    // The employee states that they have gone through the learning content. Video watch time
    // is not tracked, so this explicit action is what unlocks the quiz.
    public async Task<ApiResponse<bool>> CompleteLearningAsync(Guid courseId, Guid moduleId, Guid companyId, Guid userId)
    {
        try
        {
            var context = await ResolveAsync(courseId, moduleId, companyId, userId);

            if (context.Error != null)
            {
                return Fail<bool>(context.Error);
            }

            // The first learning action in a course marks the course as started.
            await EnsureCourseProgressAsync(courseId, context.Employee!.Id, userId);

            var progress = await _learningRepository.GetProgressAsync(moduleId, context.Employee.Id);

            if (progress == null)
            {
                var created = await _learningRepository.TryAddProgressAsync(new ModuleProgress
                {
                    Id = Guid.NewGuid(),
                    ModuleId = moduleId,
                    EmployeeId = context.Employee.Id,
                    LearningCompletedAt = DateTime.UtcNow,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId
                });

                // When a concurrent request created the row first, the outcome is the same.
                if (!created)
                {
                    _logger.LogInformation("Learning progress for module {ModuleId} was created by a concurrent request", moduleId);
                }
            }

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Learning content marked as complete.",
                Data = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing learning content of module {ModuleId}", moduleId);

            return Fail<bool>("An error occurred while updating your progress.");
        }
    }

    // Starts a new attempt, or returns the one that is still open so a page reload
    // does not use up another attempt.
    public async Task<ApiResponse<QuizAttemptForTakingResponse>> StartAttemptAsync(Guid courseId, Guid? moduleId, Guid companyId, Guid userId)
    {
        try
        {
            var context = await ResolveQuizAsync(courseId, moduleId, companyId, userId);

            if (context.Error != null)
            {
                return Fail<QuizAttemptForTakingResponse>(context.Error);
            }

            var quiz = context.Quiz!;
            var questions = ModuleQuizService.ToQuestionsForTaking(quiz);

            if (questions.Count == 0)
            {
                return Fail<QuizAttemptForTakingResponse>("This quiz has no questions yet.");
            }

            var attempts = await _learningRepository.GetAttemptsAsync(quiz.Id, context.Employee!.Id);
            var attempt = attempts.FirstOrDefault(x => x.SubmittedAt == null);

            if (attempt == null)
            {
                if (context.IsPassed || attempts.Any(x => x.IsPassed))
                {
                    return Fail<QuizAttemptForTakingResponse>("You have already passed this quiz.");
                }

                if (quiz.MaxAttempts.HasValue && attempts.Count >= quiz.MaxAttempts.Value)
                {
                    return Fail<QuizAttemptForTakingResponse>("You have used all attempts for this quiz.");
                }

                attempt = new ModuleQuizAttempt
                {
                    Id = Guid.NewGuid(),
                    QuizId = quiz.Id,
                    EmployeeId = context.Employee.Id,
                    AttemptNumber = attempts.Select(x => x.AttemptNumber).DefaultIfEmpty(0).Max() + 1,
                    PassingPercentage = quiz.PassingPercentage,
                    StartedAt = DateTime.UtcNow,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId
                };

                // The unique attempt number makes a concurrent start fail here instead of creating
                // an extra attempt; the attempt created by the other request is used instead.
                if (!await _learningRepository.TryAddAttemptAsync(attempt))
                {
                    attempts = await _learningRepository.GetAttemptsAsync(quiz.Id, context.Employee.Id);
                    attempt = attempts.FirstOrDefault(x => x.SubmittedAt == null);

                    if (attempt == null)
                    {
                        return Fail<QuizAttemptForTakingResponse>("The quiz could not be started. Please try again.");
                    }
                }
            }

            return new ApiResponse<QuizAttemptForTakingResponse>
            {
                Success = true,
                Message = "Quiz attempt started.",
                Data = new QuizAttemptForTakingResponse
                {
                    AttemptId = attempt.Id,
                    AttemptNumber = attempt.AttemptNumber,
                    PassingPercentage = quiz.PassingPercentage,
                    StartedAt = attempt.StartedAt,
                    Questions = questions
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting quiz attempt for course {CourseId}, module {ModuleId}", courseId, moduleId);

            return Fail<QuizAttemptForTakingResponse>("An error occurred while starting the quiz.");
        }
    }

    public async Task<ApiResponse<QuizAttemptResultResponse>> SubmitAttemptAsync(Guid courseId, Guid? moduleId, Guid attemptId, SubmitModuleQuizDto request, Guid companyId, Guid userId)
    {
        try
        {
            var context = await ResolveQuizAsync(courseId, moduleId, companyId, userId);

            if (context.Error != null)
            {
                return Fail<QuizAttemptResultResponse>(context.Error);
            }

            var quiz = context.Quiz!;
            var employeeId = context.Employee!.Id;

            // Looked up by quiz and employee, so another employee's attempt is never found.
            var attempt = await _learningRepository.GetAttemptWithAnswersAsync(attemptId, quiz.Id, employeeId);

            if (attempt == null)
            {
                return Fail<QuizAttemptResultResponse>(AttemptNotFoundMessage);
            }

            if (attempt.SubmittedAt != null)
            {
                return Fail<QuizAttemptResultResponse>("This attempt has already been submitted.");
            }

            var questions = ModuleQuizService.ActiveQuestions(quiz);

            if (questions.Count == 0)
            {
                return Fail<QuizAttemptResultResponse>("This quiz has no questions yet.");
            }

            var submitted = request.Answers ?? new List<SubmitModuleQuizAnswerDto>();

            // The submission must answer exactly the current questions, each once.
            if (submitted.Count != questions.Count
                || submitted.Select(x => x.QuestionId).Distinct().Count() != submitted.Count
                || submitted.Any(x => questions.All(question => question.Id != x.QuestionId)))
            {
                return Fail<QuizAttemptResultResponse>("The quiz has changed or some questions are missing. Reload the quiz and answer every question.");
            }

            var answers = new List<ModuleQuizAnswer>();

            foreach (var question in questions)
            {
                var options = ModuleQuizService.ActiveOptions(question);
                var selectedIds = submitted.First(x => x.QuestionId == question.Id).SelectedOptionIds ?? new List<Guid>();

                if (selectedIds.Count == 0)
                {
                    return Fail<QuizAttemptResultResponse>("Answer every question before submitting the quiz.");
                }

                // Only this question's own options are accepted, each at most once.
                if (selectedIds.Distinct().Count() != selectedIds.Count
                    || selectedIds.Any(id => options.All(option => option.Id != id)))
                {
                    return Fail<QuizAttemptResultResponse>("One or more selected options are not valid for this quiz.");
                }

                if (!question.IsMultipleAnswer && selectedIds.Count != 1)
                {
                    return Fail<QuizAttemptResultResponse>("Select exactly one option for single-answer questions.");
                }

                var answer = new ModuleQuizAnswer
                {
                    Id = Guid.NewGuid(),
                    AttemptId = attempt.Id,
                    QuestionId = question.Id,
                    QuestionText = question.QuestionText,
                    IsMultipleAnswer = question.IsMultipleAnswer,
                    SortOrder = answers.Count + 1,
                    // Correct only when the selection is exactly the set of correct options (no partial credit).
                    IsCorrect = options.All(option => option.IsCorrect == selectedIds.Contains(option.Id))
                };

                // The question's options and answer key are copied so this result stays
                // accurate even if the quiz is edited later.
                for (var index = 0; index < options.Count; index++)
                {
                    answer.Options.Add(new ModuleQuizAnswerOption
                    {
                        Id = Guid.NewGuid(),
                        AnswerId = answer.Id,
                        OptionId = options[index].Id,
                        OptionText = options[index].OptionText,
                        IsCorrect = options[index].IsCorrect,
                        IsSelected = selectedIds.Contains(options[index].Id),
                        SortOrder = index + 1
                    });
                }

                answers.Add(answer);
            }

            var now = DateTime.UtcNow;

            attempt.TotalQuestions = answers.Count;
            attempt.CorrectAnswers = answers.Count(x => x.IsCorrect);
            attempt.ScorePercentage = CalculateScore(attempt.CorrectAnswers, attempt.TotalQuestions);
            attempt.PassingPercentage = quiz.PassingPercentage;
            attempt.IsPassed = attempt.ScorePercentage >= quiz.PassingPercentage;
            attempt.SubmittedAt = now;
            attempt.UpdatedAt = now;
            attempt.UpdatedBy = userId;

            if (attempt.IsPassed)
            {
                // Passing a module quiz satisfies that module's quiz requirement; passing the
                // final quiz completes the course. Either is recorded once and never overwritten.
                context.MarkPassed(now, userId);
            }

            // The answers, the attempt and the progress row are saved in one transaction. A
            // concurrent submission of the same attempt fails as a whole, so a course
            // completion cannot be recorded twice.
            if (!await _learningRepository.TrySaveSubmissionAsync(answers))
            {
                return Fail<QuizAttemptResultResponse>("This attempt has already been submitted.");
            }

            var attempts = await _learningRepository.GetAttemptsAsync(quiz.Id, employeeId);
            attempt.Answers = answers;

            return new ApiResponse<QuizAttemptResultResponse>
            {
                Success = true,
                Message = !attempt.IsPassed
                    ? "Quiz submitted. You did not reach the passing score."
                    : context.IsFinalQuiz ? "Final quiz passed. You have completed this course." : "Quiz passed.",
                Data = ToResult(attempt, quiz, attempts, context)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting quiz attempt {AttemptId}", attemptId);

            return Fail<QuizAttemptResultResponse>("An error occurred while submitting the quiz.");
        }
    }

    public async Task<ApiResponse<List<QuizAttemptSummaryResponse>>> GetAttemptsAsync(Guid courseId, Guid? moduleId, Guid companyId, Guid userId)
    {
        try
        {
            var context = await ResolveQuizAsync(courseId, moduleId, companyId, userId);

            if (context.Error != null)
            {
                return Fail<List<QuizAttemptSummaryResponse>>(context.Error);
            }

            var attempts = await _learningRepository.GetAttemptsAsync(context.Quiz!.Id, context.Employee!.Id);

            return new ApiResponse<List<QuizAttemptSummaryResponse>>
            {
                Success = true,
                Message = "Quiz attempts retrieved successfully.",
                Data = attempts
                    .OrderByDescending(x => x.AttemptNumber)
                    .Select(x => FillAttemptSummary(new QuizAttemptSummaryResponse(), x))
                    .ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving quiz attempts for course {CourseId}, module {ModuleId}", courseId, moduleId);

            return Fail<List<QuizAttemptSummaryResponse>>("An error occurred while retrieving your attempts.");
        }
    }

    public async Task<ApiResponse<QuizAttemptResultResponse>> GetAttemptResultAsync(Guid courseId, Guid? moduleId, Guid attemptId, Guid companyId, Guid userId)
    {
        try
        {
            var context = await ResolveQuizAsync(courseId, moduleId, companyId, userId);

            if (context.Error != null)
            {
                return Fail<QuizAttemptResultResponse>(context.Error);
            }

            var quiz = context.Quiz!;
            var attempt = await _learningRepository.GetAttemptWithAnswersAsync(attemptId, quiz.Id, context.Employee!.Id);

            if (attempt == null)
            {
                return Fail<QuizAttemptResultResponse>(AttemptNotFoundMessage);
            }

            if (attempt.SubmittedAt == null)
            {
                return Fail<QuizAttemptResultResponse>("This attempt has not been submitted yet.");
            }

            var attempts = await _learningRepository.GetAttemptsAsync(quiz.Id, context.Employee.Id);

            return new ApiResponse<QuizAttemptResultResponse>
            {
                Success = true,
                Message = "Quiz result retrieved successfully.",
                Data = ToResult(attempt, quiz, attempts, context)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving quiz attempt {AttemptId}", attemptId);

            return Fail<QuizAttemptResultResponse>("An error occurred while retrieving the quiz result.");
        }
    }

    private sealed class LearningContext
    {
        public string? Error { get; init; }

        public Employee? Employee { get; init; }

        public Course? Course { get; init; }

        // Null for the final course quiz.
        public CourseModule? Module { get; init; }

        public ModuleQuiz? Quiz { get; init; }

        // Set for a module quiz.
        public ModuleProgress? Progress { get; init; }

        // Set for the final course quiz.
        public CourseProgress? CourseProgress { get; init; }

        public bool IsFinalQuiz => CourseProgress != null;

        public bool IsPassed => IsFinalQuiz ? CourseProgress!.CompletedAt != null : Progress?.QuizPassedAt != null;

        // Records the pass on the tracked progress row; an earlier pass is kept as it is.
        public void MarkPassed(DateTime now, Guid userId)
        {
            if (IsPassed)
            {
                return;
            }

            if (IsFinalQuiz)
            {
                CourseProgress!.CompletedAt = now;
                CourseProgress.UpdatedAt = now;
                CourseProgress.UpdatedBy = userId;
            }
            else
            {
                Progress!.QuizPassedAt = now;
                Progress.UpdatedAt = now;
                Progress.UpdatedBy = userId;
            }
        }
    }

    // The progress of one employee through the active modules of the given courses.
    private sealed class ModuleStates
    {
        public List<CourseModule> Modules { get; init; } = new();

        public List<ModuleProgress> Progress { get; init; } = new();

        public HashSet<Guid> QuizModuleIds { get; init; } = new();

        public HashSet<Guid> FailedQuizModuleIds { get; init; } = new();

        public ModuleProgress? ProgressOf(CourseModule module) => Progress.FirstOrDefault(x => x.ModuleId == module.Id);

        // A module is completed when its learning content is marked complete and,
        // if it has an active quiz, that quiz has been passed.
        public bool IsCompleted(CourseModule module)
        {
            var progress = ProgressOf(module);

            return progress?.LearningCompletedAt != null
                && (!QuizModuleIds.Contains(module.Id) || progress.QuizPassedAt != null);
        }

        public List<CourseModule> ModulesOf(Guid courseId) => Modules.Where(x => x.CourseId == courseId).ToList();

        // Every required (active) module is completed. A course without any active module
        // never counts as "all completed", so its final quiz stays locked.
        public bool AllRequiredModulesCompleted(Guid courseId)
        {
            var modules = ModulesOf(courseId);

            return modules.Count > 0 && modules.All(IsCompleted);
        }
    }

    private async Task<ModuleStates> LoadModuleStatesAsync(IEnumerable<Guid> courseIds, Guid employeeId)
    {
        var modules = await _learningRepository.GetActiveModulesAsync(courseIds.ToList());
        var moduleIds = modules.Select(x => x.Id).ToList();

        return new ModuleStates
        {
            Modules = modules,
            Progress = await _learningRepository.GetProgressAsync(moduleIds, employeeId),
            QuizModuleIds = (await _learningRepository.GetModuleIdsWithActiveQuizAsync(moduleIds)).ToHashSet(),
            FailedQuizModuleIds = (await _learningRepository.GetModuleIdsWithFailedQuizAsync(moduleIds, employeeId)).ToHashSet()
        };
    }

    // The signed-in user's own active employee record in the given company. The employee id
    // is always derived here, never taken from the request.
    private async Task<Employee?> GetEmployeeAsync(Guid userId, Guid companyId)
    {
        var employee = await _employeeRepository.GetByUserIdAsync(userId);

        if (employee == null || !employee.IsActive || employee.User.CompanyId != companyId)
        {
            return null;
        }

        return employee;
    }

    // Employee + published, active course of their company + active module of that course.
    private async Task<LearningContext> ResolveAsync(Guid courseId, Guid moduleId, Guid companyId, Guid userId)
    {
        var employee = await GetEmployeeAsync(userId, companyId);

        if (employee == null)
        {
            return new LearningContext { Error = EmployeeNotFoundMessage };
        }

        var course = await _learningRepository.GetAvailableCourseAsync(courseId, companyId);

        if (course == null)
        {
            return new LearningContext { Error = CourseNotFoundMessage };
        }

        var module = await _learningRepository.GetActiveModuleAsync(moduleId, courseId);

        if (module == null)
        {
            return new LearningContext { Error = ModuleNotFoundMessage };
        }

        return new LearningContext { Employee = employee, Course = course, Module = module };
    }

    private async Task<LearningContext> ResolveQuizAsync(Guid courseId, Guid? moduleId, Guid companyId, Guid userId)
    {
        return moduleId.HasValue
            ? await ResolveModuleQuizAsync(courseId, moduleId.Value, companyId, userId)
            : await ResolveFinalQuizAsync(courseId, companyId, userId);
    }

    // As ResolveAsync, plus an active quiz and completed learning content.
    private async Task<LearningContext> ResolveModuleQuizAsync(Guid courseId, Guid moduleId, Guid companyId, Guid userId)
    {
        var context = await ResolveAsync(courseId, moduleId, companyId, userId);

        if (context.Error != null)
        {
            return context;
        }

        var quiz = await _learningRepository.GetActiveQuizAsync(moduleId);

        if (quiz == null)
        {
            return new LearningContext { Error = QuizNotFoundMessage };
        }

        var progress = await _learningRepository.GetProgressAsync(moduleId, context.Employee!.Id);

        if (progress?.LearningCompletedAt == null)
        {
            return new LearningContext { Error = LearningNotCompletedMessage };
        }

        return new LearningContext
        {
            Employee = context.Employee,
            Course = context.Course,
            Module = context.Module,
            Quiz = quiz,
            Progress = progress
        };
    }

    // Employee + published, active course of their company + active final quiz + eligibility.
    // Eligibility is checked here on every final quiz request, whatever the frontend shows.
    private async Task<LearningContext> ResolveFinalQuizAsync(Guid courseId, Guid companyId, Guid userId)
    {
        var employee = await GetEmployeeAsync(userId, companyId);

        if (employee == null)
        {
            return new LearningContext { Error = EmployeeNotFoundMessage };
        }

        var course = await _learningRepository.GetAvailableCourseAsync(courseId, companyId);

        if (course == null)
        {
            return new LearningContext { Error = CourseNotFoundMessage };
        }

        var quiz = await _learningRepository.GetActiveFinalQuizAsync(courseId);

        if (quiz == null)
        {
            return new LearningContext { Error = FinalQuizUnavailableMessage };
        }

        var courseProgress = await _learningRepository.GetCourseProgressAsync(courseId, employee.Id);

        // A course that is already completed stays accessible for viewing results even if
        // modules were added or changed afterwards.
        if (courseProgress?.CompletedAt == null)
        {
            var states = await LoadModuleStatesAsync(new[] { courseId }, employee.Id);

            if (!states.AllRequiredModulesCompleted(courseId))
            {
                return new LearningContext { Error = FinalQuizLockedMessage };
            }
        }

        courseProgress ??= await EnsureCourseProgressAsync(courseId, employee.Id, userId);

        return new LearningContext
        {
            Employee = employee,
            Course = course,
            Quiz = quiz,
            CourseProgress = courseProgress
        };
    }

    // Returns the employee's progress row for the course, creating it (course started) when
    // missing. The unique course + employee index keeps concurrent requests to a single row.
    private async Task<CourseProgress> EnsureCourseProgressAsync(Guid courseId, Guid employeeId, Guid userId)
    {
        var progress = await _learningRepository.GetCourseProgressAsync(courseId, employeeId);

        if (progress != null)
        {
            return progress;
        }

        var now = DateTime.UtcNow;

        progress = new CourseProgress
        {
            Id = Guid.NewGuid(),
            CourseId = courseId,
            EmployeeId = employeeId,
            StartedAt = now,
            IsActive = true,
            CreatedAt = now,
            CreatedBy = userId
        };

        if (await _learningRepository.TryAddCourseProgressAsync(progress))
        {
            return progress;
        }

        return await _learningRepository.GetCourseProgressAsync(courseId, employeeId)
            ?? throw new InvalidOperationException("Course progress could not be created.");
    }

    private static T FillCourse<T>(T response, Course course, ModuleStates states, CourseProgress? courseProgress, bool hasActiveFinalQuiz)
        where T : LearningCourseResponse
    {
        var modules = states.ModulesOf(course.Id);

        response.Id = course.Id;
        response.Name = course.Name;
        response.Description = course.Description;
        response.Thumbnail = course.Thumbnail;
        response.ModuleCount = modules.Count;
        response.CompletedModuleCount = modules.Count(states.IsCompleted);
        response.StartedAt = courseProgress?.StartedAt;
        response.CompletedAt = courseProgress?.CompletedAt;

        if (courseProgress?.CompletedAt != null)
        {
            response.Status = StatusCompleted;
        }
        else if (hasActiveFinalQuiz && states.AllRequiredModulesCompleted(course.Id))
        {
            response.Status = StatusFinalQuizAvailable;
        }
        else if (courseProgress != null || modules.Any(module => states.ProgressOf(module) != null))
        {
            response.Status = StatusInProgress;
        }
        else
        {
            response.Status = StatusNotStarted;
        }

        return response;
    }

    private async Task<LearningFinalQuizResponse> BuildFinalQuizStateAsync(
        Guid courseId, Guid employeeId, ModuleStates states, CourseProgress? courseProgress, ModuleQuiz? finalQuiz)
    {
        var isCompleted = courseProgress?.CompletedAt != null;
        LearningQuizSummaryResponse? summary = null;

        if (finalQuiz != null && (isCompleted || states.AllRequiredModulesCompleted(courseId)))
        {
            var attempts = await _learningRepository.GetAttemptsAsync(finalQuiz.Id, employeeId);
            summary = BuildQuizSummary(finalQuiz, attempts, isCompleted);
        }

        if (isCompleted)
        {
            return new LearningFinalQuizResponse
            {
                State = "Passed",
                Message = "You have passed the final quiz and completed this course.",
                Quiz = summary
            };
        }

        if (finalQuiz == null)
        {
            return new LearningFinalQuizResponse { State = "Unavailable", Message = FinalQuizUnavailableMessage };
        }

        var modules = states.ModulesOf(courseId);

        if (modules.Count == 0)
        {
            return new LearningFinalQuizResponse
            {
                State = "Locked",
                Message = "This course has no modules yet, so the final quiz cannot be taken."
            };
        }

        var remaining = modules.Count(module => !states.IsCompleted(module));

        if (remaining > 0)
        {
            return new LearningFinalQuizResponse
            {
                State = "Locked",
                Message = $"Complete every module, including its quiz, to unlock the final quiz. {remaining} of {modules.Count} modules remaining."
            };
        }

        return summary!.CanStartAttempt
            ? new LearningFinalQuizResponse
            {
                State = "Available",
                Message = "You have completed every module. Pass the final quiz to complete this course.",
                Quiz = summary
            }
            : new LearningFinalQuizResponse
            {
                State = "NoAttemptsLeft",
                Message = "You have used all attempts for the final quiz.",
                Quiz = summary
            };
    }

    private static T FillSummary<T>(T summary, CourseModule module, ModuleStates states)
        where T : LearningModuleSummaryResponse
    {
        var progress = states.ProgressOf(module);
        var hasQuiz = states.QuizModuleIds.Contains(module.Id);

        summary.Id = module.Id;
        summary.Title = module.Title;
        summary.Description = module.Description;
        summary.HasVideo = module.VideoPath != null;
        summary.HasText = module.Content != null;
        summary.HasQuiz = hasQuiz;
        summary.LearningCompleted = progress?.LearningCompletedAt != null;
        summary.QuizPassed = progress?.QuizPassedAt != null;
        summary.QuizStatus = !hasQuiz ? "None"
            : summary.QuizPassed ? "Passed"
            : states.FailedQuizModuleIds.Contains(module.Id) ? "Failed"
            : "NotAttempted";
        summary.IsCompleted = states.IsCompleted(module);

        return summary;
    }

    private static int? GetAttemptsRemaining(ModuleQuiz quiz, List<ModuleQuizAttempt> attempts)
    {
        return quiz.MaxAttempts.HasValue ? Math.Max(0, quiz.MaxAttempts.Value - attempts.Count) : null;
    }

    private static LearningQuizSummaryResponse BuildQuizSummary(ModuleQuiz quiz, List<ModuleQuizAttempt> attempts, bool isPassed)
    {
        var remaining = GetAttemptsRemaining(quiz, attempts);
        var hasOpenAttempt = attempts.Any(x => x.SubmittedAt == null);

        return new LearningQuizSummaryResponse
        {
            QuestionCount = ModuleQuizService.ActiveQuestions(quiz).Count,
            PassingPercentage = quiz.PassingPercentage,
            MaxAttempts = quiz.MaxAttempts,
            AttemptsUsed = attempts.Count,
            AttemptsRemaining = remaining,
            IsPassed = isPassed,
            HasOpenAttempt = hasOpenAttempt,
            CanStartAttempt = !isPassed && (hasOpenAttempt || remaining == null || remaining > 0)
        };
    }

    private static T FillAttemptSummary<T>(T summary, ModuleQuizAttempt attempt)
        where T : QuizAttemptSummaryResponse
    {
        summary.AttemptId = attempt.Id;
        summary.AttemptNumber = attempt.AttemptNumber;
        summary.IsSubmitted = attempt.SubmittedAt != null;
        summary.TotalQuestions = attempt.TotalQuestions;
        summary.CorrectAnswers = attempt.CorrectAnswers;
        summary.ScorePercentage = attempt.ScorePercentage;
        summary.PassingPercentage = attempt.PassingPercentage;
        summary.IsPassed = attempt.IsPassed;
        summary.StartedAt = attempt.StartedAt;
        summary.SubmittedAt = attempt.SubmittedAt;

        return summary;
    }

    private static QuizAttemptResultResponse ToResult(ModuleQuizAttempt attempt, ModuleQuiz quiz, List<ModuleQuizAttempt> attempts, LearningContext context)
    {
        var remaining = GetAttemptsRemaining(quiz, attempts);
        var isPassed = context.IsPassed;

        // The answer key stays hidden while the employee can still retry, so a failed
        // attempt cannot be used to copy the correct answers into the next one.
        var result = FillAttemptSummary(new QuizAttemptResultResponse(), attempt);
        result.AttemptsRemaining = remaining;
        result.CanRetry = !isPassed && (remaining == null || remaining > 0);
        result.CorrectAnswersRevealed = isPassed;
        result.CourseCompletedAt = context.CourseProgress?.CompletedAt;
        result.Answers = attempt.Answers
            .OrderBy(x => x.SortOrder)
            .Select(answer => new QuizAttemptAnswerResponse
            {
                QuestionText = answer.QuestionText,
                IsMultipleAnswer = answer.IsMultipleAnswer,
                IsCorrect = answer.IsCorrect,
                Options = answer.Options
                    .OrderBy(x => x.SortOrder)
                    .Select(option => new QuizAttemptAnswerOptionResponse
                    {
                        OptionText = option.OptionText,
                        IsSelected = option.IsSelected,
                        IsCorrect = isPassed ? option.IsCorrect : null
                    })
                    .ToList()
            })
            .ToList();

        return result;
    }
}
