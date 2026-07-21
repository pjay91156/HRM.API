using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HRM.API.Enums;

namespace HRM.API.Models;

[Table("meeting_room_booking", Schema = "hrm")]
public class MeetingRoomBooking : BaseEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("meeting_room_id")]
    public Guid MeetingRoomId { get; set; }

    [Column("employee_id")]
    public Guid EmployeeId { get; set; }

    [Column("booking_date")]
    public DateOnly BookingDate { get; set; }

    [Column("start_time")]
    public TimeOnly StartTime { get; set; }

    [Column("end_time")]
    public TimeOnly EndTime { get; set; }

    [Column("reason")]
    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;

    [Column("number_of_attendees")]
    public int? NumberOfAttendees { get; set; }

    [Column("status")]
    public MeetingRoomBookingStatus Status { get; set; }

    [ForeignKey(nameof(MeetingRoomId))]
    public virtual MeetingRoom MeetingRoom { get; set; } = null!;

    [ForeignKey(nameof(EmployeeId))]
    public virtual Employee Employee { get; set; } = null!;
}
