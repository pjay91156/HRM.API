using HRM.API.Extensions;
using HRM.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class CompanyController : ControllerBase
{
    private readonly ICompanyService _companyService;

    public CompanyController(ICompanyService companyService)
    {
        _companyService = companyService;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetCompanyInformation()
    {
        var companyId = User.GetCompanyId();

        var response = await _companyService.GetCompanyInformationAsync(companyId);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateCompanyInformation([FromBody] UpdateCompanyInformationDto request)
    {
        var companyId = User.GetCompanyId();
        var userId = User.GetUserId();

        var response = await _companyService.UpdateCompanyInformationAsync(companyId, request, userId);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpPost("me/logo")]
    [RequestSizeLimit(2 * 1024 * 1024)]
    public async Task<IActionResult> UpdateCompanyLogo([FromForm] IFormFile file)
    {
        var companyId = User.GetCompanyId();
        var userId = User.GetUserId();

        var response = await _companyService.UpdateCompanyLogoAsync(companyId, file, userId);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }
}
