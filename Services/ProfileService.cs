using HRM.API.Data;
using HRM.API.Repositories;
using HRM.API.Responses;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace HRM.API.Services;

public interface IProfileService
{
    Task<ApiResponse<MyProfileResponse>> GetMyProfileAsync(Guid userId);
    Task<ApiResponse<MyProfileResponse>> UpdateMyProfileAsync(Guid userId, UpdateMyProfileDto request);
    Task<ApiResponse<MyProfileResponse>> UpdateMyProfilePictureAsync(Guid userId, IFormFile file);
    Task<ApiResponse<string>> ChangePasswordAsync(Guid userId, ChangePasswordDto request);
    Task<ApiResponse<MyProfileResponse>> UpdateTwoFactorPreferenceAsync(Guid userId, UpdateTwoFactorPreferenceDto request);
}

public class ProfileService : IProfileService
{
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    private const long MaxFileSizeBytes = 2 * 1024 * 1024; // 2 MB

    private readonly IEmployeeRepository _employeeRepository;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly ApplicationDbContext _context;

    public ProfileService(
        IEmployeeRepository employeeRepository,
        IWebHostEnvironment webHostEnvironment,
        ApplicationDbContext context)
    {
        _employeeRepository = employeeRepository;
        _webHostEnvironment = webHostEnvironment;
        _context = context;
    }

    private static MyProfileResponse MapProfile(Models.Employee employee)
    {
        return new MyProfileResponse
        {
            UserId = employee.UserId,
            EmployeeId = employee.Id,
            FirstName = employee.User.FirstName,
            LastName = employee.User.LastName,
            Email = employee.User.Email,
            PhoneNumber = employee.PhoneNumber,
            EmployeeCode = employee.EmployeeCode,
            DepartmentName = employee.Department.DepartmentName,
            DesignationName = employee.Designation.DesignationName,
            JoiningDate = employee.JoiningDate,
            ProfilePictureUrl = employee.User.ProfilePictureUrl,
            Role = employee.User.Role.ToString(),
            ManagerName = employee.Manager == null
                ? null
                : $"{employee.Manager.User.FirstName} {employee.Manager.User.LastName}",
            CompanyName = employee.User.Company.CompanyName,
            TwoFactorEnabled = employee.User.TwoFactorEnabled,
            TwoFactorMandatory = employee.User.Company.TwoFactorMandatory
        };
    }

