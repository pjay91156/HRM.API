namespace HRM.API.DTOs;

public class RegularizationApprovalRequestDto
{
    public Guid AttendanceId { get; set; }

    public Guid EmployeeId { get; set; }

    public int Status { get; set; }

    public string? Comments { get; set; }
}

public class RegularizationDetailItemDto
{
    public string ChangeType { get; set; } = string.Empty;

    public int? SessionNumber { get; set; }

    public bool IsNewSession { get; set; }

    public string Current { get; set; } = string.Empty;

    public string Requested { get; set; } = string.Empty;

    public string ChangeDescription { get; set; } = string.Empty;
}

public class ExistingSessionDto
{
    public int SessionNumber { get; set; }

    public string CheckIn { get; set; } = string.Empty;

    public string CheckOut { get; set; } = string.Empty;

    public string WorkingTime { get; set; } = string.Empty;
}

public class RegularizationDetailsResponseDto
{
    public Guid AttendanceId { get; set; }

    public Guid EmployeeId { get; set; }

    public string EmployeeCode { get; set; } = string.Empty;

    public string EmployeeName { get; set; } = string.Empty;

    public string DepartmentName { get; set; } = string.Empty;

    public DateOnly AttendanceDate { get; set; }

    public DateTime RequestedOn { get; set; }

    public string Reason { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string CurrentWorkingTime { get; set; } = string.Empty;

    public string ProposedWorkingTime { get; set; } = string.Empty;

    public List<ExistingSessionDto> ExistingSessions { get; set; } = new();

    public List<RegularizationDetailItemDto> Changes { get; set; } = new();
}
