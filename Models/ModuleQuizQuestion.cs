using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HRM.API.Models;

// Deleted questions are kept with is_active = false so past attempts can still reference them.
[Table("module_quiz_question", Schema = "hrm")]
[Index(nameof(QuizId), nameof(SortOrder))]
public class ModuleQuizQuestion : BaseEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("quiz_id")]
    public Guid QuizId { get; set; }

    [ForeignKey(nameof(QuizId))]
    public ModuleQuiz? Quiz { get; set; }

    [Column("question_text")]
    [MaxLength(1000)]
    public string QuestionText { get; set; } = string.Empty;

    // false: exactly one correct option; true: one or more correct options.
    [Column("is_multiple_answer")]
    public bool IsMultipleAnswer { get; set; }

    [Column("sort_order")]
    public int SortOrder { get; set; }

    public List<ModuleQuizOption> Options { get; set; } = new();
}
