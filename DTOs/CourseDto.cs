public class CreateCourseDto
{
    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? Status { get; set; }

    public IFormFile? Thumbnail { get; set; }
}

public class UpdateCourseDto
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? Status { get; set; }

    public IFormFile? Thumbnail { get; set; }

    public bool RemoveThumbnail { get; set; }
}

public class UpdateCourseActiveDto
{
    public bool IsActive { get; set; }
}
