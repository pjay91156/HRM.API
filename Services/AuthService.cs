using BCrypt.Net;
using HRM.API.Data;
using HRM.API.DTOs.Auth;
using HRM.API.Responses;
using HRM.API.Models;
using HRM.API.Services;
using Microsoft.EntityFrameworkCore;
using HRM.API.Repositories;

namespace HRM.API.Services;
public interface IAuthService
{
    Task<ApiResponse<RegisterResponse>> RegisterAsync(
        RegisterRequestDto request);
    Task<(ApiResponse<LoginResponse> Response, string? RefreshToken)> LoginAsync(LoginRequestDto request);
    Task<(ApiResponse<LoginResponse> Response, string? RefreshToken)> VerifyTwoFactorAsync(VerifyTwoFactorRequestDto request);
    Task<ApiResponse<string>> ResendTwoFactorCodeAsync(ResendTwoFactorRequestDto request);
    Task<(ApiResponse<LoginResponse> Response, string? RefreshToken)> RefreshTokenAsync(string? refreshToken);
    Task LogoutAsync(string? refreshToken);
    Task<ApiResponse<string>> ForgotPasswordAsync(ForgotPasswordRequestDto request);
    Task<ApiResponse<string>> ResetPasswordAsync(ResetPasswordRequestDto request);
}
public class AuthService : IAuthService
{
    private const int RefreshTokenExpiryDays = 7;
    private const int PasswordResetTokenExpiryMinutes = 30;
    private const int LoginOtpExpiryMinutes = 10;
    private const int MaxTwoFactorAttempts = 5;

    private readonly ApplicationDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly IAuthRepository _authRepository;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;

    public AuthService(
        ApplicationDbContext context,
        IJwtService jwtService,
        IAuthRepository authRepository,
        IEmailService emailService,
        IConfiguration configuration)
    {
        _context = context;
        _jwtService=jwtService;
        _authRepository=authRepository;
        _emailService = emailService;
        _configuration = configuration;
    }

    private async Task<string> IssueRefreshTokenAsync(Guid userId)
    {
        var refreshToken = _jwtService.GenerateRefreshToken();

        _context.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = _jwtService.HashToken(refreshToken),
            ExpiresAt = DateTime.UtcNow.AddDays(RefreshTokenExpiryDays),
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        return refreshToken;
    }

