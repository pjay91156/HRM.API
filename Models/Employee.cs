using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRM.API.Models;

[Table("employee", Schema = "hrm")]
public class Employee : BaseEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("department_id")]
    public Guid DepartmentId { get; set; }

    [Column("designation_id")]
    public Guid DesignationId { get; set; }

    [Column("employee_code")]
    [MaxLength(20)]
    public string EmployeeCode { get; set; } = string.Empty;

    [Column("joining_date")]
    public DateOnly JoiningDate { get; set; }
      [Column("manager_id")]
    public Guid? ManagerId { get; set; }

    [Column("phone_number")]
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }  

    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;

    [ForeignKey(nameof(DepartmentId))]
    public virtual Department Department { get; set; } = null!;

    [ForeignKey(nameof(DesignationId))]
    public virtual Designation Designation { get; set; } = null!;
     [ForeignKey(nameof(ManagerId))]
    public virtual Employee? Manager { get; set; }
}