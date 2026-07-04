using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRM.API.Models;

[Table("companies", Schema = "hrm")]
public class Company : BaseEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("company_name")]
    public string CompanyName { get; set; } = string.Empty;

    [Column("email")]
    public string Email { get; set; } = string.Empty;
}