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
    var (response, refreshToken) = await _service.LoginAsync(request);

    if (!response.Success)
    {
        return BadRequest(response);
    }

    if (response.Data?.RequiresTwoFactor == true)
    {
        return Ok(response);
    }

    SetRefreshTokenCookie(refreshToken!);

    return Ok(response);
}

[HttpPost("verify-2fa")]
public async Task<ActionResult<ApiResponse<LoginResponse>>> VerifyTwoFactor(
    [FromBody] VerifyTwoFactorRequestDto request)
{
    var (response, refreshToken) = await _service.VerifyTwoFactorAsync(request);

    if (!response.Success)
    {
        return BadRequest(response);
    }

    SetRefreshTokenCookie(refreshToken!);

    return Ok(response);
}

[HttpPost("resend-2fa")]
public async Task<IActionResult> ResendTwoFactor([FromBody] ResendTwoFactorRequestDto request)
{
    var response = await _service.ResendTwoFactorCodeAsync(request);

    if (!response.Success)
    {
        return BadRequest(response);
    }

    return Ok(response);
}

[HttpPost("refresh")]
public async Task<IActionResult> Refresh()
{
    Request.Cookies.TryGetValue(RefreshTokenCookieName, out var refreshToken);

    var (response, newRefreshToken) = await _service.RefreshTokenAsync(refreshToken);

    if (!response.Success)
    {
        Response.Cookies.Delete(RefreshTokenCookieName);
        return Unauthorized(response);
    }

    SetRefreshTokenCookie(newRefreshToken!);

    return Ok(response);
}

[HttpPost("forgot-password")]
public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
{
    var response = await _service.ForgotPasswordAsync(request);

    return Ok(response);
}

[HttpPost("reset-password")]
public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
{
    var response = await _service.ResetPasswordAsync(request);

    if (!response.Success)
    {
        return BadRequest(response);
    }

    return Ok(response);
}

[HttpPost("logout")]
public async Task<IActionResult> Logout()
{
    Request.Cookies.TryGetValue(RefreshTokenCookieName, out var refreshToken);

    await _service.LogoutAsync(refreshToken);

    Response.Cookies.Delete(RefreshTokenCookieName);

    return Ok(new ApiResponse<string> { Success = true, Message = "Logged out successfully." });
}

private const string RefreshTokenCookieName = "refreshToken";

private void SetRefreshTokenCookie(string refreshToken)
{
    Response.Cookies.Append(RefreshTokenCookieName, refreshToken, new CookieOptions
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Strict,
        Expires = DateTimeOffset.UtcNow.AddDays(7)
    });
}
}