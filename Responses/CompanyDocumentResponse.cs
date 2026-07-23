namespace HRM.API.Responses;

public class CompanyDocumentResponse
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int DocumentType { get; set; }

    public string DocumentTypeName { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public string FileExtension { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class CompanyDocumentDownloadInfo
{
    public string AbsolutePath { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;
}
