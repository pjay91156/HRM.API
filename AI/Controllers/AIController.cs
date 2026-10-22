using HRM.API.AI.Models;
using HRM.API.AI.Services;
using Microsoft.AspNetCore.Mvc;
using HRM.API.Responses;

namespace HRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AIController : ControllerBase
{
    private readonly IAIService _aiService;
    private readonly ILogger<AIController> _logger;

    public AIController(
        IAIService aiService,
        ILogger<AIController> logger)
    {
        _aiService = aiService;
        _logger = logger;
    }

    [HttpPost("chat")]
    public async Task<IActionResult> Chat([FromBody] ChatDto request)
    {
        try
        {
            _logger.LogInformation("AI chat request received.");

            if (string.IsNullOrWhiteSpace(request.Message))
            {
                _logger.LogWarning("AI chat request contains an empty message.");

                return BadRequest(new ApiResponse<ChatResponse>
                {
                    Success = false,
                    Message = "Message is required."
                });
            }

            _logger.LogInformation("User Message: {Message}", request.Message);

            var response = await _aiService.ChatAsync(request.Message);

            _logger.LogInformation("AI response generated successfully.");

            return Ok(new ApiResponse<ChatResponse>
            {
                Success = true,
                Message = "Response generated successfully.",
                Data = new ChatResponse
                {
                    Response = response
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while processing AI chat request.");

            return StatusCode(StatusCodes.Status500InternalServerError,
                new ApiResponse<ChatResponse>
                {
                    Success = false,
                    Message = "An error occurred while processing the AI request.",
                    Errors = new List<string>
                    {
                    ex.Message
                    }
                });
        }
    }
}