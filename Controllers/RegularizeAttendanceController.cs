using HRM.API.Extensions;
using HRM.API.Responses;
using HRM.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace HRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RegularizeAttendanceController : ControllerBase
{
    private readonly IRegularizeAttendanceService _regularizeAttendanceService;

    public RegularizeAttendanceController(
        IRegularizeAttendanceService regularizeAttendanceService)
    {
        _regularizeAttendanceService = regularizeAttendanceService;
    }

    /// <summary>
    /// Get attendance sessions for a specific employee and date.
    /// </summary>
    /// <param name="employeeId"></param>
    /// <param name="attendanceDate"></param>
    /// <returns></returns>
    [HttpGet("sessions")]
    [ProducesResponseType(typeof(ApiResponse<AttendanceSessionsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AttendanceSessionsResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<AttendanceSessionsResponse>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAttendanceSessions(      
        [FromQuery] DateOnly attendanceDate)
    {
        var userId=User.GetUserId();
        var response = await _regularizeAttendanceService.GetAttendanceSessionsAsync(
            userId,
            attendanceDate);

        if (!response.Success)
        {
            return NotFound(response);
        }

        return Ok(response);
    }
     [HttpPost("regularization")]
        public async Task<IActionResult> CreateRegularizationRequest(
            [FromBody] RegularizationRequest request)
        {
            try
            {
                var userId = User.GetUserId();

                var response = await _regularizeAttendanceService
                    .CreateRegularizationRequestAsync(
                        request,
                        userId);

                if (!response.Success)
                {
                    return BadRequest(response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<object>
                    {
                        Success = false,
                        Message = ex.Message
                    });
            }
        }
   

[HttpGet("manager/pending")]
public async Task<IActionResult> GetPendingRequests()
{
    var userId = User.GetUserId();

    var response = await _regularizeAttendanceService
        .GetPendingRequestsAsync(userId);

    if (!response.Success)
        return BadRequest(response);

    return Ok(response);
}
}