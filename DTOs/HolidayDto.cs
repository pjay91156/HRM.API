using HRM.API.Enums;

public class HolidayDto
{
    public string Name { get; set; } = string.Empty;

    public DateTime HolidayDate { get; set; }

    public HolidayType HolidayType { get; set; }

    public string Location { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public IFormFile? Image { get; set; }
}

public class UpdateHolidayDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateTime HolidayDate { get; set; }

    public HolidayType HolidayType { get; set; }

    public string Location { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public IFormFile? Image { get; set; }
}
