using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HRM.API.Enums;

namespace HRM.API.Models;

[Table("audit_logs", Schema = "hrm")]
public class AuditLog
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("company_id")]
    public Guid CompanyId { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("user_name")]
    [MaxLength(200)]
    public string UserName { get; set; } = string.Empty;

    [Column("action")]
    public AuditAction Action { get; set; }

    [Column("module")]
    public AuditModule Module { get; set; }

    [Column("entity_type")]
    [MaxLength(100)]
    public string EntityType { get; set; } = string.Empty;

    [Column("entity_id")]
    public Guid? EntityId { get; set; }

    [Column("target_user_id")]
    public Guid? TargetUserId { get; set; }

    [Column("target_user_name")]
    [MaxLength(200)]
    public string? TargetUserName { get; set; }

    [Column("description")]
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Column("status")]
    [MaxLength(50)]
    public string? Status { get; set; }

    [Column("old_values")]
    public string? OldValues { get; set; }

    [Column("new_values")]
    public string? NewValues { get; set; }

    [Column("ip_address")]
    [MaxLength(64)]
    public string? IpAddress { get; set; }

    [Column("user_agent")]
    [MaxLength(500)]
    public string? UserAgent { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}
