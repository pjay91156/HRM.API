using HRM.API.Extensions;
using HRM.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CompanyDocumentController : ControllerBase
{
    private readonly ICompanyDocumentService _companyDocumentService;

    public CompanyDocumentController(ICompanyDocumentService companyDocumentService)
    {
        _companyDocumentService = companyDocumentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var response = await _companyDocumentService.GetAllAsync();

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var response = await _companyDocumentService.GetByIdAsync(id);

        if (!response.Success)
            return NotFound(response);

        return Ok(response);
    }

    [HttpPost]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> Create([FromForm] CreateCompanyDocumentDto request)
    {
        var userId = User.GetUserId();

        var response = await _companyDocumentService.CreateAsync(request, userId);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpPut]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> Update([FromForm] UpdateCompanyDocumentDto request)
    {
        var userId = User.GetUserId();

        var response = await _companyDocumentService.UpdateAsync(request, userId);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = User.GetUserId();

        var response = await _companyDocumentService.DeleteAsync(id, userId);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpGet("{id:guid}/download")]
    public async Task<IActionResult> Download(Guid id)
    {
        var response = await _companyDocumentService.GetDownloadInfoAsync(id);

        if (!response.Success || response.Data == null)
            return NotFound(response);

        var bytes = await System.IO.File.ReadAllBytesAsync(response.Data.AbsolutePath);

        return File(bytes, response.Data.ContentType, response.Data.FileName);
    }
}
