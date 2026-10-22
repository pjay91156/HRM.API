namespace HRM.API.Responses;

public class HolidayResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateTime HolidayDate { get; set; }

    public int HolidayType { get; set; }

    public string HolidayTypeName { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
