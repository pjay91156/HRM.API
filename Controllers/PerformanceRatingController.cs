using HRM.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HRM.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PerformanceRatingController : ControllerBase
{
    private readonly IPerformanceRatingService _performanceRatingService;

    public PerformanceRatingController(IPerformanceRatingService performanceRatingService)
    {
        _performanceRatingService = performanceRatingService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreatePerformanceRatingDto request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var response = await _performanceRatingService.CreateAsync(request, userId);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpPut]
    public async Task<IActionResult> Update(UpdatePerformanceRatingDto request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var response = await _performanceRatingService.UpdateAsync(request, userId);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var response = await _performanceRatingService.DeleteAsync(id, userId);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var response = await _performanceRatingService.GetAllAsync();

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var response = await _performanceRatingService.GetByIdAsync(id);

        if (!response.Success)
            return NotFound(response);

        return Ok(response);
    }
}
