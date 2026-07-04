using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRM.API.Models;

[Table("department", Schema = "hrm")]
public class Department : BaseEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("department_name")]
    [MaxLength(100)]
    public string DepartmentName { get; set; } = string.Empty;
    [Column("company_id")]
    public Guid CompanyId { get; set; }

    public virtual ICollection<Designation> Designations { get; set; }
        = new List<Designation>();

    public virtual ICollection<Employee> Employees { get; set; }
        = new List<Employee>();
}