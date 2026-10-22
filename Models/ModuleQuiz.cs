using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HRM.API.Models;

// A quiz belongs either to one module (a module quiz, module_id set) or directly to
// one course (that course's final quiz, course_id set). Exactly one of the two is set,
// and each module and each course has at most one quiz.
[Table("module_quiz", Schema = "hrm")]
[Index(nameof(ModuleId), IsUnique = true)]
[Index(nameof(CourseId), IsUnique = true)]
public class ModuleQuiz : BaseEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("module_id")]
    public Guid? ModuleId { get; set; }

    [ForeignKey(nameof(ModuleId))]
    public CourseModule? Module { get; set; }

    // Set only for a final course quiz.
    [Column("course_id")]
    public Guid? CourseId { get; set; }

    [ForeignKey(nameof(CourseId))]
    [DeleteBehavior(DeleteBehavior.NoAction)]
    public Course? Course { get; set; }

    // Minimum score, 0 through 100, needed to pass.
    [Column("passing_percentage")]
    public int PassingPercentage { get; set; }

    // Null means unlimited attempts.
    [Column("max_attempts")]
    public int? MaxAttempts { get; set; }

    public List<ModuleQuizQuestion> Questions { get; set; } = new();
}
