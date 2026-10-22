public class CreateCourseModuleDto
{
    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? Content { get; set; }

    public IFormFile? Video { get; set; }
}

public class UpdateCourseModuleDto
{
    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? Content { get; set; }

    public IFormFile? Video { get; set; }

    public bool RemoveVideo { get; set; }
}

public class UpdateCourseModuleActiveDto
{
    public bool IsActive { get; set; }
}

public class ReorderCourseModulesDto
{
    // Every module of the course, in the order they should be displayed.
    public List<Guid>? ModuleIds { get; set; }
}

public class UploadCourseModuleVideoDto
{
    public IFormFile? Video { get; set; }
}
