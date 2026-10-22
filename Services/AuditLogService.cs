using System.Text.Json;
using HRM.API.Models;
using HRM.API.Repositories;
using HRM.API.Responses;
using Microsoft.AspNetCore.Http;

namespace HRM.API.Services;

public interface IAuditLogService
{
    Task LogAsync(AuditLogEntryDto entry);
    Task<ApiResponse<AuditLogPagedResponse>> GetPagedAsync(Guid companyId, AuditLogFilterDto filter);
    Task<ApiResponse<AuditLogDetailResponse>> GetByIdAsync(Guid id, Guid companyId);
    Task<ApiResponse<AuditLogSummaryResponse>> GetSummaryAsync(Guid companyId);
}

public class AuditLogService : IAuditLogService
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuditLogService> _logger;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public AuditLogService(
        IAuditLogRepository auditLogRepository,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AuditLogService> logger)
    {
        _auditLogRepository = auditLogRepository;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task LogAsync(AuditLogEntryDto entry)
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;

            var log = new AuditLog
            {
                Id = Guid.NewGuid(),
                CompanyId = entry.CompanyId,
                UserId = entry.UserId,
                UserName = entry.UserName,
                Action = entry.Action,
                Module = entry.Module,
                EntityType = entry.EntityType,
                EntityId = entry.EntityId,
                TargetUserId = entry.TargetUserId,
                TargetUserName = entry.TargetUserName,
                Description = entry.Description,
                Status = entry.Status,
                OldValues = entry.OldValues == null ? null : JsonSerializer.Serialize(entry.OldValues, SerializerOptions),
                NewValues = entry.NewValues == null ? null : JsonSerializer.Serialize(entry.NewValues, SerializerOptions),
                IpAddress = httpContext?.Connection.RemoteIpAddress?.ToString(),
                UserAgent = httpContext?.Request.Headers.UserAgent.ToString(),
                CreatedAt = DateTime.UtcNow
            };

            await _auditLogRepository.AddAsync(log);
        }
        catch (Exception ex)
        {
            // Audit logging must never break the business operation it's attached to.
            _logger.LogError(ex, "Failed to write audit log for {Module}/{Action}", entry.Module, entry.Action);
        }
    }

    public async Task<ApiResponse<AuditLogPagedResponse>> GetPagedAsync(Guid companyId, AuditLogFilterDto filter)
    {
        try
        {
            var (totalRecords, records) = await _auditLogRepository.GetPagedAsync(companyId, filter);

            return new ApiResponse<AuditLogPagedResponse>
            {
                Success = true,
                Message = "Activity logs retrieved successfully.",
                Data = new AuditLogPagedResponse
                {
                    TotalRecords = totalRecords,
                    Records = records.Select(ToListItem).ToList()
                }
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<AuditLogPagedResponse>
            {
                Success = false,
                Message = "Failed to fetch activity logs.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<AuditLogDetailResponse>> GetByIdAsync(Guid id, Guid companyId)
    {
        try
        {
            var log = await _auditLogRepository.GetByIdAsync(id, companyId);

            if (log == null)
            {
                return new ApiResponse<AuditLogDetailResponse>
                {
                    Success = false,
                    Message = "Activity log not found."
                };
            }

            return new ApiResponse<AuditLogDetailResponse>
            {
                Success = true,
                Message = "Activity log retrieved successfully.",
                Data = new AuditLogDetailResponse
                {
                    Id = log.Id,
                    CreatedAt = log.CreatedAt,
                    UserId = log.UserId,
                    UserName = log.UserName,
                    Action = log.Action.ToString(),
                    Module = log.Module.ToString(),
                    TargetUserId = log.TargetUserId,
                    TargetUserName = log.TargetUserName,
                    Description = log.Description,
                    Status = log.Status,
                    EntityType = log.EntityType,
                    EntityId = log.EntityId,
                    IpAddress = log.IpAddress,
                    UserAgent = log.UserAgent,
                    OldValues = Deserialize(log.OldValues),
                    NewValues = Deserialize(log.NewValues)
                }
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<AuditLogDetailResponse>
            {
                Success = false,
                Message = "Failed to fetch activity log details.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<AuditLogSummaryResponse>> GetSummaryAsync(Guid companyId)
    {
        try
        {
            var (total, today, employeeChanges, leaveActivities) = await _auditLogRepository.GetSummaryAsync(companyId);

            return new ApiResponse<AuditLogSummaryResponse>
            {
                Success = true,
                Message = "Activity summary retrieved successfully.",
                Data = new AuditLogSummaryResponse
                {
                    TotalActivities = total,
                    TodayActivities = today,
                    EmployeeChanges = employeeChanges,
                    LeaveActivities = leaveActivities
                }
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<AuditLogSummaryResponse>
            {
                Success = false,
                Message = "Failed to fetch activity summary.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    private static AuditLogListItemResponse ToListItem(AuditLog log) => new()
    {
        Id = log.Id,
        CreatedAt = log.CreatedAt,
        UserId = log.UserId,
        UserName = log.UserName,
        Action = log.Action.ToString(),
        Module = log.Module.ToString(),
        TargetUserId = log.TargetUserId,
        TargetUserName = log.TargetUserName,
        Description = log.Description,
        Status = log.Status
    };

    private static Dictionary<string, string>? Deserialize(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            var raw = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);

            return raw?.ToDictionary(
                kv => kv.Key,
                kv => kv.Value.ValueKind == JsonValueKind.String ? kv.Value.GetString() ?? string.Empty : kv.Value.ToString());
        }
        catch
        {
            return null;
        }
    }
}
