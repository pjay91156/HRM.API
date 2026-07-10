using HRM.API.Extensions;
using HRM.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PerformanceTemplateController : ControllerBase
{
    private readonly IPerformanceTemplateService _performanceTemplateService;

    public PerformanceTemplateController(IPerformanceTemplateService performanceTemplateService)
    {
        _performanceTemplateService = performanceTemplateService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreatePerformanceTemplateDto request)
    {
        var companyId = User.GetCompanyId();
        var userId = User.GetUserId();

        var response = await _performanceTemplateService.CreateAsync(request, companyId, userId);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpPut]
    public async Task<IActionResult> Update(UpdatePerformanceTemplateDto request)
    {
        var companyId = User.GetCompanyId();
        var userId = User.GetUserId();

        var response = await _performanceTemplateService.UpdateAsync(request, companyId, userId);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = User.GetUserId();

        var response = await _performanceTemplateService.DeleteAsync(id, userId);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var companyId = User.GetCompanyId();

        var response = await _performanceTemplateService.GetAllAsync(companyId);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var response = await _performanceTemplateService.GetByIdAsync(id);

        if (!response.Success)
            return NotFound(response);

        return Ok(response);
    }
}
