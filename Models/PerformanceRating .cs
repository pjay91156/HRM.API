using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRM.API.Models;

[Table("performance_rating", Schema = "hrm")]
public class PerformanceRating : BaseEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("rating")]
    public byte Rating { get; set; }

    [Column("rating_name")]
    [MaxLength(50)]
    public string RatingName { get; set; } = string.Empty;

    [Column("description")]
    [MaxLength(500)]
    public string? Description { get; set; }

    [Column("display_order")]
    public int DisplayOrder { get; set; }
}