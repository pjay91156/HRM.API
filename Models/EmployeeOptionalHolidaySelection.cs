using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRM.API.Models;

[Table("employee_optional_holiday_selection", Schema = "hrm")]
public class EmployeeOptionalHolidaySelection : BaseEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("employee_id")]
    public Guid EmployeeId { get; set; }

    [Column("holiday_id")]
    public Guid HolidayId { get; set; }

    [Column("year")]
    public int Year { get; set; }
}
