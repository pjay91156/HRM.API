namespace HRM.API.Responses;

// HR/Admin view of a quiz, including the answer key.
public class ModuleQuizResponse
{
    public Guid Id { get; set; }

    // Null for a final course quiz.
    public Guid? ModuleId { get; set; }

    // Set only for a final course quiz.
    public Guid? CourseId { get; set; }

    public int PassingPercentage { get; set; }

    public int? MaxAttempts { get; set; }

    public bool IsActive { get; set; }

    public List<ModuleQuizQuestionResponse> Questions { get; set; } = new();
}

public class ModuleQuizQuestionResponse
{
    public Guid Id { get; set; }

    public string QuestionText { get; set; } = string.Empty;

    public bool IsMultipleAnswer { get; set; }

    public int SortOrder { get; set; }

    public List<ModuleQuizOptionResponse> Options { get; set; } = new();
}

public class ModuleQuizOptionResponse
{
    public Guid Id { get; set; }

    public string OptionText { get; set; } = string.Empty;

    public bool IsCorrect { get; set; }

    public int SortOrder { get; set; }
}

// What a quiz taker sees: questions and options without the answer key.
public class QuizQuestionForTakingResponse
{
    public Guid Id { get; set; }

    public string QuestionText { get; set; } = string.Empty;

    public bool IsMultipleAnswer { get; set; }

    public List<QuizOptionForTakingResponse> Options { get; set; } = new();
}

public class QuizOptionForTakingResponse
{
    public Guid Id { get; set; }

    public string OptionText { get; set; } = string.Empty;
}

// An employee who has completed a course, for the HR/Admin view.
public class CourseCompletionResponse
{
    public Guid EmployeeId { get; set; }

    public string EmployeeCode { get; set; } = string.Empty;

    public string EmployeeName { get; set; } = string.Empty;

    public DateTime StartedAt { get; set; }

    public DateTime CompletedAt { get; set; }
}
