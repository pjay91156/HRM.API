using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRM.API.Models;

[Table("performance_cycle", Schema = "hrm")]
public class PerformanceCycle : BaseEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("cycle_name")]
    [MaxLength(100)]
    public string CycleName { get; set; } = string.Empty;

    [Column("review_period_start")]
    public DateOnly ReviewPeriodStart { get; set; }

    [Column("review_period_end")]
    public DateOnly ReviewPeriodEnd { get; set; }

    [Column("employee_review_start")]
    public DateOnly EmployeeReviewStart { get; set; }

    [Column("employee_review_end")]
    public DateOnly EmployeeReviewEnd { get; set; }

    [Column("manager_review_start")]
    public DateOnly ManagerReviewStart { get; set; }

    [Column("manager_review_end")]
    public DateOnly ManagerReviewEnd { get; set; }

    [Column("status")]
    public byte Status { get; set; }

    #region Navigation Properties

    public virtual ICollection<EmployeePerformanceReview> EmployeePerformanceReviews { get; set; }
        = new List<EmployeePerformanceReview>();

    #endregion
}