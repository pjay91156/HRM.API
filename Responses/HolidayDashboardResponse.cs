namespace HRM.API.Responses;

public class HolidayDashboardResponse
{
    public int TotalHolidays { get; set; }

    public int PublicHolidayCount { get; set; }

    public int OptionalHolidayCount { get; set; }

    public int CompanyHolidayCount { get; set; }

    public List<HolidayResponse> UpcomingHolidays { get; set; } = new();
}
