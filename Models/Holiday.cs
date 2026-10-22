using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HRM.API.Enums;

namespace HRM.API.Models;

[Table("holiday", Schema = "hrm")]
public class Holiday : BaseEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("holiday_date")]
    public DateTime HolidayDate { get; set; }

    [Column("holiday_type")]
    public HolidayType HolidayType { get; set; }

    [Column("location")]
    public string Location { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("image_url")]
    public string? ImageUrl { get; set; }
}
