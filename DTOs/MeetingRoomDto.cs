public class CreateMeetingRoomDto
{
    public Guid FloorId { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Capacity { get; set; }

    public string? Description { get; set; }

    public List<Guid> AmenityIds { get; set; } = new();
}

public class UpdateMeetingRoomDto
{
    public Guid Id { get; set; }

    public Guid FloorId { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Capacity { get; set; }

    public string? Description { get; set; }

    public List<Guid> AmenityIds { get; set; } = new();
}
