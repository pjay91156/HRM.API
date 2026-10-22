namespace HRM.API.Responses;

public class LearningCourseResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string? Thumbnail { get; set; }

    public int ModuleCount { get; set; }

    public int CompletedModuleCount { get; set; }

    // NotStarted, InProgress, FinalQuizAvailable or Completed.
    public string Status { get; set; } = string.Empty;

    public DateTime? StartedAt { get; set; }

    // Set once the final course quiz has been passed.
    public DateTime? CompletedAt { get; set; }
}

public class LearningCourseDetailResponse : LearningCourseResponse
{
    public List<LearningModuleSummaryResponse> Modules { get; set; } = new();

    public LearningFinalQuizResponse FinalQuiz { get; set; } = new();
}

public class LearningFinalQuizResponse
{
    // Unavailable (no active final quiz), Locked (modules remain), Available,
    // NoAttemptsLeft or Passed.
    public string State { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    // Null while the final quiz is unavailable or locked.
    public LearningQuizSummaryResponse? Quiz { get; set; }
}

public class LearningModuleSummaryResponse
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool HasVideo { get; set; }

    public bool HasText { get; set; }

    public bool HasQuiz { get; set; }

    public bool LearningCompleted { get; set; }

    public bool QuizPassed { get; set; }

    // None (module has no quiz), NotAttempted, Failed or Passed.
    public string QuizStatus { get; set; } = string.Empty;

    // Learning content marked complete and, when the module has a quiz, the quiz passed.
    public bool IsCompleted { get; set; }
}

public class LearningModuleResponse : LearningModuleSummaryResponse
{
    public Guid CourseId { get; set; }

    public string CourseName { get; set; } = string.Empty;

    public string? Content { get; set; }

    // Null when the module has no quiz or the learning content is not completed yet.
    public LearningQuizSummaryResponse? Quiz { get; set; }
}

public class LearningQuizSummaryResponse
{
    public int QuestionCount { get; set; }

    public int PassingPercentage { get; set; }

    public int? MaxAttempts { get; set; }

    public int AttemptsUsed { get; set; }

    // Null when attempts are unlimited.
    public int? AttemptsRemaining { get; set; }

    public bool IsPassed { get; set; }

    public bool HasOpenAttempt { get; set; }

    public bool CanStartAttempt { get; set; }
}

public class QuizAttemptForTakingResponse
{
    public Guid AttemptId { get; set; }

    public int AttemptNumber { get; set; }

    public int PassingPercentage { get; set; }

    public DateTime StartedAt { get; set; }

    public List<QuizQuestionForTakingResponse> Questions { get; set; } = new();
}

public class QuizAttemptSummaryResponse
{
    public Guid AttemptId { get; set; }

    public int AttemptNumber { get; set; }

    public bool IsSubmitted { get; set; }

    public int TotalQuestions { get; set; }

    public int CorrectAnswers { get; set; }

    public decimal ScorePercentage { get; set; }

    public int PassingPercentage { get; set; }

    public bool IsPassed { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime? SubmittedAt { get; set; }
}

public class QuizAttemptResultResponse : QuizAttemptSummaryResponse
{
    // Null when attempts are unlimited.
    public int? AttemptsRemaining { get; set; }

    public bool CanRetry { get; set; }

    // The answer key is included only once the employee has passed the quiz.
    public bool CorrectAnswersRevealed { get; set; }

    // For a final course quiz: when the course was completed. Null for module quizzes
    // and while the course is not completed.
    public DateTime? CourseCompletedAt { get; set; }

    public List<QuizAttemptAnswerResponse> Answers { get; set; } = new();
}

public class QuizAttemptAnswerResponse
{
    public string QuestionText { get; set; } = string.Empty;

    public bool IsMultipleAnswer { get; set; }

    public bool IsCorrect { get; set; }

    public List<QuizAttemptAnswerOptionResponse> Options { get; set; } = new();
}

public class QuizAttemptAnswerOptionResponse
{
    public string OptionText { get; set; } = string.Empty;

    public bool IsSelected { get; set; }

    // Null unless the answer key is revealed.
    public bool? IsCorrect { get; set; }
}
