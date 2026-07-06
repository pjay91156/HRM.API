using HRM.API.DTOs;
using HRM.API.Extensions;
using HRM.API.Models;
using HRM.API.Responses;
using HRM.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/attendance")]
[ApiController]
public class AttendanceController : ControllerBase
{
    private readonly IAttendanceService
        _attendanceService;

    public AttendanceController(
        IAttendanceService attendanceService)
    {
        _attendanceService =
            attendanceService;
    }

    [HttpPost("checkin")]
    public async Task<IActionResult> CheckIn(
        )
    {
        try
        {
            var userId = User.GetUserId();
            var response =
                await _attendanceService
                    .CheckInAsync(userId);

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500,
                new ApiResponse<object>
                {
                    Success = false,
                    Message = "Internal Server Error",
                    Errors = new List<string>
                    {
                        ex.Message
                    }
                });
        }
    }

    [HttpPost("checkout")]
    public async Task<IActionResult> CheckOut(
        )
    {
        try
        {
            var userId = User.GetUserId();
            var response =
                await _attendanceService
                    .CheckOutAsync(userId);

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500,
                new ApiResponse<object>
                {
                    Success = false,
                    Message = "Internal Server Error",
                    Errors = new List<string>
                    {
                        ex.Message
                    }
                });
        }
    }

    [HttpGet("today")]
    public async Task<IActionResult>
        GetTodayAttendance(DateTime attendanceDate)
    {
        try
        {
            var userId = User.GetUserId();
            var response =
                await _attendanceService
                    .GetTodayAttendanceAsync(
                        userId, attendanceDate);

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500,
                new ApiResponse<object>
                {
                    Success = false,
                    Message = "Internal Server Error",
                    Errors = new List<string>
                    {
                        ex.Message
                    }
                });
        }
    }
    [HttpGet("history/{employeeId}")]
    public async Task<IActionResult>
    GetAttendanceHistory(Guid employeeId)
    {
        try
        {
            var response =
                await _attendanceService
                    .GetAttendanceHistoryAsync(employeeId);

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500,
                new ApiResponse<object>
                {
                    Success = false,
                    Message = "Internal Server Error",
                    Errors = new List<string>
                    {
                    ex.Message
                    }
                });
        }
    }
    [HttpGet("calendar/{employeeId}")]
    public async Task<IActionResult>
        GetAttendanceCalendar(
            Guid employeeId,
            int month,
            int year)
    {
        try
        {
            var response =
                await _attendanceService
                    .GetAttendanceCalendarAsync(
                        employeeId,
                        month,
                        year);

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500,
                new ApiResponse<object>
                {
                    Success = false,
                    Message = "Internal Server Error",
                    Errors = new List<string>
                    {
                    ex.Message
                    }
                });
        }
    }
    [HttpGet("summary")]
    public async Task<IActionResult> GetAttendanceSummary(DateTime attendanceDate)
    {
        var userId = User.GetUserId();

        var result = await _attendanceService.GetAttendanceSummaryAsync(userId, attendanceDate);

        return Ok(result);
    }
    [HttpGet("weekly-summary")]
    public async Task<IActionResult> GetWeeklySummary(DateTime attendanceDate)
    {
        var userId = User.GetUserId();

        var result = await _attendanceService.GetWeeklySummaryAsync(userId, attendanceDate);

        return Ok(result);
    }
    [HttpGet("teamsummary")]
    public async Task<IActionResult> GetSummary(DateTime attendanceDate)
    {
        try
        {
            Guid userId = User.GetUserId();
            Guid companyId=User.GetCompanyId();

            var response = await _attendanceService
                .GetTeamAttendanceSummaryAsync(userId, attendanceDate,companyId);

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500,
                new ApiResponse<object>
                {
                    Success = false,
                    Message = "Internal Server Error",
                    Errors = new List<string>
                    {
                    ex.Message
                    }
                });
        }
    }
    [HttpPost("team-attendance")]
public async Task<IActionResult> GetTeamAttendance(
    [FromBody] TeamAttendanceFilterDto request)
{
    try
    {
        Guid userId = User.GetUserId();
        Guid companyId=User.GetCompanyId();

        var response = await _attendanceService
            .GetTeamAttendanceAsync(
                userId,
                request,
                companyId);

        return Ok(response);
    }
    catch (Exception ex)
    {
        return StatusCode(500,
            new ApiResponse<object>
            {
                Success = false,
                Message = "Internal Server Error",
                Errors = new List<string>
                {
                    ex.Message
                }
            });
    }
}
}