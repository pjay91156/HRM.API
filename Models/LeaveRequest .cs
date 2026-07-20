using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HRM.API.Enums;

namespace HRM.API.Models;

[Table("leave_request", Schema = "hrm")]
public class LeaveRequest : BaseEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("employee_id")]
    public Guid EmployeeId { get; set; }

    [Column("leave_type_id")]
    public Guid LeaveTypeId { get; set; }

    [Column("from_date")]
    public DateTime FromDate { get; set; }

    [Column("to_date")]
    public DateTime ToDate { get; set; }

    [Column("leave_duration")]
    public LeaveDuration LeaveDuration { get; set; }

    [Column("half_day_period")]
    public HalfDayPeriod? HalfDayPeriod { get; set; }

    [Column("total_days")]
    public decimal TotalDays { get; set; }

    [Column("reason")]
    public string Reason { get; set; } = string.Empty;

    [Column("status")]
    public LeaveStatus Status { get; set; }

    [Column("approved_by")]
    public Guid? ApprovedBy { get; set; }

    [Column("approved_at")]
    public DateTime? ApprovedAt { get; set; }

    [Column("approver_comments")]
    public string? ApproverComments { get; set; }
}