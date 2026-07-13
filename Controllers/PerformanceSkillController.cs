using HRM.API.Extensions;
using HRM.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PerformanceSkillController : ControllerBase
{
    private readonly IPerformanceSkillService _performanceSkillService;

    public PerformanceSkillController(IPerformanceSkillService performanceSkillService)
    {
        _performanceSkillService = performanceSkillService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreatePerformanceSkillDto request)
    {
        var companyId = User.GetCompanyId();
        var userId = User.GetUserId();

        var response = await _performanceSkillService.CreateAsync(request, companyId, userId);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpPut]
    public async Task<IActionResult> Update(UpdatePerformanceSkillDto request)
    {
        var companyId = User.GetCompanyId();
        var userId = User.GetUserId();

        var response = await _performanceSkillService.UpdateAsync(request, companyId, userId);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = User.GetUserId();

        var response = await _performanceSkillService.DeleteAsync(id, userId);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpGet("bycategory/{categoryId:guid}")]
    public async Task<IActionResult> GetByCategory(Guid categoryId)
    {
        var companyId = User.GetCompanyId();

        var response = await _performanceSkillService.GetByCategoryAsync(categoryId, companyId);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var response = await _performanceSkillService.GetByIdAsync(id);

        if (!response.Success)
            return NotFound(response);

        return Ok(response);
    }
}
