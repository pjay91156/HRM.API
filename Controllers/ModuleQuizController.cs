using HRM.API.Extensions;
using HRM.API.Responses;
using HRM.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM.API.Controllers;

// HR/Admin management of the quiz that belongs to one module.
[Route("api/course/{courseId:guid}/modules/{moduleId:guid}/quiz")]
[ApiController]
[Authorize(Roles = "Admin,SuperAdmin")]
public class ModuleQuizController : ControllerBase
{
    private readonly IModuleQuizService _moduleQuizService;

    public ModuleQuizController(IModuleQuizService moduleQuizService)
    {
        _moduleQuizService = moduleQuizService;
    }

    [HttpGet]
    public async Task<IActionResult> Get(Guid courseId, Guid moduleId)
    {
        var companyId = User.GetCompanyId();

        var response = await _moduleQuizService.GetAsync(courseId, moduleId, companyId);

        if (!response.Success)
            return Failure(response);

        return Ok(response);
    }

    [HttpPut]
    public async Task<IActionResult> SaveSettings(Guid courseId, Guid moduleId, SaveModuleQuizSettingsDto request)
    {
        var userId = User.GetUserId();
        var companyId = User.GetCompanyId();

        var response = await _moduleQuizService.SaveSettingsAsync(courseId, moduleId, request, companyId, userId);

        if (!response.Success)
            return Failure(response);

        return Ok(response);
    }

    [HttpPut("active")]
    public async Task<IActionResult> SetActive(Guid courseId, Guid moduleId, UpdateModuleQuizActiveDto request)
    {
        var userId = User.GetUserId();
        var companyId = User.GetCompanyId();

        var response = await _moduleQuizService.SetActiveAsync(courseId, moduleId, request.IsActive, companyId, userId);

        if (!response.Success)
            return Failure(response);

        return Ok(response);
    }

    [HttpPost("questions")]
    public async Task<IActionResult> AddQuestion(Guid courseId, Guid moduleId, SaveModuleQuizQuestionDto request)
    {
        var userId = User.GetUserId();
        var companyId = User.GetCompanyId();

        var response = await _moduleQuizService.AddQuestionAsync(courseId, moduleId, request, companyId, userId);

        if (!response.Success)
            return Failure(response);

        return Ok(response);
    }

    [HttpPut("questions/{questionId:guid}")]
    public async Task<IActionResult> UpdateQuestion(Guid courseId, Guid moduleId, Guid questionId, SaveModuleQuizQuestionDto request)
    {
        var userId = User.GetUserId();
        var companyId = User.GetCompanyId();

        var response = await _moduleQuizService.UpdateQuestionAsync(courseId, moduleId, questionId, request, companyId, userId);

        if (!response.Success)
            return Failure(response);

        return Ok(response);
    }

    [HttpDelete("questions/{questionId:guid}")]
    public async Task<IActionResult> DeleteQuestion(Guid courseId, Guid moduleId, Guid questionId)
    {
        var userId = User.GetUserId();
        var companyId = User.GetCompanyId();

        var response = await _moduleQuizService.DeleteQuestionAsync(courseId, moduleId, questionId, companyId, userId);

        if (!response.Success)
            return Failure(response);

        return Ok(response);
    }

    [HttpPut("questions/reorder")]
    public async Task<IActionResult> ReorderQuestions(Guid courseId, Guid moduleId, ReorderModuleQuizQuestionsDto request)
    {
        var userId = User.GetUserId();
        var companyId = User.GetCompanyId();

        var response = await _moduleQuizService.ReorderQuestionsAsync(courseId, moduleId, request, companyId, userId);

        if (!response.Success)
            return Failure(response);

        return Ok(response);
    }

    // The quiz as an employee would see it, without the answer key.
    [HttpGet("preview")]
    public async Task<IActionResult> GetPreview(Guid courseId, Guid moduleId)
    {
        var companyId = User.GetCompanyId();

        var response = await _moduleQuizService.GetPreviewAsync(courseId, moduleId, companyId);

        if (!response.Success)
            return Failure(response);

        return Ok(response);
    }

    private IActionResult Failure<T>(ApiResponse<T> response)
    {
        if (response.Message == ModuleQuizService.CourseNotFoundMessage
            || response.Message == ModuleQuizService.ModuleNotFoundMessage
            || response.Message == ModuleQuizService.QuizNotFoundMessage
            || response.Message == ModuleQuizService.QuestionNotFoundMessage)
            return NotFound(response);

        return BadRequest(response);
    }
}
