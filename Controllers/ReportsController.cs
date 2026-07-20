using System.Text;
using HRM.API.Extensions;
using HRM.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    private FileContentResult AsCsvFile(string csv, string fileName)
    {
        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv)).ToArray();
        return File(bytes, "text/csv", fileName);
    }

    [HttpGet("employees")]
    public async Task<IActionResult> GetEmployeeDirectoryReport()
    {
        var companyId = User.GetCompanyId();

        var csv = await _reportService.GetEmployeeDirectoryReportAsync(companyId);

        return AsCsvFile(csv, "employee-directory.csv");
    }

    [HttpGet("attendance")]
    public async Task<IActionResult> GetAttendanceReport([FromQuery] DateOnly fromDate, [FromQuery] DateOnly toDate)
    {
        if (fromDate > toDate)
        {
            return BadRequest(new { success = false, message = "From date cannot be greater than To date." });
        }

        var userId = User.GetUserId();
        var companyId = User.GetCompanyId();

        var csv = await _reportService.GetAttendanceReportAsync(userId, companyId, fromDate, toDate);

        return AsCsvFile(csv, $"attendance-report_{fromDate}_{toDate}.csv");
    }

    [HttpGet("leaves")]
    public async Task<IActionResult> GetLeaveReport([FromQuery] DateOnly? fromDate, [FromQuery] DateOnly? toDate, [FromQuery] string? status)
    {
        var userId = User.GetUserId();

        var csv = await _reportService.GetLeaveReportAsync(userId, fromDate, toDate, status);

        return AsCsvFile(csv, "leave-report.csv");
    }

    [HttpGet("performance-reviews")]
    public async Task<IActionResult> GetPerformanceReviewReport([FromQuery] Guid? cycleId)
    {
        var userId = User.GetUserId();

        var csv = await _reportService.GetPerformanceReviewReportAsync(userId, cycleId);

        return AsCsvFile(csv, "performance-review-report.csv");
    }
}
