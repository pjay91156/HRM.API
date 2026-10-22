using HRM.API.Models;
using HRM.API.Repositories;
using HRM.API.Responses;

namespace HRM.API.Services;

// HR/Admin management of quizzes. A quiz is addressed by its course and module;
// a null module id means the course's final quiz.
public interface IModuleQuizService
{
    Task<ApiResponse<ModuleQuizResponse>> GetAsync(Guid courseId, Guid? moduleId, Guid companyId);

    Task<ApiResponse<Guid>> SaveSettingsAsync(Guid courseId, Guid? moduleId, SaveModuleQuizSettingsDto request, Guid companyId, Guid userId);

    Task<ApiResponse<bool>> SetActiveAsync(Guid courseId, Guid? moduleId, bool isActive, Guid companyId, Guid userId);

    Task<ApiResponse<Guid>> AddQuestionAsync(Guid courseId, Guid? moduleId, SaveModuleQuizQuestionDto request, Guid companyId, Guid userId);

    Task<ApiResponse<bool>> UpdateQuestionAsync(Guid courseId, Guid? moduleId, Guid questionId, SaveModuleQuizQuestionDto request, Guid companyId, Guid userId);

    Task<ApiResponse<bool>> DeleteQuestionAsync(Guid courseId, Guid? moduleId, Guid questionId, Guid companyId, Guid userId);

    Task<ApiResponse<bool>> ReorderQuestionsAsync(Guid courseId, Guid? moduleId, ReorderModuleQuizQuestionsDto request, Guid companyId, Guid userId);

    Task<ApiResponse<List<QuizQuestionForTakingResponse>>> GetPreviewAsync(Guid courseId, Guid? moduleId, Guid companyId);

    Task<ApiResponse<List<CourseCompletionResponse>>> GetCourseCompletionsAsync(Guid courseId, Guid companyId);
}

public class ModuleQuizService : IModuleQuizService
{
    public const string CourseNotFoundMessage = CourseService.NotFoundMessage;
    public const string ModuleNotFoundMessage = CourseModuleService.ModuleNotFoundMessage;
    public const string QuizNotFoundMessage = "Quiz not found.";
    public const string QuestionNotFoundMessage = "Question not found.";

    private const string InactiveCourseMessage = "This course is inactive. Activate the course before changing its quizzes.";

    private const int MaxQuestionLength = 1000;
    private const int MaxOptionLength = 500;
    private const int MinOptions = 2;
    private const int MaxOptions = 10;
    private const int MaxAttemptsLimit = 100;

    private readonly IModuleQuizRepository _quizRepository;
    private readonly ICourseModuleRepository _moduleRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly ILogger<ModuleQuizService> _logger;

    public ModuleQuizService(
        IModuleQuizRepository quizRepository,
        ICourseModuleRepository moduleRepository,
        ICourseRepository courseRepository,
        ILogger<ModuleQuizService> logger)
    {
        _quizRepository = quizRepository;
        _moduleRepository = moduleRepository;
        _courseRepository = courseRepository;
        _logger = logger;
    }

    private static ApiResponse<T> Fail<T>(string message)
    {
        return new ApiResponse<T> { Success = false, Message = message };
    }

