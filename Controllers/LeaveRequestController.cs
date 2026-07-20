using HRM.API.DTOs;
using HRM.API.Services;
using HRM.API.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HRM.API.Extensions;

namespace HRM.API.Controllers;
// [Authorize]
[Route("api/[controller]")]
[ApiController]
public class LeaveRequestController : ControllerBase
{
    private readonly ILeaveRequestService _leaveRequestService;

    public LeaveRequestController(
        ILeaveRequestService leaveRequestService)
    {
        _leaveRequestService = leaveRequestService;
    }

    [HttpPost]
    public async Task<IActionResult> ApplyLeave(
        [FromBody] ApplyLeaveRequestDto request)
    {
        try
        {
            var userId = User.GetUserId();
            var response =
                await _leaveRequestService.ApplyLeaveAsync(request, userId);

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500,
                new ApiResponse<string>
                {
                    Success = false,
                    Message = "An error occurred while applying leave.",
                    Errors = new List<string> { ex.Message }
                });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetLeaves()
    {
        try
        {
            var response =
                await _leaveRequestService.GetLeavesAsync();

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500,
                new ApiResponse<string>
                {
                    Success = false,
                    Message = "An error occurred while retrieving leave requests.",
                    Errors = new List<string> { ex.Message }
                });
        }
    }

    [HttpGet("myLeaves")]
    public async Task<IActionResult> GetEmployeeLeaves(
        Guid employeeId)
    {
        try
        {
            var userId = User.GetUserId();
            var response =
                await _leaveRequestService.GetEmployeeLeavesAsync(userId);

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500,
                new ApiResponse<string>
                {
                    Success = false,
                    Message = "An error occurred while retrieving employee leave requests.",
                    Errors = new List<string> { ex.Message }
                });
        }
    }
    [HttpGet("myteamleaverequests")]
    public async Task<IActionResult> GetTeamLeaveRequests()
    {
        try
        {
            var userId = User.GetUserId();
            var response =
                await _leaveRequestService.GetTeamLeaveRequestsAsync(userId);

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
                    Message = "An unexpected error occurred.",
                    Errors = new List<string>
                    {
                    ex.Message
                    }
                });
        }
    }

    [HttpPut("approve-reject")]
    public async Task<IActionResult> ApproveRejectLeave(
        [FromBody] LeaveApprovalRequestDto request)
    {
        try
        {
            // Replace this with JWT User Id later
            var approverId = User.GetUserId();

            var response =
                await _leaveRequestService.ApproveRejectLeaveAsync(
                    approverId,
                    request);

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500,
                new ApiResponse<string>
                {
                    Success = false,
                    Message = "An error occurred while updating leave request.",
                    Errors = new List<string> { ex.Message }
                });
        }
    }

    [HttpPut("cancel/{leaveRequestId}")]
    public async Task<IActionResult> CancelLeave(
        Guid leaveRequestId)
    {
        try
        {
            // Replace this with JWT User Id later
            var updatedBy = Guid.NewGuid();

            var response =
                await _leaveRequestService.CancelLeaveAsync(
                    leaveRequestId,
                    updatedBy);

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500,
                new ApiResponse<string>
                {
                    Success = false,
                    Message = "An error occurred while cancelling leave request.",
                    Errors = new List<string> { ex.Message }
                });
        }
    }
    [HttpGet("balance")]
    public async Task<IActionResult> GetLeaveBalance()
    {
        try
        {
            var userId = User.GetUserId();

            var response =
                await _leaveRequestService.GetLeaveBalanceAsync(userId);

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500,
                new ApiResponse<string>
                {
                    Success = false,
                    Message = "An error occurred while retrieving leave balance.",
                    Errors = new List<string> { ex.Message }
                });
        }
    }

    [HttpGet("team-calendar")]
    public async Task<IActionResult>
    GetTeamLeaveCalendar()
    {
        try
        {
            var userId = User.GetUserId();

            var response =
                await _leaveRequestService
                    .GetTeamLeaveCalendarAsync(userId);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ApiResponse<object>
                {
                    Success = false,
                    Message =
                        "An unexpected error occurred.",

                    Errors = new List<string>
                    {
                    ex.Message
                    }
                });
        }
    }
}