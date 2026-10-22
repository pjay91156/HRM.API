using System.Security.Claims;

namespace HRM.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (Guid.TryParse(userId, out var guid))
        {
            return guid;
        }

        throw new UnauthorizedAccessException("User Id claim not found.");
    }

    public static Guid GetCompanyId(this ClaimsPrincipal user)
    {
        var companyId = user.FindFirst("company_id")?.Value;

        if (string.IsNullOrWhiteSpace(companyId))
        {
            throw new UnauthorizedAccessException("Company Id claim not found.");
        }

        return Guid.Parse(companyId);
    }
}