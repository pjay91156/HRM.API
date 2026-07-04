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

        return new Guid("29BE1D7B-444E-4481-83AA-98A1235595C9");
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