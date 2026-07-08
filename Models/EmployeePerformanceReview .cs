using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRM.API.Models;

[Table("employee_performance_review", Schema = "hrm")]
public class EmployeePerformanceReview : BaseEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("performance_cycle_id")]
    public Guid PerformanceCycleId { get; set; }

    [Column("employee_id")]
    public Guid EmployeeId { get; set; }

    [Column("manager_id")]
    public Guid ManagerId { get; set; }

    [Column("submitted_on")]
    public DateTime? SubmittedOn { get; set; }

    [Column("manager_reviewed_on")]
    public DateTime? ManagerReviewedOn { get; set; }

    [Column("overall_employee_comment")]
    [MaxLength(2000)]
    public string? OverallEmployeeComment { get; set; }

    [Column("overall_manager_comment")]
    [MaxLength(2000)]
    public string? OverallManagerComment { get; set; }

    [Column("overall_score")]
    public decimal? OverallScore { get; set; }

    [Column("status")]
    public byte Status { get; set; }

    #region Navigation Properties

    [ForeignKey(nameof(PerformanceCycleId))]
    public virtual PerformanceCycle PerformanceCycle { get; set; } = null!;

    [ForeignKey(nameof(EmployeeId))]
    public virtual Employee Employee { get; set; } = null!;

    [ForeignKey(nameof(ManagerId))]
    public virtual Employee Manager { get; set; } = null!;

    public virtual ICollection<EmployeePerformanceSkillReview> SkillReviews { get; set; }
        = new List<EmployeePerformanceSkillReview>();

    #endregion
}