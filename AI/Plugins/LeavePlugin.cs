
using HRM.API.Responses;
using HRM.API.Services;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace HRM.API.AI.Plugins;

public class LeavePlugin
{
    private readonly ILeaveRequestService _leaveRequestService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<LeavePlugin> _logger;

    public LeavePlugin(
        ILeaveRequestService leaveRequestService,
        ICurrentUserService currentUserService,
        ILogger<LeavePlugin> logger)
    {
        _leaveRequestService = leaveRequestService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    [KernelFunction]
    [Description("Gets the current leave balance of the logged-in employee including all leave types and remaining balances.")]
    public async Task<List<LeaveBalanceResponse>> GetCurrentLeaveBalance()
    {
        try
        {
            _logger.LogInformation("AI Plugin: GetCurrentLeaveBalance started.");

            var userId = _currentUserService.UserId;

            _logger.LogInformation("Current User Id: {UserId}", userId);

            var response = await _leaveRequestService.GetLeaveBalanceAsync(userId);

            if (!response.Success)
            {
                _logger.LogWarning(
                    "Unable to retrieve leave balance. Message: {Message}",
                    response.Message);

                throw new Exception(response.Message);
            }

            _logger.LogInformation(
                "Leave balance retrieved successfully. Total Leave Types: {Count}",
                response.Data?.Count ?? 0);

            return response.Data ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "An error occurred while retrieving leave balance.");

            throw;
        }
    }
}