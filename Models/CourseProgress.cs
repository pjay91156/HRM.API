using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HRM.API.Models;

// An employee's progress through one course. The unique index guarantees a single
// row, and therefore a single completion, per employee and course.
[Table("course_progress", Schema = "hrm")]
[Index(nameof(CourseId), nameof(EmployeeId), IsUnique = true)]
public class CourseProgress : BaseEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("course_id")]
    public Guid CourseId { get; set; }

    [ForeignKey(nameof(CourseId))]
    public Course? Course { get; set; }

    [Column("employee_id")]
    public Guid EmployeeId { get; set; }

    [ForeignKey(nameof(EmployeeId))]
    [DeleteBehavior(DeleteBehavior.NoAction)]
    public Employee? Employee { get; set; }

    [Column("started_at")]
    public DateTime StartedAt { get; set; }

    // Set once, when the employee passes the final course quiz; never cleared by later
    // changes to the course, its modules or its quizzes.
    [Column("completed_at")]
    public DateTime? CompletedAt { get; set; }
}
