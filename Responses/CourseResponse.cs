namespace HRM.API.Responses;

public class CourseResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string? Thumbnail { get; set; }

    public string Status { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}

public class CourseThumbnailFile
{
    public byte[] Content { get; set; } = Array.Empty<byte>();

    public string ContentType { get; set; } = string.Empty;
}
