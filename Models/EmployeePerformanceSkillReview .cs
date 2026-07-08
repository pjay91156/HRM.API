using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRM.API.Models;

[Table("employee_performance_skill_review", Schema = "hrm")]
public class EmployeePerformanceSkillReview : BaseEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("employee_performance_review_id")]
    public Guid EmployeePerformanceReviewId { get; set; }

    [Column("performance_skill_id")]
    public Guid PerformanceSkillId { get; set; }

    [Column("employee_rating")]
    public decimal? EmployeeRating { get; set; }

    [Column("employee_comment")]
    [MaxLength(1000)]
    public string? EmployeeComment { get; set; }

    [Column("manager_rating")]
    public decimal? ManagerRating { get; set; }

    [Column("manager_comment")]
    [MaxLength(1000)]
    public string? ManagerComment { get; set; }

    #region Navigation Properties

    [ForeignKey(nameof(EmployeePerformanceReviewId))]
    public virtual EmployeePerformanceReview EmployeePerformanceReview { get; set; } = null!;

    [ForeignKey(nameof(PerformanceSkillId))]
    public virtual PerformanceSkill PerformanceSkill { get; set; } = null!;

    #endregion
}