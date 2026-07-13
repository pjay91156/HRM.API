using HRM.API.Extensions;
using HRM.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PerformanceCategoryController : ControllerBase
{
    private readonly IPerformanceCategoryService _performanceCategoryService;

    public PerformanceCategoryController(IPerformanceCategoryService performanceCategoryService)
    {
        _performanceCategoryService = performanceCategoryService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreatePerformanceCategoryDto request)
    {
        var companyId = User.GetCompanyId();
        var userId = User.GetUserId();

        var response = await _performanceCategoryService.CreateAsync(request, companyId, userId);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpPut]
    public async Task<IActionResult> Update(UpdatePerformanceCategoryDto request)
    {
        var companyId = User.GetCompanyId();
        var userId = User.GetUserId();

        var response = await _performanceCategoryService.UpdateAsync(request, companyId, userId);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = User.GetUserId();

        var response = await _performanceCategoryService.DeleteAsync(id, userId);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpGet("bytemplate/{templateId:guid}")]
    public async Task<IActionResult> GetByTemplate(Guid templateId)
    {
        var companyId = User.GetCompanyId();

        var response = await _performanceCategoryService.GetByTemplateAsync(templateId, companyId);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var response = await _performanceCategoryService.GetByIdAsync(id);

        if (!response.Success)
            return NotFound(response);

        return Ok(response);
    }
}
