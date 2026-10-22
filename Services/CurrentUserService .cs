using System.Security.Claims;

namespace HRM.API.Services;
public interface ICurrentUserService
{
    Guid UserId { get; }

    string? Email { get; }

    string? UserName { get; }
}

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            var userId = _httpContextAccessor.HttpContext?
                .User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return Guid.Parse(userId!);
        }
    }

    public string? Email =>
        _httpContextAccessor.HttpContext?
            .User
            .FindFirst(ClaimTypes.Email)?.Value;

    public string? UserName =>
        _httpContextAccessor.HttpContext?
            .User
            .Identity?
            .Name;
}