namespace HRM.API.Responses;

public class CourseModuleResponse
{
    public Guid Id { get; set; }

    public Guid CourseId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Content { get; set; }

    public bool HasVideo { get; set; }

    public string? VideoFileName { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}

public class CourseModuleVideoStream
{
    public Stream Stream { get; set; } = Stream.Null;

    public string ContentType { get; set; } = string.Empty;
}
