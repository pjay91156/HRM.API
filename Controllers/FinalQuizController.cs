using HRM.API.Extensions;
using HRM.API.Responses;
using HRM.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM.API.Controllers;

// HR/Admin management of a course's final quiz. It uses the same quiz service as the
// module quizzes; no module id means the course's final quiz.
[Route("api/course/{courseId:guid}/final-quiz")]
[ApiController]
[Authorize(Roles = "Admin,SuperAdmin")]
public class FinalQuizController : ControllerBase
{
    private readonly IModuleQuizService _moduleQuizService;

    public FinalQuizController(IModuleQuizService moduleQuizService)
    {
        _moduleQuizService = moduleQuizService;
    }

    [HttpGet]
    public async Task<IActionResult> Get(Guid courseId)
    {
        var companyId = User.GetCompanyId();

        var response = await _moduleQuizService.GetAsync(courseId, null, companyId);

        if (!response.Success)
            return Failure(response);

        return Ok(response);
    }

    [HttpPut]
    public async Task<IActionResult> SaveSettings(Guid courseId, SaveModuleQuizSettingsDto request)
    {
        var userId = User.GetUserId();
        var companyId = User.GetCompanyId();

        var response = await _moduleQuizService.SaveSettingsAsync(courseId, null, request, companyId, userId);

        if (!response.Success)
            return Failure(response);

        return Ok(response);
    }

    [HttpPut("active")]
    public async Task<IActionResult> SetActive(Guid courseId, UpdateModuleQuizActiveDto request)
    {
        var userId = User.GetUserId();
        var companyId = User.GetCompanyId();

        var response = await _moduleQuizService.SetActiveAsync(courseId, null, request.IsActive, companyId, userId);

        if (!response.Success)
            return Failure(response);

        return Ok(response);
    }

    [HttpPost("questions")]
    public async Task<IActionResult> AddQuestion(Guid courseId, SaveModuleQuizQuestionDto request)
    {
        var userId = User.GetUserId();
        var companyId = User.GetCompanyId();

        var response = await _moduleQuizService.AddQuestionAsync(courseId, null, request, companyId, userId);

        if (!response.Success)
            return Failure(response);

        return Ok(response);
    }

    [HttpPut("questions/{questionId:guid}")]
    public async Task<IActionResult> UpdateQuestion(Guid courseId, Guid questionId, SaveModuleQuizQuestionDto request)
    {
        var userId = User.GetUserId();
        var companyId = User.GetCompanyId();

        var response = await _moduleQuizService.UpdateQuestionAsync(courseId, null, questionId, request, companyId, userId);

        if (!response.Success)
            return Failure(response);

        return Ok(response);
    }

    [HttpDelete("questions/{questionId:guid}")]
    public async Task<IActionResult> DeleteQuestion(Guid courseId, Guid questionId)
    {
        var userId = User.GetUserId();
        var companyId = User.GetCompanyId();

        var response = await _moduleQuizService.DeleteQuestionAsync(courseId, null, questionId, companyId, userId);

        if (!response.Success)
            return Failure(response);

        return Ok(response);
    }

    [HttpPut("questions/reorder")]
    public async Task<IActionResult> ReorderQuestions(Guid courseId, ReorderModuleQuizQuestionsDto request)
    {
        var userId = User.GetUserId();
        var companyId = User.GetCompanyId();

        var response = await _moduleQuizService.ReorderQuestionsAsync(courseId, null, request, companyId, userId);

        if (!response.Success)
            return Failure(response);

        return Ok(response);
    }

    // The final quiz as an employee would see it, without the answer key.
    [HttpGet("preview")]
    public async Task<IActionResult> GetPreview(Guid courseId)
    {
        var companyId = User.GetCompanyId();

        var response = await _moduleQuizService.GetPreviewAsync(courseId, null, companyId);

        if (!response.Success)
            return Failure(response);

        return Ok(response);
    }

    // Employees who have completed the course.
    [HttpGet("completions")]
    public async Task<IActionResult> GetCompletions(Guid courseId)
    {
        var companyId = User.GetCompanyId();

        var response = await _moduleQuizService.GetCourseCompletionsAsync(courseId, companyId);

        if (!response.Success)
            return Failure(response);

        return Ok(response);
    }

    private IActionResult Failure<T>(ApiResponse<T> response)
    {
        if (response.Message == ModuleQuizService.CourseNotFoundMessage
            || response.Message == ModuleQuizService.QuizNotFoundMessage
            || response.Message == ModuleQuizService.QuestionNotFoundMessage)
            return NotFound(response);

        return BadRequest(response);
    }
}
