namespace HRM.API.AI.Services;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using HRM.API.AI.Plugins;

public interface IAIService
{
    Task<string> ChatAsync(string message);
}


public class AIService : IAIService
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatCompletionService;
    private readonly LeavePlugin _leavePlugin;

    public AIService(
        Kernel kernel,
        IChatCompletionService chatCompletionService,
        LeavePlugin leavePlugin)
    {
        _kernel = kernel;
        _chatCompletionService = chatCompletionService;
        _leavePlugin = leavePlugin;
    }

    public async Task<string> ChatAsync(string message)
    {
        // Register AI functions
        _kernel.Plugins.AddFromObject(_leavePlugin);

        // Conversation
        var history = new ChatHistory();

        history.AddSystemMessage(
            "You are an HR Assistant. Always use available functions when employee-specific information is requested.");

        history.AddUserMessage(message);

        // Allow Semantic Kernel to automatically call functions
        var settings = new PromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        var response = await _chatCompletionService.GetChatMessageContentAsync(
            history,
            settings,
            _kernel);

        return response.Content ?? string.Empty;
    }
}