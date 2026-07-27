using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRM.API.Models;

[Table("login_otp", Schema = "hrm")]
public class LoginOtp
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("token_hash")]
    [MaxLength(200)]
    public string TokenHash { get; set; } = string.Empty;

    [Column("code_hash")]
    [MaxLength(200)]
    public string CodeHash { get; set; } = string.Empty;

    [Column("attempt_count")]
    public int AttemptCount { get; set; }

    [Column("expires_at")]
    public DateTime ExpiresAt { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("used_at")]
    public DateTime? UsedAt { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;
}
