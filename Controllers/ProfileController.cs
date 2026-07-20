using HRM.API.Extensions;
using HRM.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;

    public ProfileController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = User.GetUserId();

        var response = await _profileService.GetMyProfileAsync(userId);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateMyProfileDto request)
    {
        var userId = User.GetUserId();

        var response = await _profileService.UpdateMyProfileAsync(userId, request);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpPost("me/picture")]
    [RequestSizeLimit(2 * 1024 * 1024)]
    public async Task<IActionResult> UpdateMyProfilePicture([FromForm] IFormFile file)
    {
        var userId = User.GetUserId();

        var response = await _profileService.UpdateMyProfilePictureAsync(userId, file);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }
}
