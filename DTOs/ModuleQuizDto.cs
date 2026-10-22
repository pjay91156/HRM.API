public class SaveModuleQuizSettingsDto
{
    public int? PassingPercentage { get; set; }

    // Null means unlimited attempts.
    public int? MaxAttempts { get; set; }
}

public class UpdateModuleQuizActiveDto
{
    public bool IsActive { get; set; }
}

public class SaveModuleQuizQuestionDto
{
    public string? QuestionText { get; set; }

    public bool IsMultipleAnswer { get; set; }

    // The complete option list in display order. On update, options with an Id are
    // changed, options without one are added and existing options left out are removed.
    public List<SaveModuleQuizOptionDto>? Options { get; set; }
}

public class SaveModuleQuizOptionDto
{
    public Guid? Id { get; set; }

    public string? OptionText { get; set; }

    public bool IsCorrect { get; set; }
}

public class ReorderModuleQuizQuestionsDto
{
    // Every question of the quiz, in the order they should be displayed.
    public List<Guid>? QuestionIds { get; set; }
}

public class SubmitModuleQuizDto
{
    public List<SubmitModuleQuizAnswerDto>? Answers { get; set; }
}

public class SubmitModuleQuizAnswerDto
{
    public Guid QuestionId { get; set; }

    public List<Guid>? SelectedOptionIds { get; set; }
}
