using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRM.API.Models;

[Table("performance_skill", Schema = "hrm")]
public class PerformanceSkill : BaseEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("performance_category_id")]
    public Guid PerformanceCategoryId { get; set; }

    [Column("skill_name")]
    [MaxLength(150)]
    public string SkillName { get; set; } = string.Empty;

    [Column("description")]
    [MaxLength(500)]
    public string? Description { get; set; }

    [Column("weightage")]
    public decimal Weightage { get; set; }

    [Column("display_order")]
    public int DisplayOrder { get; set; }

    #region Navigation Properties

    [ForeignKey(nameof(PerformanceCategoryId))]
    public virtual PerformanceCategory PerformanceCategory { get; set; } = null!;

    public virtual ICollection<EmployeePerformanceSkillReview> EmployeePerformanceSkillReviews { get; set; }
        = new List<EmployeePerformanceSkillReview>();

    #endregion
}