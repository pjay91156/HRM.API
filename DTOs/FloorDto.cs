public class CreateFloorDto
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}

public class UpdateFloorDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}
