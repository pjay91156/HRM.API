public class CreateMeetingRoomBookingDto
{
    public Guid MeetingRoomId { get; set; }

    public DateOnly BookingDate { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public string Reason { get; set; } = string.Empty;

    public int? NumberOfAttendees { get; set; }
}
