public class SkillRatingDto
{
    public Guid SkillId { get; set; }

    public decimal? Rating { get; set; }

    public string? Comment { get; set; }
}

public class SubmitEmployeePerformanceReviewDto
{
    public List<SkillRatingDto> SkillRatings { get; set; } = new();

    public string? OverallComment { get; set; }

    public bool IsDraft { get; set; }
}

public class SubmitManagerPerformanceReviewDto
{
    public List<SkillRatingDto> SkillRatings { get; set; } = new();

    public string? OverallComment { get; set; }

    public bool IsDraft { get; set; }
}
