
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRM.API.Models;
[Table("attendance", Schema = "hrm")]
public class Attendance : BaseEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("employee_id")]
    public Guid EmployeeId { get; set; }

    [Column("attendance_date")]
    public DateTime AttendanceDate { get; set; }

    [Column("total_working_hours")]
    public decimal TotalWorkingHours { get; set; }

    [ForeignKey(nameof(EmployeeId))]
    public virtual Employee Employee { get; set; } = null!;

    public virtual ICollection<AttendanceSession> Sessions
        { get; set; } = new List<AttendanceSession>();
}