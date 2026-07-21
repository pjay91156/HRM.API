namespace HRM.API.Responses;

public class MeetingRoomBookingResponse
{
    public Guid Id { get; set; }

    public Guid MeetingRoomId { get; set; }

    public string MeetingRoomName { get; set; } = string.Empty;

    public Guid FloorId { get; set; }

    public string FloorName { get; set; } = string.Empty;

    public Guid EmployeeId { get; set; }

    public string EmployeeName { get; set; } = string.Empty;

    public DateOnly BookingDate { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public string Reason { get; set; } = string.Empty;

    public int? NumberOfAttendees { get; set; }

    public string Status { get; set; } = string.Empty;

    public bool CanCancel { get; set; }
}
