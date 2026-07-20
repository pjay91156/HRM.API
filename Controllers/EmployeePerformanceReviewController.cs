using HRM.API.Extensions;
using HRM.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class EmployeePerformanceReviewController : ControllerBase
{
    private readonly IEmployeePerformanceReviewService _service;

    public EmployeePerformanceReviewController(IEmployeePerformanceReviewService service)
    {
        _service = service;
    }

    [HttpGet("my-reviews")]
    public async Task<IActionResult> GetMyReviews()
    {
        var userId = User.GetUserId();

        var response = await _service.GetMyReviewsAsync(userId);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpGet("my-reviews/{id:guid}")]
    public async Task<IActionResult> GetMyReviewDetail(Guid id)
    {
        var userId = User.GetUserId();

        var response = await _service.GetMyReviewDetailAsync(userId, id);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpPut("my-reviews/{id:guid}")]
    public async Task<IActionResult> SubmitEmployeeReview(Guid id, [FromBody] SubmitEmployeePerformanceReviewDto request)
    {
        var userId = User.GetUserId();

        var response = await _service.SubmitEmployeeReviewAsync(userId, id, request);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpGet("my-team-reviews")]
    public async Task<IActionResult> GetMyTeamReviews()
    {
        var userId = User.GetUserId();

        var response = await _service.GetMyTeamReviewsAsync(userId);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpGet("my-team-reviews/{id:guid}")]
    public async Task<IActionResult> GetTeamReviewDetail(Guid id)
    {
        var userId = User.GetUserId();

        var response = await _service.GetTeamReviewDetailAsync(userId, id);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpPut("my-team-reviews/{id:guid}")]
    public async Task<IActionResult> SubmitManagerReview(Guid id, [FromBody] SubmitManagerPerformanceReviewDto request)
    {
        var userId = User.GetUserId();

        var response = await _service.SubmitManagerReviewAsync(userId, id, request);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }
}