    public static List<ModuleQuizQuestion> ActiveQuestions(ModuleQuiz quiz)
    {
        return quiz.Questions
            .Where(x => x.IsActive)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.CreatedAt)
            .ThenBy(x => x.Id)
            .ToList();
    }

    public static List<ModuleQuizOption> ActiveOptions(ModuleQuizQuestion question)
    {
        return question.Options
            .Where(x => x.IsActive)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.CreatedAt)
            .ThenBy(x => x.Id)
            .ToList();
    }

    // Questions and options without the answer key, as shown to someone taking the quiz.
    public static List<QuizQuestionForTakingResponse> ToQuestionsForTaking(ModuleQuiz quiz)
    {
        return ActiveQuestions(quiz)
            .Select(question => new QuizQuestionForTakingResponse
            {
                Id = question.Id,
                QuestionText = question.QuestionText,
                IsMultipleAnswer = question.IsMultipleAnswer,
                Options = ActiveOptions(question)
                    .Select(option => new QuizOptionForTakingResponse { Id = option.Id, OptionText = option.OptionText })
                    .ToList()
            })
            .ToList();
    }

    private static ModuleQuizResponse ToResponse(ModuleQuiz quiz)
    {
        return new ModuleQuizResponse
        {
            Id = quiz.Id,
            ModuleId = quiz.ModuleId,
            CourseId = quiz.CourseId,
            PassingPercentage = quiz.PassingPercentage,
            MaxAttempts = quiz.MaxAttempts,
            IsActive = quiz.IsActive,
            Questions = ActiveQuestions(quiz)
                .Select(question => new ModuleQuizQuestionResponse
                {
                    Id = question.Id,
                    QuestionText = question.QuestionText,
                    IsMultipleAnswer = question.IsMultipleAnswer,
                    SortOrder = question.SortOrder,
                    Options = ActiveOptions(question)
                        .Select(option => new ModuleQuizOptionResponse
                        {
                            Id = option.Id,
                            OptionText = option.OptionText,
                            IsCorrect = option.IsCorrect,
                            SortOrder = option.SortOrder
                        })
                        .ToList()
                })
                .ToList()
        };
    }

    public async Task<ApiResponse<ModuleQuizResponse>> GetAsync(Guid courseId, Guid? moduleId, Guid companyId)
    {
        try
        {
            if (await _courseRepository.GetByIdAsync(courseId, companyId) == null)
            {
                return Fail<ModuleQuizResponse>(CourseNotFoundMessage);
            }

            if (moduleId.HasValue && await _moduleRepository.GetByIdAsync(moduleId.Value, courseId, companyId) == null)
            {
                return Fail<ModuleQuizResponse>(ModuleNotFoundMessage);
            }

            var quiz = await GetQuizAsync(courseId, moduleId, companyId);

            // A module without a quiz is a normal state, so this is a success with no data.
            return new ApiResponse<ModuleQuizResponse>
            {
                Success = true,
                Message = quiz == null ? "No quiz has been set up yet." : "Quiz retrieved successfully.",
                Data = quiz == null ? null : ToResponse(quiz)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving quiz of module {ModuleId}", moduleId);

            return Fail<ModuleQuizResponse>("An error occurred while retrieving the quiz.");
        }
    }

    public async Task<ApiResponse<Guid>> SaveSettingsAsync(Guid courseId, Guid? moduleId, SaveModuleQuizSettingsDto request, Guid companyId, Guid userId)
    {
        try
        {
            var (error, _) = await GetModuleForChangeAsync(courseId, moduleId, companyId);

            if (error != null)
            {
                return Fail<Guid>(error);
            }

            if (request.PassingPercentage is null or < 0 or > 100)
            {
                return Fail<Guid>("Passing percentage must be between 0 and 100.");
            }

            if (request.MaxAttempts is < 1 or > MaxAttemptsLimit)
            {
                return Fail<Guid>($"Maximum attempts must be between 1 and {MaxAttemptsLimit}, or left empty for unlimited attempts.");
            }

            var quiz = await GetQuizAsync(courseId, moduleId, companyId);

            if (quiz == null)
            {
                // A new quiz starts inactive; it is activated once it has questions.
                quiz = new ModuleQuiz
                {
                    Id = Guid.NewGuid(),
                    ModuleId = moduleId,
                    CourseId = moduleId.HasValue ? null : courseId,
                    PassingPercentage = request.PassingPercentage.Value,
                    MaxAttempts = request.MaxAttempts,
                    IsActive = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId
                };

                await _quizRepository.AddQuizAsync(quiz);
            }
            else
            {
                // Attempts already submitted keep the passing percentage they were scored with.
                quiz.PassingPercentage = request.PassingPercentage.Value;
                quiz.MaxAttempts = request.MaxAttempts;
                quiz.UpdatedAt = DateTime.UtcNow;
                quiz.UpdatedBy = userId;
            }

            await _quizRepository.SaveChangesAsync();

            return new ApiResponse<Guid>
            {
                Success = true,
                Message = "Quiz settings saved successfully.",
                Data = quiz.Id
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving quiz settings of module {ModuleId}", moduleId);

            return Fail<Guid>("An error occurred while saving the quiz settings.");
        }
    }

    public async Task<ApiResponse<bool>> SetActiveAsync(Guid courseId, Guid? moduleId, bool isActive, Guid companyId, Guid userId)
    {
        try
        {
            var (error, module) = await GetModuleForChangeAsync(courseId, moduleId, companyId);

            if (error != null)
            {
                return Fail<bool>(error);
            }

            var quiz = await GetQuizAsync(courseId, moduleId, companyId);

            if (quiz == null)
            {
                return Fail<bool>(QuizNotFoundMessage);
            }

            if (quiz.IsActive == isActive)
            {
                return Fail<bool>(isActive ? "Quiz is already active." : "Quiz is already inactive.");
            }

            if (isActive)
            {
                if (module is { IsActive: false })
                {
                    return Fail<bool>("The module is inactive. Activate the module before activating its quiz.");
                }

                // Every saved question is already validated, so one active question is enough.
                if (ActiveQuestions(quiz).Count == 0)
                {
                    return Fail<bool>("Add at least one question before activating the quiz.");
                }
            }

            quiz.IsActive = isActive;
            quiz.UpdatedAt = DateTime.UtcNow;
            quiz.UpdatedBy = userId;

            await _quizRepository.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Message = isActive ? "Quiz activated successfully." : "Quiz deactivated successfully.",
                Data = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing active state of the quiz of module {ModuleId}", moduleId);

            return Fail<bool>("An error occurred while updating the quiz.");
        }
    }

    public async Task<ApiResponse<Guid>> AddQuestionAsync(Guid courseId, Guid? moduleId, SaveModuleQuizQuestionDto request, Guid companyId, Guid userId)
    {
        try
        {
            var (error, _) = await GetModuleForChangeAsync(courseId, moduleId, companyId);

            if (error != null)
            {
                return Fail<Guid>(error);
            }

            var quiz = await GetQuizAsync(courseId, moduleId, companyId);

            if (quiz == null)
            {
                return Fail<Guid>("Save the quiz settings before adding questions.");
            }

            var validation = ValidateQuestion(request, out var questionText, out var options);

            if (validation != null)
            {
                return Fail<Guid>(validation);
            }

            if (options.Any(x => x.Id.HasValue))
            {
                return Fail<Guid>("A new question cannot reference existing options.");
            }

            var now = DateTime.UtcNow;

            var question = new ModuleQuizQuestion
            {
                Id = Guid.NewGuid(),
                QuizId = quiz.Id,
                QuestionText = questionText,
                IsMultipleAnswer = request.IsMultipleAnswer,
                SortOrder = quiz.Questions.Where(x => x.IsActive).Select(x => x.SortOrder).DefaultIfEmpty(0).Max() + 1,
                IsActive = true,
                CreatedAt = now,
                CreatedBy = userId
            };

            for (var index = 0; index < options.Count; index++)
            {
                question.Options.Add(NewOption(question.Id, options[index], index + 1, now, userId));
            }

            await _quizRepository.AddQuestionAsync(question);

            await _quizRepository.SaveChangesAsync();

            return new ApiResponse<Guid>
            {
                Success = true,
                Message = "Question added successfully.",
                Data = question.Id
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding a question to the quiz of module {ModuleId}", moduleId);

            return Fail<Guid>("An error occurred while adding the question.");
        }
    }

    public async Task<ApiResponse<bool>> UpdateQuestionAsync(Guid courseId, Guid? moduleId, Guid questionId, SaveModuleQuizQuestionDto request, Guid companyId, Guid userId)
    {
        try
        {
            var (error, _) = await GetModuleForChangeAsync(courseId, moduleId, companyId);

            if (error != null)
            {
                return Fail<bool>(error);
            }

            var quiz = await GetQuizAsync(courseId, moduleId, companyId);
            var question = quiz?.Questions.FirstOrDefault(x => x.Id == questionId && x.IsActive);

            if (question == null)
            {
                return Fail<bool>(QuestionNotFoundMessage);
            }

            var validation = ValidateQuestion(request, out var questionText, out var options);

            if (validation != null)
            {
                return Fail<bool>(validation);
            }

            var existingOptions = ActiveOptions(question);
            var requestedIds = options.Where(x => x.Id.HasValue).Select(x => x.Id!.Value).ToList();

            // Only options that belong to this question may be referenced, each at most once.
            if (requestedIds.Distinct().Count() != requestedIds.Count
                || requestedIds.Any(id => existingOptions.All(x => x.Id != id)))
            {
                return Fail<bool>("One or more answer options do not belong to this question.");
            }

            var now = DateTime.UtcNow;

            // Past attempts keep their own copy of the question, so editing it here does not change them.
            question.QuestionText = questionText;
            question.IsMultipleAnswer = request.IsMultipleAnswer;
            question.UpdatedAt = now;
            question.UpdatedBy = userId;

            foreach (var removed in existingOptions.Where(x => !requestedIds.Contains(x.Id)))
            {
                removed.IsActive = false;
                removed.UpdatedAt = now;
                removed.UpdatedBy = userId;
            }

            for (var index = 0; index < options.Count; index++)
            {
                var requested = options[index];

                if (requested.Id == null)
                {
                    await _quizRepository.AddOptionAsync(NewOption(question.Id, requested, index + 1, now, userId));
                    continue;
                }

                var option = existingOptions.First(x => x.Id == requested.Id);
                option.OptionText = requested.Text;
                option.IsCorrect = requested.IsCorrect;
                option.SortOrder = index + 1;
                option.UpdatedAt = now;
                option.UpdatedBy = userId;
            }

            await _quizRepository.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Question updated successfully.",
                Data = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating question {QuestionId}", questionId);

            return Fail<bool>("An error occurred while updating the question.");
        }
    }

    public async Task<ApiResponse<bool>> DeleteQuestionAsync(Guid courseId, Guid? moduleId, Guid questionId, Guid companyId, Guid userId)
    {
        try
        {
            var (error, _) = await GetModuleForChangeAsync(courseId, moduleId, companyId);

            if (error != null)
            {
                return Fail<bool>(error);
            }

            var quiz = await GetQuizAsync(courseId, moduleId, companyId);
            var question = quiz?.Questions.FirstOrDefault(x => x.Id == questionId && x.IsActive);

            if (quiz == null || question == null)
            {
                return Fail<bool>(QuestionNotFoundMessage);
            }

            if (quiz.IsActive && ActiveQuestions(quiz).Count == 1)
            {
                return Fail<bool>("An active quiz needs at least one question. Deactivate the quiz before deleting its last question.");
            }

            // Kept as inactive so attempts that answered this question stay intact.
            question.IsActive = false;
            question.UpdatedAt = DateTime.UtcNow;
            question.UpdatedBy = userId;

            await _quizRepository.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Question deleted successfully.",
                Data = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting question {QuestionId}", questionId);

            return Fail<bool>("An error occurred while deleting the question.");
        }
    }

    public async Task<ApiResponse<bool>> ReorderQuestionsAsync(Guid courseId, Guid? moduleId, ReorderModuleQuizQuestionsDto request, Guid companyId, Guid userId)
    {
        try
        {
            var (error, _) = await GetModuleForChangeAsync(courseId, moduleId, companyId);

            if (error != null)
            {
                return Fail<bool>(error);
            }

            var quiz = await GetQuizAsync(courseId, moduleId, companyId);

            if (quiz == null)
            {
                return Fail<bool>(QuizNotFoundMessage);
            }

            var questions = ActiveQuestions(quiz);
            var orderedIds = request.QuestionIds ?? new List<Guid>();

            // The list must name every question of this quiz exactly once.
            if (orderedIds.Count != questions.Count
                || orderedIds.Distinct().Count() != orderedIds.Count
                || orderedIds.Any(id => questions.All(x => x.Id != id)))
            {
                return Fail<bool>("The question order is out of date. Reload the quiz and try again.");
            }

            var now = DateTime.UtcNow;

            for (var index = 0; index < orderedIds.Count; index++)
            {
                var question = questions.First(x => x.Id == orderedIds[index]);

                if (question.SortOrder == index + 1)
                {
                    continue;
                }

                question.SortOrder = index + 1;
                question.UpdatedAt = now;
                question.UpdatedBy = userId;
            }

            await _quizRepository.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Question order updated successfully.",
                Data = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reordering questions of the quiz of module {ModuleId}", moduleId);

            return Fail<bool>("An error occurred while reordering the questions.");
        }
    }

    public async Task<ApiResponse<List<QuizQuestionForTakingResponse>>> GetPreviewAsync(Guid courseId, Guid? moduleId, Guid companyId)
    {
        try
        {
            var quiz = await GetQuizAsync(courseId, moduleId, companyId);

            if (quiz == null)
            {
                return Fail<List<QuizQuestionForTakingResponse>>(QuizNotFoundMessage);
            }

            return new ApiResponse<List<QuizQuestionForTakingResponse>>
            {
                Success = true,
                Message = "Quiz preview retrieved successfully.",
                Data = ToQuestionsForTaking(quiz)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving quiz preview of module {ModuleId}", moduleId);

            return Fail<List<QuizQuestionForTakingResponse>>("An error occurred while retrieving the quiz preview.");
        }
    }

    private async Task<ModuleQuiz?> GetQuizAsync(Guid courseId, Guid? moduleId, Guid companyId)
    {
        return moduleId.HasValue
            ? await _quizRepository.GetByModuleAsync(moduleId.Value, courseId, companyId)
            : await _quizRepository.GetFinalByCourseAsync(courseId, companyId);
    }

    public async Task<ApiResponse<List<CourseCompletionResponse>>> GetCourseCompletionsAsync(Guid courseId, Guid companyId)
    {
        try
        {
            if (await _courseRepository.GetByIdAsync(courseId, companyId) == null)
            {
                return Fail<List<CourseCompletionResponse>>(CourseNotFoundMessage);
            }

            var completions = await _quizRepository.GetCourseCompletionsAsync(courseId, companyId);

            return new ApiResponse<List<CourseCompletionResponse>>
            {
                Success = true,
                Message = "Course completions retrieved successfully.",
                Data = completions
                    .Select(x => new CourseCompletionResponse
                    {
                        EmployeeId = x.EmployeeId,
                        EmployeeCode = x.Employee!.EmployeeCode,
                        EmployeeName = $"{x.Employee.User.FirstName} {x.Employee.User.LastName}".Trim(),
                        StartedAt = x.StartedAt,
                        CompletedAt = x.CompletedAt!.Value
                    })
                    .ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving completions of course {CourseId}", courseId);

            return Fail<List<CourseCompletionResponse>>("An error occurred while retrieving course completions.");
        }
    }

    // Returns an error when the course or module is missing, belongs to another company, or the course is inactive.
    // The returned module is null for a final course quiz.
    private async Task<(string? Error, CourseModule? Module)> GetModuleForChangeAsync(Guid courseId, Guid? moduleId, Guid companyId)
    {
        var course = await _courseRepository.GetByIdAsync(courseId, companyId);

        if (course == null)
        {
            return (CourseNotFoundMessage, null);
        }

        CourseModule? module = null;

        if (moduleId.HasValue)
        {
            module = await _moduleRepository.GetByIdAsync(moduleId.Value, courseId, companyId);

            if (module == null)
            {
                return (ModuleNotFoundMessage, null);
            }
        }

        if (!course.IsActive)
        {
            return (InactiveCourseMessage, null);
        }

        return (null, module);
    }

    private static ModuleQuizOption NewOption(Guid questionId, ValidatedOption option, int sortOrder, DateTime now, Guid userId)
    {
        return new ModuleQuizOption
        {
            Id = Guid.NewGuid(),
            QuestionId = questionId,
            OptionText = option.Text,
            IsCorrect = option.IsCorrect,
            SortOrder = sortOrder,
            IsActive = true,
            CreatedAt = now,
            CreatedBy = userId
        };
    }

    private sealed record ValidatedOption(Guid? Id, string Text, bool IsCorrect);

    private static string? ValidateQuestion(SaveModuleQuizQuestionDto request, out string questionText, out List<ValidatedOption> options)
    {
        questionText = request.QuestionText?.Trim() ?? string.Empty;
        options = (request.Options ?? new List<SaveModuleQuizOptionDto>())
            .Select(x => new ValidatedOption(x.Id, x.OptionText?.Trim() ?? string.Empty, x.IsCorrect))
            .ToList();

        if (string.IsNullOrEmpty(questionText))
        {
            return "Question text is required and cannot contain only spaces.";
        }

        if (questionText.Length > MaxQuestionLength)
        {
            return $"Question text cannot exceed {MaxQuestionLength} characters.";
        }

        if (options.Count < MinOptions || options.Count > MaxOptions)
        {
            return $"A question needs between {MinOptions} and {MaxOptions} answer options.";
        }

        if (options.Any(x => string.IsNullOrEmpty(x.Text)))
        {
            return "Answer options cannot be empty.";
        }

        if (options.Any(x => x.Text.Length > MaxOptionLength))
        {
            return $"An answer option cannot exceed {MaxOptionLength} characters.";
        }

        if (options.Select(x => x.Text.ToLowerInvariant()).Distinct().Count() != options.Count)
        {
            return "Answer options must be different from each other.";
        }

        var correctCount = options.Count(x => x.IsCorrect);

        if (request.IsMultipleAnswer && correctCount == 0)
        {
            return "A multiple-answer question needs at least one correct option.";
        }

        if (!request.IsMultipleAnswer && correctCount != 1)
        {
            return "A single-answer question needs exactly one correct option.";
        }

        return null;
    }
}
