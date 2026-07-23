using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HRM.API.Enums;

namespace HRM.API.Models;

[Table("company_document", Schema = "hrm")]
public class CompanyDocument : BaseEntity
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("title")]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Column("description")]
    [MaxLength(1000)]
    public string? Description { get; set; }

    [Column("document_type")]
    public DocumentType DocumentType { get; set; }

    [Column("file_name")]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Column("file_path")]
    [MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;

    [Column("file_extension")]
    [MaxLength(20)]
    public string FileExtension { get; set; } = string.Empty;

    [Column("content_type")]
    [MaxLength(100)]
    public string ContentType { get; set; } = string.Empty;

    [Column("file_size")]
    public long FileSize { get; set; }
}
