using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HRM.API.Models;

// An employee's progress through one module.
[Table("module_progress", Schema = "hrm")]
[Index(nameof(ModuleId), nameof(EmployeeId), IsUnique = true)]
public class ModuleProgress : BaseEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("module_id")]
    public Guid ModuleId { get; set; }

    [ForeignKey(nameof(ModuleId))]
    public CourseModule? Module { get; set; }

    [Column("employee_id")]
    public Guid EmployeeId { get; set; }

    [ForeignKey(nameof(EmployeeId))]
    [DeleteBehavior(DeleteBehavior.NoAction)]
    public Employee? Employee { get; set; }

    // Set when the employee marks the learning content as complete.
    [Column("learning_completed_at")]
    public DateTime? LearningCompletedAt { get; set; }

    // Set the first time the employee passes the module quiz; never cleared by later quiz edits.
    [Column("quiz_passed_at")]
    public DateTime? QuizPassedAt { get; set; }
}
