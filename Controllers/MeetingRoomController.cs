using HRM.API.Extensions;
using HRM.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class MeetingRoomController : ControllerBase
{
    private readonly IMeetingRoomService _meetingRoomService;

    public MeetingRoomController(IMeetingRoomService meetingRoomService)
    {
        _meetingRoomService = meetingRoomService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? floorId)
    {
        var response = await _meetingRoomService.GetAllAsync(floorId);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var response = await _meetingRoomService.GetByIdAsync(id);

        if (!response.Success)
            return NotFound(response);

        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateMeetingRoomDto request)
    {
        var userId = User.GetUserId();

        var response = await _meetingRoomService.CreateAsync(request, userId);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpPut]
    public async Task<IActionResult> Update(UpdateMeetingRoomDto request)
    {
        var userId = User.GetUserId();

        var response = await _meetingRoomService.UpdateAsync(request, userId);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = User.GetUserId();

        var response = await _meetingRoomService.DeleteAsync(id, userId);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }
}
