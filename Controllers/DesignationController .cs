using HRM.API.DTOs;
using HRM.API.Extensions;
using HRM.API.Models;
using HRM.API.Responses;
using HRM.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class DesignationController : ControllerBase
{
    private readonly IDesignationService _designationService;

    public DesignationController(IDesignationService designationService)
    {
        _designationService = designationService;
    }

    [HttpGet("designations/{departmentId}")]
    public async Task<IActionResult> GetDesignationsByDepartmentId(Guid departmentId)
    {
        var response = await _designationService
            .GetDesignationsByDepartmentIdAsync(departmentId);

        return Ok(response);
    }
    [HttpGet("alldesignations")]
    public async Task<IActionResult> GetDesignations()
    {
        var response = await _designationService
            .GetDesignationsAsync();

        return Ok(response);
    }
    [HttpPost("designation")]
    public async Task<IActionResult> AddDesignation([FromBody] DesignationDto request)
    {
        try
        {
            var userId = User.GetUserId();
            var response = await _designationService.AddDesignationAsync(request, userId);

            if (response.Success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                });
        }
    }
    [HttpDelete("designation/{id}")]
    public async Task<IActionResult> DeleteDesignation(Guid id)
    {
        try
        {
            var userId = User.GetUserId();
            var response = await _designationService.DeleteDesignationAsync(id, userId);

            if (response.Success)
                return Ok(response);

            return BadRequest(response);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
        }
    }
}