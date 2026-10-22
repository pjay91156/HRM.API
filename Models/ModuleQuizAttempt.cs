using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HRM.API.Models;

// The unique index stops concurrent requests from creating two attempts with the same number.
[Table("module_quiz_attempt", Schema = "hrm")]
[Index(nameof(QuizId), nameof(EmployeeId), nameof(AttemptNumber), IsUnique = true)]
public class ModuleQuizAttempt : BaseEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("quiz_id")]
    public Guid QuizId { get; set; }

    [ForeignKey(nameof(QuizId))]
    public ModuleQuiz? Quiz { get; set; }

    [Column("employee_id")]
    public Guid EmployeeId { get; set; }

    [ForeignKey(nameof(EmployeeId))]
    [DeleteBehavior(DeleteBehavior.NoAction)]
    public Employee? Employee { get; set; }

    [Column("attempt_number")]
    public int AttemptNumber { get; set; }

    // The values below are recorded when the attempt is submitted, so later
    // changes to the quiz do not alter a past result.
    [Column("total_questions")]
    public int TotalQuestions { get; set; }

    [Column("correct_answers")]
    public int CorrectAnswers { get; set; }

    [Column("score_percentage")]
    [Precision(5, 2)]
    public decimal ScorePercentage { get; set; }

    [Column("passing_percentage")]
    public int PassingPercentage { get; set; }

    [Column("is_passed")]
    public bool IsPassed { get; set; }

    [Column("started_at")]
    public DateTime StartedAt { get; set; }

    // Null while the attempt is still open.
    [Column("submitted_at")]
    public DateTime? SubmittedAt { get; set; }

    public List<ModuleQuizAnswer> Answers { get; set; } = new();
}
