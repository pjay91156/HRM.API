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
public class DepartmentController : ControllerBase
{
    private readonly IDepartmentService _departmentService;

    public DepartmentController(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    [HttpGet("departments")]
    public async Task<ActionResult<ApiResponse<List<DepartmentResponse>>>> GetDepartmentsByCompanyId()
    {
        var companyId = User.GetCompanyId();

        var departments = await _departmentService.GetDepartmentsByCompanyIdAsync(companyId);

        return Ok(new ApiResponse<List<DepartmentResponse>>
        {
            Success = true,
            Message = "Departments retrieved successfully.",
            Data = departments
        });
    }
    [HttpPost("department")]
    public async Task<IActionResult> AddDepartment([FromBody] DepartmentDto request)
    {
        try
        {
            var companyId = User.GetCompanyId();
            var userId = User.GetUserId();

            var response = await _departmentService.AddDepartmentAsync(
                request,
                companyId,
                userId);

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ApiResponse<string>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                });
        }
    }
    [HttpDelete("department/{departmentId:guid}")]
    public async Task<IActionResult> DeleteEmployee(Guid departmentId)
    {

        var response = await _departmentService.DeleteDepartmentAsync(departmentId);

        return Ok(response);
    }
}