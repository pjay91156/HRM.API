using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRM.API.Models;

[Table("designation", Schema = "hrm")]
public class Designation : BaseEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("department_id")]
    public Guid DepartmentId { get; set; }

    [Column("designation_name")]
    [MaxLength(100)]
    public string DesignationName { get; set; } = string.Empty;

    [ForeignKey(nameof(DepartmentId))]
    public virtual Department Department { get; set; } = null!;

    public virtual ICollection<Employee> Employees { get; set; }
        = new List<Employee>();
}