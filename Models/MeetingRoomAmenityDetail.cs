using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRM.API.Models;

[Table("meeting_room_amenity_detail", Schema = "hrm")]
public class MeetingRoomAmenityDetail : BaseEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("meeting_room_id")]
    public Guid MeetingRoomId { get; set; }

    [Column("meeting_room_amenity_id")]
    public Guid MeetingRoomAmenityId { get; set; }

    [ForeignKey(nameof(MeetingRoomId))]
    public virtual MeetingRoom MeetingRoom { get; set; } = null!;

    [ForeignKey(nameof(MeetingRoomAmenityId))]
    public virtual MeetingRoomAmenity MeetingRoomAmenity { get; set; } = null!;
}
