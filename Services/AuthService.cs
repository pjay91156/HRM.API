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
    Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequestDto request);
}
public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly IAuthRepository _authRepository;

    public AuthService(ApplicationDbContext context,IJwtService jwtService,IAuthRepository authRepository)
    {
        _context = context;
        _jwtService=jwtService;
        _authRepository=authRepository;
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
    public async Task<ApiResponse<LoginResponse>> LoginAsync(
    LoginRequestDto request)
{
    var user = await _authRepository.GetUserByEmailAsync(request.Email);

    if (user == null)
    {
        return new ApiResponse<LoginResponse>
        {
            Success = false,
            Message = "Invalid email or password"
        };
    }

    var passwordValid = BCrypt.Net.BCrypt.Verify(
        request.Password,
        user.PasswordHash);

    if (!passwordValid)
    {
        return new ApiResponse<LoginResponse>
        {
            Success = false,
            Message = "Invalid email or password"
        };
    }

    var token = _jwtService.GenerateToken(user);

    return new ApiResponse<LoginResponse>
    {
        Success = true,
        Message = "Login successful",
        Data = new LoginResponse
        {
            Token = token,
            FirstName = user.FirstName,
            Email = user.Email
        }
    };
}
}