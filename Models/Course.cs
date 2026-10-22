using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRM.API.Models;

[Table("course", Schema = "hrm")]
public class Course : BaseEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("company_id")]
    public Guid CompanyId { get; set; }

    [ForeignKey(nameof(CompanyId))]
    public Company? Company { get; set; }

    [Column("name")]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Column("description")]
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    // Path relative to the private upload root, e.g. {companyId}/courses/{courseId}/thumbnail_....png
    [Column("thumbnail")]
    [MaxLength(500)]
    public string? Thumbnail { get; set; }

    // Stored as the CourseStatus name: Draft, Published or Archived.
    [Column("status")]
    [MaxLength(20)]
    public string Status { get; set; } = string.Empty;
}
