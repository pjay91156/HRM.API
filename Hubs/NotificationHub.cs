using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace HRM.API.Hubs;

[Authorize]
public class NotificationHub : Hub
{
     public override async Task OnConnectedAsync()
    {
        var userId = Context.User?
            .FindFirst(ClaimTypes.NameIdentifier)?.Value;

        Console.WriteLine($"Connected user: {userId}");

        await base.OnConnectedAsync();
    }
}