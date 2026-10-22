using HRM.API.Extensions;
using HRM.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin,SuperAdmin")]
[Route("api/[controller]")]
public class AuditLogController : ControllerBase
{
    private readonly IAuditLogService _auditLogService;

    public AuditLogController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAuditLogs([FromQuery] AuditLogFilterDto filter)
    {
        var companyId = User.GetCompanyId();
        var response = await _auditLogService.GetPagedAsync(companyId, filter);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var companyId = User.GetCompanyId();
        var response = await _auditLogService.GetSummaryAsync(companyId);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAuditLogById(Guid id)
    {
        var companyId = User.GetCompanyId();
        var response = await _auditLogService.GetByIdAsync(id, companyId);

        if (!response.Success)
            return NotFound(response);

        return Ok(response);
    }
}
