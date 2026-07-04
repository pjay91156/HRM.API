using HRM.API.DTOs;
using HRM.API.Extensions;
using HRM.API.Models;
using HRM.API.Responses;
using HRM.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("dashboarddetails")]
    public async Task<IActionResult> GetDashboard()
    {
        try
        {
            var companyId = User.GetCompanyId();

            var response = await _dashboardService.GetDashboardAsync(companyId);

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<string>
            {
                Success = false,
                Message = ex.Message
            });
        }
    }
    [HttpGet("department-chart")]
public async Task<IActionResult>
    GetDepartmentEmployeeChart()
{
    try
    {
        var response =
            await _dashboardService
                .GetDepartmentEmployeeCountsAsync();

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }
    catch (Exception ex)
    {
        return StatusCode(500,
            new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred.",
                Errors = new List<string>
                {
                    ex.Message
                }
            });
    }
}
    [HttpGet("headcount-trend")]
    public async Task<IActionResult> GetHeadcountTrend()
    {
        var companyId = User.GetCompanyId();

        var result = await _dashboardService.GetHeadcountTrendAsync(companyId);

        return Ok(result);
    }
}