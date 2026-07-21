using HRM.API.Enums;
using HRM.API.Extensions;
using HRM.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class MeetingRoomBookingController : ControllerBase
{
    private readonly IMeetingRoomBookingService _meetingRoomBookingService;

    public MeetingRoomBookingController(IMeetingRoomBookingService meetingRoomBookingService)
    {
        _meetingRoomBookingService = meetingRoomBookingService;
    }

    private bool IsAdmin => User.IsInRole(nameof(UserRole.Admin)) || User.IsInRole(nameof(UserRole.SuperAdmin));

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? floorId,
        [FromQuery] Guid? meetingRoomId,
        [FromQuery] DateOnly? fromDate,
        [FromQuery] DateOnly? toDate,
        [FromQuery] MeetingRoomBookingStatus? status)
    {
        var userId = User.GetUserId();

        var response = await _meetingRoomBookingService.GetAllAsync(userId, floorId, meetingRoomId, fromDate, toDate, status);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyBookings()
    {
        var userId = User.GetUserId();

        var response = await _meetingRoomBookingService.GetMyBookingsAsync(userId);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateMeetingRoomBookingDto request)
    {
        var userId = User.GetUserId();

        var response = await _meetingRoomBookingService.CreateAsync(request, userId);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpPut("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var userId = User.GetUserId();

        var response = await _meetingRoomBookingService.CancelAsync(id, userId, IsAdmin);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }
}
