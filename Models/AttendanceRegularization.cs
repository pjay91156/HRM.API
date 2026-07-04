 
 
 using System.ComponentModel.DataAnnotations;
 namespace HRM.API.Models;
using System.ComponentModel.DataAnnotations.Schema;[Table("attendance_regularization", Schema = "hrm")]
    public class AttendanceRegularization
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("attendance_id")]
        public Guid AttendanceId { get; set; }

        [Column("attendance_session_id")]
        public Guid? SessionId { get; set; }

        [Required]
        [Column("change_type")]
        [MaxLength(20)]
        public int ChangeType { get; set; } 

        [Column("after_check_in")]
        public TimeOnly? AfterCheckIn { get; set; }

        [Column("after_check_out")]
        public TimeOnly? AfterCheckOut { get; set; }

        [Required]
        [Column("reason")]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;

        [Column("status")]
        public int Status { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("created_by")]
        public Guid CreatedBy { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [Column("updated_by")]
        public Guid? UpdatedBy { get; set; }

        #region Navigation Properties

        [ForeignKey(nameof(AttendanceId))]
        public virtual Attendance Attendance { get; set; } = null!;

        [ForeignKey(nameof(SessionId))]
        public virtual AttendanceSession? AttendanceSession { get; set; }

        [ForeignKey(nameof(CreatedBy))]
        public virtual User CreatedByUser { get; set; } = null!;

        [ForeignKey(nameof(UpdatedBy))]
        public virtual User? UpdatedByUser { get; set; }

        #endregion
    }