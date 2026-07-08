using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HRM.API.Enums;

namespace HRM.API.Models;

[Table("notification", Schema = "hrm")]
public class Notification : BaseEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("title")]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Column("message")]
    [MaxLength(1000)]
    public string Message { get; set; } = string.Empty;

    [Column("type")]
    public NotificationType Type { get; set; }

    [Column("link")]
    [MaxLength(300)]
    public string? Link { get; set; }

    [Column("is_read")]
    public bool IsRead { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;
}
