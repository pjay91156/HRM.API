using HRM.API.Extensions;
using HRM.API.Responses;
using HRM.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin,SuperAdmin")]
public class CourseController : ControllerBase
{
    private readonly ICourseService _courseService;

    public CourseController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var companyId = User.GetCompanyId();

        var response = await _courseService.GetAllAsync(companyId);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var companyId = User.GetCompanyId();

        var response = await _courseService.GetByIdAsync(id, companyId);

        if (!response.Success)
            return Failure(response);

        return Ok(response);
    }

    [HttpPost]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<IActionResult> Create([FromForm] CreateCourseDto request)
    {
        var userId = User.GetUserId();
        var companyId = User.GetCompanyId();

        var response = await _courseService.CreateAsync(request, companyId, userId);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpPut]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<IActionResult> Update([FromForm] UpdateCourseDto request)
    {
        var userId = User.GetUserId();
        var companyId = User.GetCompanyId();

        var response = await _courseService.UpdateAsync(request, companyId, userId);

        if (!response.Success)
            return Failure(response);

        return Ok(response);
    }

    [HttpPut("{id:guid}/active")]
    public async Task<IActionResult> SetActive(Guid id, UpdateCourseActiveDto request)
    {
        var userId = User.GetUserId();
        var companyId = User.GetCompanyId();

        var response = await _courseService.SetActiveAsync(id, request.IsActive, companyId, userId);

        if (!response.Success)
            return Failure(response);

        return Ok(response);
    }

    [HttpGet("{id:guid}/thumbnail")]
    public async Task<IActionResult> GetThumbnail(Guid id)
    {
        var companyId = User.GetCompanyId();

        var response = await _courseService.GetThumbnailAsync(id, companyId);

        if (!response.Success || response.Data == null)
            return NotFound(response);

        return File(response.Data.Content, response.Data.ContentType);
    }

    private IActionResult Failure<T>(ApiResponse<T> response)
    {
        if (response.Message == CourseService.NotFoundMessage)
            return NotFound(response);

        return BadRequest(response);
    }
}