    public async Task<ApiResponse<RegisterResponse>> RegisterAsync(
        RegisterRequestDto request)
    {
        var exists = await _context.Users
            .AnyAsync(x => x.Email == request.Email);

        if (exists)
        {
            return new ApiResponse<RegisterResponse>
            {
                Success = false,
                Message = "Email already exists."
            };
        }

        var companyId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var company = new Company
        {
            Id = companyId,
            CompanyName = request.CompanyName,
            Email = request.Email,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        _context.Companies.Add(company);

        var user = new User
        {
            Id = userId,
            CompanyId = companyId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = Enums.UserRole.SuperAdmin,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        _context.Users.Add(user);

        await _context.SaveChangesAsync();

        return new ApiResponse<RegisterResponse>
        {
            Success = true,
            Message = "Registration successful.",
            Data = new RegisterResponse
            {
                CompanyId = companyId,
                UserId = userId,
                Email = request.Email
            }
        };
    }
    public async Task<(ApiResponse<LoginResponse> Response, string? RefreshToken)> LoginAsync(
    LoginRequestDto request)
{
    var user = await _authRepository.GetUserByEmailAsync(request.Email);

    if (user == null)
    {
        return (new ApiResponse<LoginResponse>
        {
            Success = false,
            Message = "Invalid email or password"
        }, null);
    }

    var passwordValid = BCrypt.Net.BCrypt.Verify(
        request.Password,
        user.PasswordHash);

    if (!passwordValid)
    {
        return (new ApiResponse<LoginResponse>
        {
            Success = false,
            Message = "Invalid email or password"
        }, null);
    }

    var requiresTwoFactor = user.TwoFactorEnabled || user.Company.TwoFactorMandatory;

    if (requiresTwoFactor)
    {
        var loginToken = await IssueLoginOtpAsync(user);

        return (new ApiResponse<LoginResponse>
        {
            Success = true,
            Message = "A verification code has been sent to your email.",
            Data = new LoginResponse
            {
                RequiresTwoFactor = true,
                LoginToken = loginToken
            }
        }, null);
    }

    var token = _jwtService.GenerateToken(user);
    var refreshToken = await IssueRefreshTokenAsync(user.Id);

    return (new ApiResponse<LoginResponse>
    {
        Success = true,
        Message = "Login successful",
        Data = new LoginResponse
        {
            Token = token,
            FirstName = user.FirstName,
            Email = user.Email
        }
    }, refreshToken);
}

private static string GenerateNumericCode(int length)
{
    var maxValue = (int)Math.Pow(10, length);
    var value = System.Security.Cryptography.RandomNumberGenerator.GetInt32(0, maxValue);
    return value.ToString(new string('0', length));
}

private async Task<string> IssueLoginOtpAsync(User user)
{
    var challengeToken = _jwtService.GenerateRefreshToken();
    var code = GenerateNumericCode(6);

    _context.LoginOtps.Add(new LoginOtp
    {
        Id = Guid.NewGuid(),
        UserId = user.Id,
        TokenHash = _jwtService.HashToken(challengeToken),
        CodeHash = _jwtService.HashToken(code),
        AttemptCount = 0,
        ExpiresAt = DateTime.UtcNow.AddMinutes(LoginOtpExpiryMinutes),
        CreatedAt = DateTime.UtcNow
    });

    await _context.SaveChangesAsync();

    var htmlBody = $@"
        <p>Hi {user.FirstName},</p>
        <p>Your HRM verification code is:</p>
        <h2 style=""letter-spacing: 4px;"">{code}</h2>
        <p>This code expires in {LoginOtpExpiryMinutes} minutes. If you didn't try to log in, you can ignore this email.</p>";

    await _emailService.SendAsync(user.Email, "Your HRM verification code", htmlBody);

    return challengeToken;
}

public async Task<(ApiResponse<LoginResponse> Response, string? RefreshToken)> VerifyTwoFactorAsync(
    VerifyTwoFactorRequestDto request)
{
    if (string.IsNullOrWhiteSpace(request.LoginToken) || string.IsNullOrWhiteSpace(request.Code))
    {
        return (new ApiResponse<LoginResponse> { Success = false, Message = "Verification code is required." }, null);
    }

    var tokenHash = _jwtService.HashToken(request.LoginToken);

    var otp = await _context.LoginOtps
        .Include(x => x.User)
        .FirstOrDefaultAsync(x => x.TokenHash == tokenHash);

    if (otp == null || otp.UsedAt != null || otp.ExpiresAt < DateTime.UtcNow)
    {
        return (new ApiResponse<LoginResponse> { Success = false, Message = "This verification session has expired. Please log in again." }, null);
    }

    if (otp.AttemptCount >= MaxTwoFactorAttempts)
    {
        return (new ApiResponse<LoginResponse> { Success = false, Message = "Too many incorrect attempts. Please log in again to get a new code." }, null);
    }

    var codeHash = _jwtService.HashToken(request.Code.Trim());

    if (codeHash != otp.CodeHash)
    {
        otp.AttemptCount++;
        await _context.SaveChangesAsync();

        return (new ApiResponse<LoginResponse> { Success = false, Message = "Invalid verification code." }, null);
    }

    otp.UsedAt = DateTime.UtcNow;

    var token = _jwtService.GenerateToken(otp.User);
    var refreshToken = await IssueRefreshTokenAsync(otp.UserId);

    await _context.SaveChangesAsync();

    return (new ApiResponse<LoginResponse>
    {
        Success = true,
        Message = "Login successful",
        Data = new LoginResponse
        {
            Token = token,
            FirstName = otp.User.FirstName,
            Email = otp.User.Email
        }
    }, refreshToken);
}

public async Task<ApiResponse<string>> ResendTwoFactorCodeAsync(ResendTwoFactorRequestDto request)
{
    if (string.IsNullOrWhiteSpace(request.LoginToken))
    {
        return new ApiResponse<string> { Success = false, Message = "Invalid request." };
    }

    var tokenHash = _jwtService.HashToken(request.LoginToken);

    var otp = await _context.LoginOtps
        .Include(x => x.User)
        .FirstOrDefaultAsync(x => x.TokenHash == tokenHash);

    if (otp == null || otp.UsedAt != null || otp.ExpiresAt < DateTime.UtcNow)
    {
        return new ApiResponse<string> { Success = false, Message = "This verification session has expired. Please log in again." };
    }

    var code = GenerateNumericCode(6);
    otp.CodeHash = _jwtService.HashToken(code);
    otp.AttemptCount = 0;
    otp.ExpiresAt = DateTime.UtcNow.AddMinutes(LoginOtpExpiryMinutes);

    await _context.SaveChangesAsync();

    var htmlBody = $@"
        <p>Hi {otp.User.FirstName},</p>
        <p>Your new HRM verification code is:</p>
        <h2 style=""letter-spacing: 4px;"">{code}</h2>
        <p>This code expires in {LoginOtpExpiryMinutes} minutes.</p>";

    await _emailService.SendAsync(otp.User.Email, "Your HRM verification code", htmlBody);

    return new ApiResponse<string> { Success = true, Message = "A new verification code has been sent to your email." };
}

    public async Task<(ApiResponse<LoginResponse> Response, string? RefreshToken)> RefreshTokenAsync(string? refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
        {
            return (new ApiResponse<LoginResponse> { Success = false, Message = "Refresh token is missing." }, null);
        }

        var tokenHash = _jwtService.HashToken(refreshToken);

        var existing = await _context.RefreshTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash);

        if (existing == null || existing.RevokedAt != null || existing.ExpiresAt < DateTime.UtcNow)
        {
            return (new ApiResponse<LoginResponse> { Success = false, Message = "Invalid or expired refresh token." }, null);
        }

        existing.RevokedAt = DateTime.UtcNow;

        var newAccessToken = _jwtService.GenerateToken(existing.User);
        var newRefreshToken = await IssueRefreshTokenAsync(existing.UserId);

        await _context.SaveChangesAsync();

        return (new ApiResponse<LoginResponse>
        {
            Success = true,
            Message = "Token refreshed successfully.",
            Data = new LoginResponse
            {
                Token = newAccessToken,
                FirstName = existing.User.FirstName,
                Email = existing.User.Email
            }
        }, newRefreshToken);
    }

    public async Task LogoutAsync(string? refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
        {
            return;
        }

        var tokenHash = _jwtService.HashToken(refreshToken);

        var existing = await _context.RefreshTokens
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash);

        if (existing != null && existing.RevokedAt == null)
        {
            existing.RevokedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    private const string GenericForgotPasswordMessage =
        "If an account exists for that email, a password reset link has been sent.";

    public async Task<ApiResponse<string>> ForgotPasswordAsync(ForgotPasswordRequestDto request)
    {
        var user = await _authRepository.GetUserByEmailAsync(request.Email);

        if (user == null)
        {
            // Do not reveal whether the email exists.
            return new ApiResponse<string> { Success = true, Message = GenericForgotPasswordMessage };
        }

        var unusedTokens = await _context.PasswordResetTokens
            .Where(x => x.UserId == user.Id && x.UsedAt == null)
            .ToListAsync();

        foreach (var unusedToken in unusedTokens)
        {
            unusedToken.UsedAt = DateTime.UtcNow;
        }

        var rawToken = _jwtService.GenerateRefreshToken();

        _context.PasswordResetTokens.Add(new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = _jwtService.HashToken(rawToken),
            ExpiresAt = DateTime.UtcNow.AddMinutes(PasswordResetTokenExpiryMinutes),
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        var clientBaseUrl = _configuration["ClientApp:BaseUrl"]?.TrimEnd('/') ?? string.Empty;
        var resetLink = $"{clientBaseUrl}/reset-password?token={Uri.EscapeDataString(rawToken)}";

        var htmlBody = $@"
            <p>Hi {user.FirstName},</p>
            <p>We received a request to reset your HRM account password. Click the link below to choose a new password:</p>
            <p><a href=""{resetLink}"">Reset your password</a></p>
            <p>This link expires in {PasswordResetTokenExpiryMinutes} minutes. If you didn't request this, you can safely ignore this email.</p>";

        await _emailService.SendAsync(user.Email, "Reset your HRM password", htmlBody);

        return new ApiResponse<string> { Success = true, Message = GenericForgotPasswordMessage };
    }

    public async Task<ApiResponse<string>> ResetPasswordAsync(ResetPasswordRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
        {
            return new ApiResponse<string> { Success = false, Message = "Reset token is required." };
        }

        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6)
        {
            return new ApiResponse<string> { Success = false, Message = "Password must be at least 6 characters." };
        }

        var tokenHash = _jwtService.HashToken(request.Token);

        var resetToken = await _context.PasswordResetTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash);

        if (resetToken == null || resetToken.UsedAt != null || resetToken.ExpiresAt < DateTime.UtcNow)
        {
            return new ApiResponse<string> { Success = false, Message = "Invalid or expired reset link. Please request a new one." };
        }

        resetToken.UsedAt = DateTime.UtcNow;
        resetToken.User.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        resetToken.User.UpdatedAt = DateTime.UtcNow;

        // Force re-login everywhere by revoking all active refresh tokens for this user.
        var activeRefreshTokens = await _context.RefreshTokens
            .Where(x => x.UserId == resetToken.UserId && x.RevokedAt == null)
            .ToListAsync();

        foreach (var refreshToken in activeRefreshTokens)
        {
            refreshToken.RevokedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return new ApiResponse<string> { Success = true, Message = "Password has been reset successfully. Please log in with your new password." };
    }
}