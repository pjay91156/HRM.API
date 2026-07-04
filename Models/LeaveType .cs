using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRM.API.Models;
[Table("leave_type", Schema = "hrm")]
public class LeaveType : BaseEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("leave_name")]
    public string LeaveName { get; set; } = string.Empty;

    [Column("leave_code")]
    public string LeaveCode { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("default_days")]
    public decimal DefaultDays { get; set; }
    
}