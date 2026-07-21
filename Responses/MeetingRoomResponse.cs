namespace HRM.API.Responses;

public class MeetingRoomResponse
{
    public Guid Id { get; set; }

    public Guid FloorId { get; set; }

    public string FloorName { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public int Capacity { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public List<MeetingRoomAmenityResponse> Amenities { get; set; } = new();
}