    public async Task<ApiResponse<MyProfileResponse>> GetMyProfileAsync(Guid userId)
    {
        try
        {
            var employee = await _employeeRepository.GetProfileByUserIdAsync(userId);

            if (employee == null)
            {
                return new ApiResponse<MyProfileResponse>
                {
                    Success = false,
                    Message = "Employee profile not found."
                };
            }

            return new ApiResponse<MyProfileResponse>
            {
                Success = true,
                Message = "Profile retrieved successfully.",
                Data = MapProfile(employee)
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<MyProfileResponse>
            {
                Success = false,
                Message = "Error while retrieving profile.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<MyProfileResponse>> UpdateMyProfileAsync(Guid userId, UpdateMyProfileDto request)
    {
        try
        {
            var employee = await _employeeRepository.GetProfileByUserIdAsync(userId);

            if (employee == null)
            {
                return new ApiResponse<MyProfileResponse>
                {
                    Success = false,
                    Message = "Employee profile not found."
                };
            }

            if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
            {
                return new ApiResponse<MyProfileResponse>
                {
                    Success = false,
                    Message = "First name and last name are required."
                };
            }

            employee.User.FirstName = request.FirstName.Trim();
            employee.User.LastName = request.LastName.Trim();
            employee.User.UpdatedAt = DateTime.UtcNow;
            employee.User.UpdatedBy = userId;

            employee.PhoneNumber = request.PhoneNumber;
            employee.UpdatedAt = DateTime.UtcNow;
            employee.UpdatedBy = userId;

            await _employeeRepository.SaveChangesAsync();

            return new ApiResponse<MyProfileResponse>
            {
                Success = true,
                Message = "Profile updated successfully.",
                Data = MapProfile(employee)
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<MyProfileResponse>
            {
                Success = false,
                Message = "Error while updating profile.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<MyProfileResponse>> UpdateMyProfilePictureAsync(Guid userId, IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return new ApiResponse<MyProfileResponse> { Success = false, Message = "Please choose an image to upload." };
            }

            if (file.Length > MaxFileSizeBytes)
            {
                return new ApiResponse<MyProfileResponse> { Success = false, Message = "Image must be 2 MB or smaller." };
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!AllowedExtensions.Contains(extension))
            {
                return new ApiResponse<MyProfileResponse> { Success = false, Message = "Only JPG, PNG, and WEBP images are allowed." };
            }

            var employee = await _employeeRepository.GetProfileByUserIdAsync(userId);

            if (employee == null)
            {
                return new ApiResponse<MyProfileResponse> { Success = false, Message = "Employee profile not found." };
            }

            var uploadsFolder = Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot", "uploads", "profile-pictures");
            Directory.CreateDirectory(uploadsFolder);

            // Remove any previous picture for this user (regardless of extension) before saving the new one.
            foreach (var existingFile in Directory.GetFiles(uploadsFolder, $"{userId}.*"))
            {
                File.Delete(existingFile);
            }

            var fileName = $"{userId}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            employee.User.ProfilePictureUrl = $"/uploads/profile-pictures/{fileName}";
            employee.User.UpdatedAt = DateTime.UtcNow;
            employee.User.UpdatedBy = userId;

            await _employeeRepository.SaveChangesAsync();

            return new ApiResponse<MyProfileResponse>
            {
                Success = true,
                Message = "Profile picture updated successfully.",
                Data = MapProfile(employee)
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<MyProfileResponse>
            {
                Success = false,
                Message = "Error while updating profile picture.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<string>> ChangePasswordAsync(Guid userId, ChangePasswordDto request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6)
            {
                return new ApiResponse<string> { Success = false, Message = "New password must be at least 6 characters." };
            }

            var employee = await _employeeRepository.GetProfileByUserIdAsync(userId);

            if (employee == null)
            {
                return new ApiResponse<string> { Success = false, Message = "Employee profile not found." };
            }

            var currentPasswordValid = BCrypt.Net.BCrypt.Verify(
                request.CurrentPassword,
                employee.User.PasswordHash);

            if (!currentPasswordValid)
            {
                return new ApiResponse<string> { Success = false, Message = "Current password is incorrect." };
            }

            if (request.CurrentPassword == request.NewPassword)
            {
                return new ApiResponse<string> { Success = false, Message = "New password must be different from the current password." };
            }

            employee.User.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            employee.User.UpdatedAt = DateTime.UtcNow;
            employee.User.UpdatedBy = userId;

            // Force re-login on other devices/sessions after a password change.
            var activeRefreshTokens = await _context.RefreshTokens
                .Where(x => x.UserId == userId && x.RevokedAt == null)
                .ToListAsync();

            foreach (var refreshToken in activeRefreshTokens)
            {
                refreshToken.RevokedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return new ApiResponse<string> { Success = true, Message = "Password changed successfully." };
        }
        catch (Exception ex)
        {
            return new ApiResponse<string>
            {
                Success = false,
                Message = "Error while changing password.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<MyProfileResponse>> UpdateTwoFactorPreferenceAsync(Guid userId, UpdateTwoFactorPreferenceDto request)
    {
        try
        {
            var employee = await _employeeRepository.GetProfileByUserIdAsync(userId);

            if (employee == null)
            {
                return new ApiResponse<MyProfileResponse> { Success = false, Message = "Employee profile not found." };
            }

            if (!request.Enabled && employee.User.Company.TwoFactorMandatory)
            {
                return new ApiResponse<MyProfileResponse>
                {
                    Success = false,
                    Message = "Two-step verification is mandatory for your company and cannot be disabled."
                };
            }

            employee.User.TwoFactorEnabled = request.Enabled;
            employee.User.UpdatedAt = DateTime.UtcNow;
            employee.User.UpdatedBy = userId;

            await _employeeRepository.SaveChangesAsync();

            return new ApiResponse<MyProfileResponse>
            {
                Success = true,
                Message = request.Enabled
                    ? "Two-step verification has been enabled."
                    : "Two-step verification has been disabled.",
                Data = MapProfile(employee)
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<MyProfileResponse>
            {
                Success = false,
                Message = "Error while updating two-step verification preference.",
                Errors = new List<string> { ex.Message }
            };
        }
    }
}
