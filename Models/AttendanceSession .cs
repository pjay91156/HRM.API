using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRM.API.Models;
[Table("attendance_session", Schema = "hrm")]
public class AttendanceSession : BaseEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("attendance_id")]
    public Guid AttendanceId { get; set; }

    [Column("session_number")]
    public int SessionNumber { get; set; }

    [Column("check_in_time")]
    public DateTime CheckInTime { get; set; }

    [Column("check_out_time")]
    public DateTime? CheckOutTime { get; set; }

    [Column("working_hours")]
    public decimal WorkingHours { get; set; }

    [Column("status")]
    public string Status { get; set; } = string.Empty;

    [ForeignKey(nameof(AttendanceId))]
    public virtual Attendance Attendance { get; set; } = null!;
}