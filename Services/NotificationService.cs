using HRM.API.Enums;
using HRM.API.Models;
using HRM.API.Repositories;
using HRM.API.Responses;

namespace HRM.API.Services;

public interface INotificationService
{
    Task CreateAsync(
        Guid userId,
        string title,
        string message,
        NotificationType type,
        string? link,
        Guid createdBy);

    Task<ApiResponse<NotificationListResponse>> GetMyNotificationsAsync(Guid userId);

    Task<ApiResponse<string>> MarkAsReadAsync(Guid notificationId, Guid userId);

    Task<ApiResponse<string>> MarkAllAsReadAsync(Guid userId);
}

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;

    public NotificationService(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task CreateAsync(
        Guid userId,
        string title,
        string message,
        NotificationType type,
        string? link,
        Guid createdBy)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            Link = link,
            IsRead = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        await _notificationRepository.AddAsync(notification);
    }

    public async Task<ApiResponse<NotificationListResponse>> GetMyNotificationsAsync(Guid userId)
    {
        try
        {
            var notifications = await _notificationRepository.GetByUserIdAsync(userId);
            var unreadCount = await _notificationRepository.GetUnreadCountAsync(userId);

            var response = new NotificationListResponse
            {
                UnreadCount = unreadCount,
                Notifications = notifications.Select(x => new NotificationResponse
                {
                    Id = x.Id,
                    Title = x.Title,
                    Message = x.Message,
                    Type = x.Type.ToString(),
                    Link = x.Link,
                    IsRead = x.IsRead,
                    CreatedAt = x.CreatedAt
                }).ToList()
            };

            return new ApiResponse<NotificationListResponse>
            {
                Success = true,
                Message = "Notifications retrieved successfully.",
                Data = response
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<NotificationListResponse>
            {
                Success = false,
                Message = "Failed to retrieve notifications.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<string>> MarkAsReadAsync(Guid notificationId, Guid userId)
    {
        var result = await _notificationRepository.MarkAsReadAsync(notificationId, userId);

        return new ApiResponse<string>
        {
            Success = result,
            Message = result ? "Notification marked as read." : "Notification not found."
        };
    }

    public async Task<ApiResponse<string>> MarkAllAsReadAsync(Guid userId)
    {
        await _notificationRepository.MarkAllAsReadAsync(userId);

        return new ApiResponse<string>
        {
            Success = true,
            Message = "All notifications marked as read."
        };
    }
}
