using HRM.API.Extensions;
using HRM.API.Responses;
using HRM.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM.API.Controllers;

[Route("api/course/{courseId:guid}/modules")]
[ApiController]
[Authorize(Roles = "Admin,SuperAdmin")]
public class CourseModuleController : ControllerBase
{
    // Room for the video plus the other form fields.
    private const long MaxRequestBytes = CourseModuleService.MaxVideoUploadCeilingBytes + 10 * 1024 * 1024;

    private readonly ICourseModuleService _courseModuleService;

    public CourseModuleController(ICourseModuleService courseModuleService)
    {
        _courseModuleService = courseModuleService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(Guid courseId)
    {
        var companyId = User.GetCompanyId();

        var response = await _courseModuleService.GetAllAsync(courseId, companyId);

        if (!response.Success)
            return Failure(response);

        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid courseId, Guid id)
    {
        var companyId = User.GetCompanyId();

        var response = await _courseModuleService.GetByIdAsync(courseId, id, companyId);

        if (!response.Success)
            return Failure(response);

        return Ok(response);
    }

    [HttpPost]
    [RequestSizeLimit(MaxRequestBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxRequestBytes)]
    public async Task<IActionResult> Create(Guid courseId, [FromForm] CreateCourseModuleDto request)
    {
        var userId = User.GetUserId();
        var companyId = User.GetCompanyId();

        var response = await _courseModuleService.CreateAsync(courseId, request, companyId, userId);

        if (!response.Success)
            return Failure(response);

        return Ok(response);
    }

    [HttpPut("{id:guid}")]
    [RequestSizeLimit(MaxRequestBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxRequestBytes)]
    public async Task<IActionResult> Update(Guid courseId, Guid id, [FromForm] UpdateCourseModuleDto request)
    {
        var userId = User.GetUserId();
        var companyId = User.GetCompanyId();

        var response = await _courseModuleService.UpdateAsync(courseId, id, request, companyId, userId);

        if (!response.Success)
            return Failure(response);

        return Ok(response);
    }

    [HttpPut("{id:guid}/active")]
    public async Task<IActionResult> SetActive(Guid courseId, Guid id, UpdateCourseModuleActiveDto request)
    {
        var userId = User.GetUserId();
        var companyId = User.GetCompanyId();

        var response = await _courseModuleService.SetActiveAsync(courseId, id, request.IsActive, companyId, userId);

        if (!response.Success)
            return Failure(response);

        return Ok(response);
    }

    [HttpPut("reorder")]
    public async Task<IActionResult> Reorder(Guid courseId, ReorderCourseModulesDto request)
    {
        var userId = User.GetUserId();
        var companyId = User.GetCompanyId();

        var response = await _courseModuleService.ReorderAsync(courseId, request, companyId, userId);

        if (!response.Success)
            return Failure(response);

        return Ok(response);
    }

    [HttpPost("{id:guid}/video")]
    [RequestSizeLimit(MaxRequestBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxRequestBytes)]
    public async Task<IActionResult> UploadVideo(Guid courseId, Guid id, [FromForm] UploadCourseModuleVideoDto request)
    {
        var userId = User.GetUserId();
        var companyId = User.GetCompanyId();

        var response = await _courseModuleService.UploadVideoAsync(courseId, id, request.Video, companyId, userId);

        if (!response.Success)
            return Failure(response);

        return Ok(response);
    }

    [HttpDelete("{id:guid}/video")]
    public async Task<IActionResult> RemoveVideo(Guid courseId, Guid id)
    {
        var userId = User.GetUserId();
        var companyId = User.GetCompanyId();

        var response = await _courseModuleService.RemoveVideoAsync(courseId, id, companyId, userId);

        if (!response.Success)
            return Failure(response);

        return Ok(response);
    }

    // Issues a short-lived playback token for the video endpoint below.
    [HttpGet("{id:guid}/video-token")]
    public async Task<IActionResult> GetVideoToken(Guid courseId, Guid id)
    {
        var companyId = User.GetCompanyId();

        var response = await _courseModuleService.CreateVideoTokenAsync(courseId, id, companyId);

        if (!response.Success)
            return Failure(response);

        return Ok(response);
    }

    // Anonymous because a browser video player cannot send the Authorization header;
    // access is granted only by a valid token from GetVideoToken.
    [AllowAnonymous]
    [HttpGet("{id:guid}/video")]
    public async Task<IActionResult> GetVideo(Guid courseId, Guid id, [FromQuery] string? token)
    {
        var response = await _courseModuleService.OpenVideoAsync(courseId, id, token);

        if (!response.Success || response.Data == null)
            return NotFound(response);

        return File(response.Data.Stream, response.Data.ContentType, enableRangeProcessing: true);
    }

    private IActionResult Failure<T>(ApiResponse<T> response)
    {
        if (response.Message == CourseModuleService.CourseNotFoundMessage
            || response.Message == CourseModuleService.ModuleNotFoundMessage)
            return NotFound(response);

        return BadRequest(response);
    }
}
