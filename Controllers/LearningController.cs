using HRM.API.Extensions;
using HRM.API.Responses;
using HRM.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM.API.Controllers;

// Employee learning: every signed-in user acts only as their own employee record,
// and only on published, active courses of their own company.
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class LearningController : ControllerBase
{
    private readonly ILearningService _learningService;

    public LearningController(ILearningService learningService)
    {
        _learningService = learningService;
    }

    [HttpGet("courses")]
    public async Task<IActionResult> GetCourses()
    {
        var response = await _learningService.GetCoursesAsync(User.GetCompanyId(), User.GetUserId());

        if (!response.Success)
            return Failure(response);

        return Ok(response);
    }

    [HttpGet("courses/{courseId:guid}")]
    public async Task<IActionResult> GetCourse(Guid courseId)
    {
        var response = await _learningService.GetCourseAsync(courseId, User.GetCompanyId(), User.GetUserId());

        if (!response.Success)
            return Failure(response);

        return Ok(response);
    }

    [HttpGet("courses/{courseId:guid}/thumbnail")]
    public async Task<IActionResult> GetCourseThumbnail(Guid courseId)
    {
        var response = await _learningService.GetCourseThumbnailAsync(courseId, User.GetCompanyId());

        if (!response.Success || response.Data == null)
            return NotFound(response);

        return File(response.Data.Content, response.Data.ContentType);
    }

    [HttpGet("courses/{courseId:guid}/modules/{moduleId:guid}")]
    public async Task<IActionResult> GetModule(Guid courseId, Guid moduleId)
    {
        var response = await _learningService.GetModuleAsync(courseId, moduleId, User.GetCompanyId(), User.GetUserId());

        if (!response.Success)
            return Failure(response);

        return Ok(response);
    }

    // Issues a short-lived playback token for the module video endpoint.
    [HttpGet("courses/{courseId:guid}/modules/{moduleId:guid}/video-token")]
    public async Task<IActionResult> GetVideoToken(Guid courseId, Guid moduleId)
    {
        var response = await _learningService.GetVideoTokenAsync(courseId, moduleId, User.GetCompanyId(), User.GetUserId());

        if (!response.Success)
            return Failure(response);

        return Ok(response);
    }

    [HttpPost("courses/{courseId:guid}/modules/{moduleId:guid}/complete-learning")]
    public async Task<IActionResult> CompleteLearning(Guid courseId, Guid moduleId)
    {
        var response = await _learningService.CompleteLearningAsync(courseId, moduleId, User.GetCompanyId(), User.GetUserId());

        if (!response.Success)
            return Failure(response);

        return Ok(response);
    }

    // Starts a quiz attempt, or resumes the one that is still open.
    [HttpPost("courses/{courseId:guid}/modules/{moduleId:guid}/quiz/attempts")]
    public async Task<IActionResult> StartAttempt(Guid courseId, Guid moduleId)
    {
        var response = await _learningService.StartAttemptAsync(courseId, moduleId, User.GetCompanyId(), User.GetUserId());

        if (!response.Success)
            return Failure(response);

        return Ok(response);
    }

    [HttpPost("courses/{courseId:guid}/modules/{moduleId:guid}/quiz/attempts/{attemptId:guid}/submit")]
    public async Task<IActionResult> SubmitAttempt(Guid courseId, Guid moduleId, Guid attemptId, SubmitModuleQuizDto request)
    {
        var response = await _learningService.SubmitAttemptAsync(courseId, moduleId, attemptId, request, User.GetCompanyId(), User.GetUserId());

        if (!response.Success)
            return Failure(response);

        return Ok(response);
    }

    [HttpGet("courses/{courseId:guid}/modules/{moduleId:guid}/quiz/attempts")]
    public async Task<IActionResult> GetAttempts(Guid courseId, Guid moduleId)
    {
        var response = await _learningService.GetAttemptsAsync(courseId, moduleId, User.GetCompanyId(), User.GetUserId());

        if (!response.Success)
            return Failure(response);

        return Ok(response);
    }

    [HttpGet("courses/{courseId:guid}/modules/{moduleId:guid}/quiz/attempts/{attemptId:guid}")]
    public async Task<IActionResult> GetAttemptResult(Guid courseId, Guid moduleId, Guid attemptId)
    {
        var response = await _learningService.GetAttemptResultAsync(courseId, moduleId, attemptId, User.GetCompanyId(), User.GetUserId());

        if (!response.Success)
            return Failure(response);

        return Ok(response);
    }

    // Final course quiz. The service verifies on every request that the employee has
    // completed every required module; the course response reports the same eligibility.
    // Starts a final quiz attempt, or resumes the one that is still open.
    [HttpPost("courses/{courseId:guid}/final-quiz/attempts")]
    public async Task<IActionResult> StartFinalQuizAttempt(Guid courseId)
    {
        var response = await _learningService.StartAttemptAsync(courseId, null, User.GetCompanyId(), User.GetUserId());

        if (!response.Success)
            return Failure(response);

        return Ok(response);
    }

    [HttpPost("courses/{courseId:guid}/final-quiz/attempts/{attemptId:guid}/submit")]
    public async Task<IActionResult> SubmitFinalQuizAttempt(Guid courseId, Guid attemptId, SubmitModuleQuizDto request)
    {
        var response = await _learningService.SubmitAttemptAsync(courseId, null, attemptId, request, User.GetCompanyId(), User.GetUserId());

        if (!response.Success)
            return Failure(response);

        return Ok(response);
    }

    [HttpGet("courses/{courseId:guid}/final-quiz/attempts")]
    public async Task<IActionResult> GetFinalQuizAttempts(Guid courseId)
    {
        var response = await _learningService.GetAttemptsAsync(courseId, null, User.GetCompanyId(), User.GetUserId());

        if (!response.Success)
            return Failure(response);

        return Ok(response);
    }

    [HttpGet("courses/{courseId:guid}/final-quiz/attempts/{attemptId:guid}")]
    public async Task<IActionResult> GetFinalQuizAttemptResult(Guid courseId, Guid attemptId)
    {
        var response = await _learningService.GetAttemptResultAsync(courseId, null, attemptId, User.GetCompanyId(), User.GetUserId());

        if (!response.Success)
            return Failure(response);

        return Ok(response);
    }

    private IActionResult Failure<T>(ApiResponse<T> response)
    {
        if (response.Message == LearningService.EmployeeNotFoundMessage
            || response.Message == LearningService.CourseNotFoundMessage
            || response.Message == LearningService.ModuleNotFoundMessage
            || response.Message == LearningService.QuizNotFoundMessage
            || response.Message == LearningService.AttemptNotFoundMessage)
            return NotFound(response);

        return BadRequest(response);
    }
}
