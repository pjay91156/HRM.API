public class LeaveTypeDto
{
    public string LeaveName { get; set; } = string.Empty;

    public string LeaveCode { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal DefaultDays { get; set; }
}