using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HRM.API.Models;

// One row per question of a submitted attempt, with the question as it was at that time.
// The unique index makes a second, concurrent submission of the same attempt fail.
[Table("module_quiz_answer", Schema = "hrm")]
[Index(nameof(AttemptId), nameof(QuestionId), IsUnique = true)]
public class ModuleQuizAnswer
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("attempt_id")]
    public Guid AttemptId { get; set; }

    [ForeignKey(nameof(AttemptId))]
    public ModuleQuizAttempt? Attempt { get; set; }

    [Column("question_id")]
    public Guid QuestionId { get; set; }

    [ForeignKey(nameof(QuestionId))]
    [DeleteBehavior(DeleteBehavior.NoAction)]
    public ModuleQuizQuestion? Question { get; set; }

    [Column("question_text")]
    [MaxLength(1000)]
    public string QuestionText { get; set; } = string.Empty;

    [Column("is_multiple_answer")]
    public bool IsMultipleAnswer { get; set; }

    [Column("sort_order")]
    public int SortOrder { get; set; }

    // True when the selected options exactly match the correct options.
    [Column("is_correct")]
    public bool IsCorrect { get; set; }

    public List<ModuleQuizAnswerOption> Options { get; set; } = new();
}
