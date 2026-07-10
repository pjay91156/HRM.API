namespace HRM.API.Responses;

public class PerformanceRatingResponse
{
    public Guid Id { get; set; }

    public byte Rating { get; set; }

    public string RatingName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; }
}
