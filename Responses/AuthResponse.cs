namespace HRM.API.Responses;
public class RegisterResponse
{
    public Guid CompanyId { get; set; }

    public Guid UserId { get; set; }

    public string Email { get; set; } = string.Empty;
}
public class LoginResponse
{
    public string Token { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
}