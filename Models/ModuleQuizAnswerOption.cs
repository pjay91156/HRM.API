using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HRM.API.Models;

// Every option the question had when the attempt was submitted, with the answer key
// and whether the employee selected it.
[Table("module_quiz_answer_option", Schema = "hrm")]
[Index(nameof(AnswerId), nameof(OptionId), IsUnique = true)]
public class ModuleQuizAnswerOption
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("answer_id")]
    public Guid AnswerId { get; set; }

    [ForeignKey(nameof(AnswerId))]
    public ModuleQuizAnswer? Answer { get; set; }

    [Column("option_id")]
    public Guid OptionId { get; set; }

    [ForeignKey(nameof(OptionId))]
    [DeleteBehavior(DeleteBehavior.NoAction)]
    public ModuleQuizOption? Option { get; set; }

    [Column("option_text")]
    [MaxLength(500)]
    public string OptionText { get; set; } = string.Empty;

    [Column("is_correct")]
    public bool IsCorrect { get; set; }

    [Column("is_selected")]
    public bool IsSelected { get; set; }

    [Column("sort_order")]
    public int SortOrder { get; set; }
}
