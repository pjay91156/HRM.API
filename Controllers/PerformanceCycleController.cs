using HRM.API.Models;
using HRM.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HRM.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PerformanceCycleController : ControllerBase
{
    private readonly IPerformanceCycleService _performanceCycleService;

    public PerformanceCycleController(IPerformanceCycleService performanceCycleService)
    {
        _performanceCycleService = performanceCycleService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreatePerformanceCycleDto request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var response = await _performanceCycleService.CreateAsync(request, userId);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpPut]
    public async Task<IActionResult> Update(UpdatePerformanceCycleDto request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var response = await _performanceCycleService.UpdateAsync(request, userId);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var response = await _performanceCycleService.DeleteAsync(id, userId);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }
    [HttpGet]
public async Task<IActionResult> GetAll()
{
    var response = await _performanceCycleService.GetAllAsync();

    if (!response.Success)
        return BadRequest(response);

    return Ok(response);
}

[HttpGet("{id:guid}")]
public async Task<IActionResult> GetById(Guid id)
{
    var response = await _performanceCycleService.GetByIdAsync(id);

    if (!response.Success)
        return NotFound(response);

    return Ok(response);
}
}