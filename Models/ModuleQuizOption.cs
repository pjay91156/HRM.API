using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HRM.API.Models;

// Deleted options are kept with is_active = false so past attempts can still reference them.
[Table("module_quiz_option", Schema = "hrm")]
[Index(nameof(QuestionId), nameof(SortOrder))]
public class ModuleQuizOption : BaseEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("question_id")]
    public Guid QuestionId { get; set; }

    [ForeignKey(nameof(QuestionId))]
    public ModuleQuizQuestion? Question { get; set; }

    [Column("option_text")]
    [MaxLength(500)]
    public string OptionText { get; set; } = string.Empty;

    [Column("is_correct")]
    public bool IsCorrect { get; set; }

    [Column("sort_order")]
    public int SortOrder { get; set; }
}
