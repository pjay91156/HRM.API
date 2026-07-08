using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRM.API.Models;

[Table("performance_category", Schema = "hrm")]
public class PerformanceCategory : BaseEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("performance_template_id")]
    public Guid PerformanceTemplateId { get; set; }

    [Column("category_name")]
    [MaxLength(100)]
    public string CategoryName { get; set; } = string.Empty;

    [Column("weightage")]
    public decimal Weightage { get; set; }

    [Column("display_order")]
    public int DisplayOrder { get; set; }

    #region Navigation Properties

    [ForeignKey(nameof(PerformanceTemplateId))]
    public virtual PerformanceTemplate PerformanceTemplate { get; set; } = null!;

    public virtual ICollection<PerformanceSkill> PerformanceSkills { get; set; }
        = new List<PerformanceSkill>();

    #endregion
}