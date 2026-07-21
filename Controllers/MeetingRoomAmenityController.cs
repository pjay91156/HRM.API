using HRM.API.Extensions;
using HRM.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class MeetingRoomAmenityController : ControllerBase
{
    private readonly IMeetingRoomAmenityService _meetingRoomAmenityService;

    public MeetingRoomAmenityController(IMeetingRoomAmenityService meetingRoomAmenityService)
    {
        _meetingRoomAmenityService = meetingRoomAmenityService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var response = await _meetingRoomAmenityService.GetAllAsync();

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var response = await _meetingRoomAmenityService.GetByIdAsync(id);

        if (!response.Success)
            return NotFound(response);

        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateMeetingRoomAmenityDto request)
    {
        var userId = User.GetUserId();

        var response = await _meetingRoomAmenityService.CreateAsync(request, userId);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpPut]
    public async Task<IActionResult> Update(UpdateMeetingRoomAmenityDto request)
    {
        var userId = User.GetUserId();

        var response = await _meetingRoomAmenityService.UpdateAsync(request, userId);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = User.GetUserId();

        var response = await _meetingRoomAmenityService.DeleteAsync(id, userId);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }
}
