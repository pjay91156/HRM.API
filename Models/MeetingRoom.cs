using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRM.API.Models;

[Table("meeting_room", Schema = "hrm")]
public class MeetingRoom : BaseEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("floor_id")]
    public Guid FloorId { get; set; }

    [Column("name")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Column("capacity")]
    public int Capacity { get; set; }

    [Column("description")]
    [MaxLength(500)]
    public string? Description { get; set; }

    [ForeignKey(nameof(FloorId))]
    public virtual Floor Floor { get; set; } = null!;

    public virtual ICollection<MeetingRoomAmenityDetail> MeetingRoomAmenityDetails { get; set; }
        = new List<MeetingRoomAmenityDetail>();
}
