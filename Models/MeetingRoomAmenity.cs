using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRM.API.Models;

[Table("meeting_room_amenity", Schema = "hrm")]
public class MeetingRoomAmenity : BaseEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("name")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Column("description")]
    [MaxLength(500)]
    public string? Description { get; set; }

    public virtual ICollection<MeetingRoomAmenityDetail> MeetingRoomAmenityDetails { get; set; }
        = new List<MeetingRoomAmenityDetail>();
}
