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
public class EmployeeController : ControllerBase
{
    private readonly IEmployeeService _employeeService;
    private readonly ILogger<EmployeeController> _logger;

    public EmployeeController(
        IEmployeeService employeeService,
        ILogger<EmployeeController> logger)
    {
        _employeeService = employeeService;
        _logger = logger;
    }

    [HttpGet("employees")]
    public async Task<ActionResult<ApiResponse<EmployeeResponse>>> GetEmployees()
    {
        try
        {
            var companyId = User.GetCompanyId();

            var response = await _employeeService.GetEmployeesAsync(companyId);

            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access while retrieving employees.");

            return Unauthorized(new ApiResponse<List<EmployeeResponse>>
            {
                Success = false,
                Message = "Unauthorized access.",
                Errors = new List<string> { ex.Message }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving employees.");

            return StatusCode(StatusCodes.Status500InternalServerError,
                new ApiResponse<List<EmployeeResponse>>
                {
                    Success = false,
                    Message = "An unexpected error occurred.",
                    Errors = new List<string> { ex.Message }
                });
        }
    }
    [HttpGet("employees/{departmentId:guid}")]
    public async Task<ActionResult<ApiResponse<EmployeeResponse>>> GetEmployeesByDepartment(Guid departmentId)
    {
        try
        {
            var companyId = User.GetCompanyId();

            var response = await _employeeService.GetEmployeesByDepartmentAsync(companyId, departmentId);

            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access while retrieving employees by department.");

            return Unauthorized(new ApiResponse<List<EmployeeResponse>>
            {
                Success = false,
                Message = "Unauthorized access.",
                Errors = new List<string> { ex.Message }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving employees by department.");

            return StatusCode(StatusCodes.Status500InternalServerError,
                new ApiResponse<List<EmployeeResponse>>
                {
                    Success = false,
                    Message = "An unexpected error occurred.",
                    Errors = new List<string> { ex.Message }
                });
        }
    }
    [HttpPost("employee")]
    public async Task<IActionResult> AddEmployee([FromBody] EmployeeDto request)
    {
        try
        {
            var companyId = User.GetCompanyId();
            var userId = User.GetUserId();

            var response = await _employeeService.AddEmployeeAsync(
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
    [HttpDelete("employee/{employeeId:guid}")]
    public async Task<IActionResult> DeleteEmployee(Guid employeeId)
    {

        var response = await _employeeService.DeleteEmployeeAsync(employeeId);

        return Ok(response);
    }
    [HttpPut("employee/{employeeId:guid}/role")]
    public async Task<IActionResult> UpdateEmployeeRole(Guid employeeId, [FromBody] UpdateUserRoleDto request)
    {
        var userId = User.GetUserId();

        var response = await _employeeService.UpdateUserRoleAsync(userId, employeeId, request.Role);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }
[HttpGet("my-team")]
public async Task<IActionResult> GetMyTeam()
{
    try
    {
        var userId = User.GetUserId();
        var companyId = User.GetCompanyId();

        var response = await _employeeService
            .GetMyTeamHierarchyAsync(userId, companyId);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }
    catch (UnauthorizedAccessException ex)
    {
        _logger.LogWarning(ex, "Unauthorized access while retrieving team hierarchy.");

        return Unauthorized(new ApiResponse<EmployeeHierarchyResponse>
        {
            Success = false,
            Message = "Unauthorized access.",
            Errors = new List<string> { ex.Message }
        });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error occurred while retrieving team hierarchy.");

        return StatusCode(StatusCodes.Status500InternalServerError,
            new ApiResponse<EmployeeHierarchyResponse>
            {
                Success = false,
                Message = "An unexpected error occurred.",
                Errors = new List<string> { ex.Message }
            });
    }
}
}