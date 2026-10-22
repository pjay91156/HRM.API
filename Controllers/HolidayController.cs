using HRM.API.Enums;
using HRM.API.Extensions;
using HRM.API.Repositories;
using HRM.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class HolidayController : ControllerBase
{
    private readonly IHolidayService _holidayService;
    private readonly IEmployeeRepository _employeeRepository;

    public HolidayController(IHolidayService holidayService, IEmployeeRepository employeeRepository)
    {
        _holidayService = holidayService;
        _employeeRepository = employeeRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetHolidays(
        [FromQuery] int? year,
        [FromQuery] HolidayType? holidayType,
        [FromQuery] string? location,
        [FromQuery] string? search)
    {
        var response = await _holidayService.GetHolidaysAsync(year, holidayType, location, search);
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetHolidayById(Guid id)
    {
        var response = await _holidayService.GetByIdAsync(id);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpGet("dashboard-summary")]
    public async Task<IActionResult> GetDashboardSummary([FromQuery] int? year)
    {
        var response = await _holidayService.GetDashboardSummaryAsync(year ?? DateTime.UtcNow.Year);
        return Ok(response);
    }

    [HttpGet("optional")]
    public async Task<IActionResult> GetOptionalHolidays([FromQuery] int? year)
    {
        var userId = User.GetUserId();
        var employee = await _employeeRepository.GetByUserIdAsync(userId);

        if (employee == null)
        {
            return BadRequest(new { success = false, message = "Employee is not found." });
        }

        var response = await _holidayService.GetOptionalHolidaysAsync(employee.Id, year ?? DateTime.UtcNow.Year);
        return Ok(response);
    }

    [HttpPost("optional/select")]
    public async Task<IActionResult> SelectOptionalHolidays([FromBody] SelectOptionalHolidaysDto request, [FromQuery] int? year)
    {
        var userId = User.GetUserId();
        var employee = await _employeeRepository.GetByUserIdAsync(userId);

        if (employee == null)
        {
            return BadRequest(new { success = false, message = "Employee is not found." });
        }

        var response = await _holidayService.SelectOptionalHolidaysAsync(employee.Id, year ?? DateTime.UtcNow.Year, request, userId);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> CreateHoliday([FromForm] HolidayDto request)
    {
        var userId = User.GetUserId();
        var response = await _holidayService.CreateAsync(request, userId);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpPut]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> UpdateHoliday([FromForm] UpdateHolidayDto request)
    {
        var userId = User.GetUserId();
        var response = await _holidayService.UpdateAsync(request, userId);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> DeleteHoliday(Guid id)
    {
        var response = await _holidayService.DeleteAsync(id);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }
}
