using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRM.API.Models;

[Table("performance_template", Schema = "hrm")]
public class PerformanceTemplate : BaseEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("template_name")]
    [MaxLength(100)]
    public string TemplateName { get; set; } = string.Empty;

    [Column("description")]
    [MaxLength(500)]
    public string? Description { get; set; }

    [Column("department_id")]
    public Guid DepartmentId { get; set; }

    #region Navigation Properties

    [ForeignKey(nameof(DepartmentId))]
    public virtual Department Department { get; set; } = null!;

    public virtual ICollection<PerformanceCategory> PerformanceCategories { get; set; }
        = new List<PerformanceCategory>();

    #endregion
}