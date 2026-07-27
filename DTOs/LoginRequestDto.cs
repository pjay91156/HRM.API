public class LoginRequestDto
{
    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}

public class VerifyTwoFactorRequestDto
{
    public string LoginToken { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;
}

public class ResendTwoFactorRequestDto
{
    public string LoginToken { get; set; } = string.Empty;
}