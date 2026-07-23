using HRM.API.Enums;

public class CreateCompanyDocumentDto
{
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DocumentType DocumentType { get; set; }

    public IFormFile File { get; set; } = null!;
}

public class UpdateCompanyDocumentDto
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DocumentType DocumentType { get; set; }

    public IFormFile? File { get; set; }
}
