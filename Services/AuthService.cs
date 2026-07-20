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
    Task<(ApiResponse<LoginResponse> Response, string? RefreshToken)> RefreshTokenAsync(string? refreshToken);
    Task LogoutAsync(string? refreshToken);
}
public class AuthService : IAuthService
{
    private const int RefreshTokenExpiryDays = 7;

    private readonly ApplicationDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly IAuthRepository _authRepository;

    public AuthService(ApplicationDbContext context,IJwtService jwtService,IAuthRepository authRepository)
    {
        _context = context;
        _jwtService=jwtService;
        _authRepository=authRepository;
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
}