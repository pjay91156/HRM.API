using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HRM.API.Models;

[Table("course_module", Schema = "hrm")]
[Index(nameof(CourseId), nameof(SortOrder))]
public class CourseModule : BaseEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("course_id")]
    public Guid CourseId { get; set; }

    [ForeignKey(nameof(CourseId))]
    public Course? Course { get; set; }

    [Column("title")]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Column("description")]
    [MaxLength(2000)]
    public string? Description { get; set; }

    // Text-based learning content.
    [Column("content")]
    public string? Content { get; set; }

    // Path relative to the private upload root,
    // e.g. {companyId}/courses/{courseId}/modules/{moduleId}/video_....mp4
    [Column("video_path")]
    [MaxLength(500)]
    public string? VideoPath { get; set; }

    [Column("sort_order")]
    public int SortOrder { get; set; }
}
