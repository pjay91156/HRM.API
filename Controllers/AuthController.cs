using HRM.API.DTOs.Auth;
using HRM.API.Services;
using Microsoft.AspNetCore.Mvc;
using HRM.API.Responses;

namespace HRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _service;

    public AuthController(IAuthService service)
    {
        _service = service;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequestDto request)
    {
        var response = await _service.RegisterAsync(request);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }
[HttpPost("login")]
[ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status400BadRequest)]
public async Task<ActionResult<ApiResponse<LoginResponse>>> Login(
    [FromBody] LoginRequestDto request)
{
    var response = await _service.LoginAsync(request);

    if (!response.Success)
    {
        return BadRequest(response);
    }

    return Ok(response);
}
}