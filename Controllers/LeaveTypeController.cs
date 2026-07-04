using HRM.API.DTOs;
using HRM.API.Extensions;
using HRM.API.Models;
using HRM.API.Responses;
using HRM.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM.API.Controllers;
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class LeaveTypeController : ControllerBase
{
    private readonly ILeaveTypeService _leaveTypeService;

    public LeaveTypeController(
        ILeaveTypeService leaveTypeService)
    {
        _leaveTypeService = leaveTypeService;
    }

    [HttpGet]
    public async Task<IActionResult> GetLeaveTypes()
    {
        var response =
            await _leaveTypeService.GetLeaveTypesAsync();

        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> AddLeaveType(
        LeaveTypeDto request)
    {
        var userId = User.GetUserId();

        var response =
            await _leaveTypeService.AddLeaveTypeAsync(
                request,
                userId);

        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteLeaveType(Guid id)
    {
        var response =
            await _leaveTypeService.DeleteLeaveTypeAsync(id);

        return Ok(response);
    }
}