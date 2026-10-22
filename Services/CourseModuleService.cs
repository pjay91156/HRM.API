using System.Security.Cryptography;
using HRM.API.Models;
using HRM.API.Repositories;
using HRM.API.Responses;
using Microsoft.AspNetCore.DataProtection;

namespace HRM.API.Services;

public interface ICourseModuleService
{
    Task<ApiResponse<List<CourseModuleResponse>>> GetAllAsync(Guid courseId, Guid companyId);

    Task<ApiResponse<CourseModuleResponse>> GetByIdAsync(Guid courseId, Guid id, Guid companyId);

    Task<ApiResponse<Guid>> CreateAsync(Guid courseId, CreateCourseModuleDto request, Guid companyId, Guid userId);

    Task<ApiResponse<bool>> UpdateAsync(Guid courseId, Guid id, UpdateCourseModuleDto request, Guid companyId, Guid userId);

    Task<ApiResponse<bool>> SetActiveAsync(Guid courseId, Guid id, bool isActive, Guid companyId, Guid userId);

    Task<ApiResponse<bool>> ReorderAsync(Guid courseId, ReorderCourseModulesDto request, Guid companyId, Guid userId);

    Task<ApiResponse<bool>> UploadVideoAsync(Guid courseId, Guid id, IFormFile? video, Guid companyId, Guid userId);

    Task<ApiResponse<bool>> RemoveVideoAsync(Guid courseId, Guid id, Guid companyId, Guid userId);

    Task<ApiResponse<string>> CreateVideoTokenAsync(Guid courseId, Guid id, Guid companyId);

    Task<ApiResponse<CourseModuleVideoStream>> OpenVideoAsync(Guid courseId, Guid id, string? token);
}

public class CourseModuleService : ICourseModuleService
{
    public const string CourseNotFoundMessage = CourseService.NotFoundMessage;
    public const string ModuleNotFoundMessage = "Module not found.";

    // Hard ceiling for a single video upload; the effective limit is FileStorage:MaxVideoSizeMb.
    public const long MaxVideoUploadCeilingBytes = 1024L * 1024 * 1024; // 1 GB

    private const string InactiveCourseMessage = "This course is inactive. Activate the course before changing its modules.";
    private const string NoLearningContentMessage = "An active module needs text content or a video. Add one of them or deactivate the module first.";

    private const int MaxTitleLength = 200;
    private const int MaxDescriptionLength = 2000;
    private const int MaxContentLength = 100000;
    private const int DefaultMaxVideoSizeMb = 500;
    private const string MaxVideoSizeKey = "FileStorage:MaxVideoSizeMb";

    // How long a playback link stays valid; long enough to finish watching a video.
    private static readonly TimeSpan VideoTokenLifetime = TimeSpan.FromHours(4);

    private static readonly Dictionary<string, string> AllowedVideoTypes = new()
    {
        { ".mp4", "video/mp4" },
        { ".webm", "video/webm" }
    };

    private readonly ICourseModuleRepository _moduleRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IFileStorageService _fileStorage;
    private readonly ITimeLimitedDataProtector _videoTokenProtector;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CourseModuleService> _logger;

    public CourseModuleService(
        ICourseModuleRepository moduleRepository,
        ICourseRepository courseRepository,
        IFileStorageService fileStorage,
        IDataProtectionProvider dataProtectionProvider,
        IConfiguration configuration,
        ILogger<CourseModuleService> logger)
    {
        _moduleRepository = moduleRepository;
        _courseRepository = courseRepository;
        _fileStorage = fileStorage;
        _videoTokenProtector = dataProtectionProvider
            .CreateProtector("HRM.API.CourseModuleVideo")
            .ToTimeLimitedDataProtector();
        _configuration = configuration;
        _logger = logger;
    }

    private static CourseModuleResponse ToResponse(CourseModule module)
    {
        return new CourseModuleResponse
        {
            Id = module.Id,
            CourseId = module.CourseId,
            Title = module.Title,
            Description = module.Description,
            Content = module.Content,
            HasVideo = module.VideoPath != null,
            VideoFileName = module.VideoPath == null ? null : Path.GetFileName(module.VideoPath),
            SortOrder = module.SortOrder,
            IsActive = module.IsActive,
            CreatedAt = module.CreatedAt,
            UpdatedAt = module.UpdatedAt
        };
    }

    private static ApiResponse<T> Fail<T>(string message)
    {
        return new ApiResponse<T> { Success = false, Message = message };
    }

    public async Task<ApiResponse<List<CourseModuleResponse>>> GetAllAsync(Guid courseId, Guid companyId)
    {
        try
        {
            var course = await _courseRepository.GetByIdAsync(courseId, companyId);

            if (course == null)
            {
                return Fail<List<CourseModuleResponse>>(CourseNotFoundMessage);
            }

            var modules = await _moduleRepository.GetByCourseAsync(courseId, companyId);

            return new ApiResponse<List<CourseModuleResponse>>
            {
                Success = true,
                Message = "Modules retrieved successfully.",
                Data = modules.Select(ToResponse).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving modules of course {CourseId}", courseId);

            return Fail<List<CourseModuleResponse>>("An error occurred while retrieving modules.");
        }
    }

    public async Task<ApiResponse<CourseModuleResponse>> GetByIdAsync(Guid courseId, Guid id, Guid companyId)
    {
        try
        {
            var module = await _moduleRepository.GetByIdAsync(id, courseId, companyId);

            if (module == null)
            {
                return Fail<CourseModuleResponse>(ModuleNotFoundMessage);
            }

            return new ApiResponse<CourseModuleResponse>
            {
                Success = true,
                Message = "Module retrieved successfully.",
                Data = ToResponse(module)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving module {ModuleId}", id);

            return Fail<CourseModuleResponse>("An error occurred while retrieving the module.");
        }
    }

    public async Task<ApiResponse<Guid>> CreateAsync(Guid courseId, CreateCourseModuleDto request, Guid companyId, Guid userId)
    {
        try
        {
            var courseError = await ValidateCourseForChangeAsync(courseId, companyId);

            if (courseError != null)
            {
                return Fail<Guid>(courseError);
            }

            var fieldsError = ValidateFields(
                request.Title, request.Description, request.Content,
                out var title, out var description, out var content);

            if (fieldsError != null)
            {
                return Fail<Guid>(fieldsError);
            }

            string? extension = null;

            if (request.Video != null)
            {
                var videoError = ValidateVideo(request.Video, out extension);

                if (videoError != null)
                {
                    return Fail<Guid>(videoError);
                }
            }

            var moduleId = Guid.NewGuid();
            string? videoPath = null;

            if (request.Video != null)
            {
                videoPath = await SaveVideoAsync(request.Video, extension!, companyId, courseId, moduleId);
            }

            // A module without any learning content starts inactive until content is added.
            var isActive = content != null || videoPath != null;

            var module = new CourseModule
            {
                Id = moduleId,
                CourseId = courseId,
                Title = title,
                Description = description,
                Content = content,
                VideoPath = videoPath,
                SortOrder = await _moduleRepository.GetMaxSortOrderAsync(courseId) + 1,
                IsActive = isActive,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            try
            {
                await _moduleRepository.AddAsync(module);

                await _moduleRepository.SaveChangesAsync();
            }
            catch
            {
                _fileStorage.Delete(videoPath);
                throw;
            }

            return new ApiResponse<Guid>
            {
                Success = true,
                Message = isActive
                    ? "Module created successfully."
                    : "Module created as inactive. Add text content or a video to activate it.",
                Data = module.Id
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating module for course {CourseId}", courseId);

            return Fail<Guid>("An error occurred while creating the module.");
        }
    }

    public async Task<ApiResponse<bool>> UpdateAsync(Guid courseId, Guid id, UpdateCourseModuleDto request, Guid companyId, Guid userId)
    {
        try
        {
            var courseError = await ValidateCourseForChangeAsync(courseId, companyId);

            if (courseError != null)
            {
                return Fail<bool>(courseError);
            }

            var module = await _moduleRepository.GetByIdAsync(id, courseId, companyId);

            if (module == null)
            {
                return Fail<bool>(ModuleNotFoundMessage);
            }

            var fieldsError = ValidateFields(
                request.Title, request.Description, request.Content,
                out var title, out var description, out var content);

            if (fieldsError != null)
            {
                return Fail<bool>(fieldsError);
            }

            string? extension = null;

            if (request.Video != null)
            {
                var videoError = ValidateVideo(request.Video, out extension);

                if (videoError != null)
                {
                    return Fail<bool>(videoError);
                }
            }

            var keepsVideo = request.Video != null || (module.VideoPath != null && !request.RemoveVideo);

            if (module.IsActive && content == null && !keepsVideo)
            {
                return Fail<bool>(NoLearningContentMessage);
            }

            string? newVideoPath = null;

            if (request.Video != null)
            {
                newVideoPath = await SaveVideoAsync(request.Video, extension!, companyId, courseId, module.Id);
            }

            var oldVideoPath = module.VideoPath;

            module.Title = title;
            module.Description = description;
            module.Content = content;
            module.UpdatedAt = DateTime.UtcNow;
            module.UpdatedBy = userId;

            if (newVideoPath != null)
            {
                module.VideoPath = newVideoPath;
            }
            else if (request.RemoveVideo)
            {
                module.VideoPath = null;
            }

            await SaveAndSwapVideoAsync(module, oldVideoPath, newVideoPath);

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Module updated successfully.",
                Data = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating module {ModuleId}", id);

            return Fail<bool>("An error occurred while updating the module.");
        }
    }

    public async Task<ApiResponse<bool>> SetActiveAsync(Guid courseId, Guid id, bool isActive, Guid companyId, Guid userId)
    {
        try
        {
            var courseError = await ValidateCourseForChangeAsync(courseId, companyId);

            if (courseError != null)
            {
                return Fail<bool>(courseError);
            }

            var module = await _moduleRepository.GetByIdAsync(id, courseId, companyId);

            if (module == null)
            {
                return Fail<bool>(ModuleNotFoundMessage);
            }

            if (module.IsActive == isActive)
            {
                return Fail<bool>(isActive ? "Module is already active." : "Module is already inactive.");
            }

            if (isActive && module.Content == null && module.VideoPath == null)
            {
                return Fail<bool>("Add text content or a video before activating this module.");
            }

            module.IsActive = isActive;
            module.UpdatedAt = DateTime.UtcNow;
            module.UpdatedBy = userId;

            _moduleRepository.Update(module);

            await _moduleRepository.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Message = isActive ? "Module activated successfully." : "Module deactivated successfully.",
                Data = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing active state of module {ModuleId}", id);

            return Fail<bool>("An error occurred while updating the module.");
        }
    }

    public async Task<ApiResponse<bool>> ReorderAsync(Guid courseId, ReorderCourseModulesDto request, Guid companyId, Guid userId)
    {
        try
        {
            var courseError = await ValidateCourseForChangeAsync(courseId, companyId);

            if (courseError != null)
            {
                return Fail<bool>(courseError);
            }

            var modules = await _moduleRepository.GetByCourseAsync(courseId, companyId);
            var orderedIds = request.ModuleIds ?? new List<Guid>();

            // The list must name every module of this course exactly once, so no
            // module from another course can be touched and no gaps are left.
            if (orderedIds.Count != modules.Count
                || orderedIds.Distinct().Count() != orderedIds.Count
                || orderedIds.Any(moduleId => modules.All(x => x.Id != moduleId)))
            {
                return Fail<bool>("The module order is out of date. Reload the modules and try again.");
            }

            var now = DateTime.UtcNow;

            for (var index = 0; index < orderedIds.Count; index++)
            {
                var module = modules.First(x => x.Id == orderedIds[index]);
                var sortOrder = index + 1;

                if (module.SortOrder == sortOrder)
                {
                    continue;
                }

                module.SortOrder = sortOrder;
                module.UpdatedAt = now;
                module.UpdatedBy = userId;

                _moduleRepository.Update(module);
            }

            await _moduleRepository.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Module order updated successfully.",
                Data = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reordering modules of course {CourseId}", courseId);

            return Fail<bool>("An error occurred while reordering the modules.");
        }
    }

    public async Task<ApiResponse<bool>> UploadVideoAsync(Guid courseId, Guid id, IFormFile? video, Guid companyId, Guid userId)
    {
        try
        {
            var courseError = await ValidateCourseForChangeAsync(courseId, companyId);

            if (courseError != null)
            {
                return Fail<bool>(courseError);
            }

            var module = await _moduleRepository.GetByIdAsync(id, courseId, companyId);

            if (module == null)
            {
                return Fail<bool>(ModuleNotFoundMessage);
            }

            if (video == null)
            {
                return Fail<bool>("Please choose a video to upload.");
            }

            var videoError = ValidateVideo(video, out var extension);

            if (videoError != null)
            {
                return Fail<bool>(videoError);
            }

            var newVideoPath = await SaveVideoAsync(video, extension, companyId, courseId, module.Id);
            var oldVideoPath = module.VideoPath;

            module.VideoPath = newVideoPath;
            module.UpdatedAt = DateTime.UtcNow;
            module.UpdatedBy = userId;

            await SaveAndSwapVideoAsync(module, oldVideoPath, newVideoPath);

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Module video uploaded successfully.",
                Data = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading video for module {ModuleId}", id);

            return Fail<bool>("An error occurred while uploading the video.");
        }
    }

    public async Task<ApiResponse<bool>> RemoveVideoAsync(Guid courseId, Guid id, Guid companyId, Guid userId)
    {
        try
        {
            var courseError = await ValidateCourseForChangeAsync(courseId, companyId);

            if (courseError != null)
            {
                return Fail<bool>(courseError);
            }

            var module = await _moduleRepository.GetByIdAsync(id, courseId, companyId);

            if (module == null)
            {
                return Fail<bool>(ModuleNotFoundMessage);
            }

            if (module.VideoPath == null)
            {
                return Fail<bool>("This module has no video.");
            }

            if (module.IsActive && module.Content == null)
            {
                return Fail<bool>(NoLearningContentMessage);
            }

            var oldVideoPath = module.VideoPath;

            module.VideoPath = null;
            module.UpdatedAt = DateTime.UtcNow;
            module.UpdatedBy = userId;

            await SaveAndSwapVideoAsync(module, oldVideoPath, null);

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Module video removed successfully.",
                Data = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing video of module {ModuleId}", id);

            return Fail<bool>("An error occurred while removing the video.");
        }
    }

    public async Task<ApiResponse<string>> CreateVideoTokenAsync(Guid courseId, Guid id, Guid companyId)
    {
        try
        {
            var module = await _moduleRepository.GetByIdAsync(id, courseId, companyId);

            if (module == null)
            {
                return Fail<string>(ModuleNotFoundMessage);
            }

            if (module.VideoPath == null)
            {
                return Fail<string>("This module has no video.");
            }

            return new ApiResponse<string>
            {
                Success = true,
                Message = "Video link created successfully.",
                Data = _videoTokenProtector.Protect($"{companyId}|{courseId}|{id}", VideoTokenLifetime)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating video link for module {ModuleId}", id);

            return Fail<string>("An error occurred while preparing the video.");
        }
    }

    // The token was issued to an authorized user of the owning company and names one module,
    // so the browser's video player can stream it without sending the Authorization header.
    public async Task<ApiResponse<CourseModuleVideoStream>> OpenVideoAsync(Guid courseId, Guid id, string? token)
    {
        try
        {
            if (!TryReadVideoToken(token, out var companyId, out var tokenCourseId, out var tokenModuleId)
                || tokenCourseId != courseId
                || tokenModuleId != id)
            {
                return Fail<CourseModuleVideoStream>("The video link is invalid or has expired.");
            }

            var module = await _moduleRepository.GetByIdAsync(id, courseId, companyId);

            if (module?.VideoPath == null
                || !AllowedVideoTypes.TryGetValue(Path.GetExtension(module.VideoPath).ToLowerInvariant(), out var contentType))
            {
                return Fail<CourseModuleVideoStream>("Module video not found.");
            }

            var stream = _fileStorage.OpenRead(module.VideoPath);

            if (stream == null)
            {
                return Fail<CourseModuleVideoStream>("Module video not found.");
            }

            return new ApiResponse<CourseModuleVideoStream>
            {
                Success = true,
                Message = "Module video retrieved successfully.",
                Data = new CourseModuleVideoStream { Stream = stream, ContentType = contentType }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error opening video of module {ModuleId}", id);

            return Fail<CourseModuleVideoStream>("An error occurred while retrieving the video.");
        }
    }

    private bool TryReadVideoToken(string? token, out Guid companyId, out Guid courseId, out Guid moduleId)
    {
        companyId = courseId = moduleId = Guid.Empty;

        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        try
        {
            var parts = _videoTokenProtector.Unprotect(token).Split('|');

            return parts.Length == 3
                && Guid.TryParse(parts[0], out companyId)
                && Guid.TryParse(parts[1], out courseId)
                && Guid.TryParse(parts[2], out moduleId);
        }
        catch (Exception ex) when (ex is CryptographicException or FormatException)
        {
            // Tampered, malformed or expired token.
            return false;
        }
    }

    // Returns an error message when the course is missing, belongs to another company or is inactive.
    private async Task<string?> ValidateCourseForChangeAsync(Guid courseId, Guid companyId)
    {
        var course = await _courseRepository.GetByIdAsync(courseId, companyId);

        if (course == null)
        {
            return CourseNotFoundMessage;
        }

        if (!course.IsActive)
        {
            return InactiveCourseMessage;
        }

        return null;
    }

    // Saves the module, then removes the video it no longer points at.
    // When saving fails, the newly stored video is removed instead.
    private async Task SaveAndSwapVideoAsync(CourseModule module, string? oldVideoPath, string? newVideoPath)
    {
        try
        {
            _moduleRepository.Update(module);

            await _moduleRepository.SaveChangesAsync();
        }
        catch
        {
            _fileStorage.Delete(newVideoPath);
            throw;
        }

        if (oldVideoPath != null && oldVideoPath != module.VideoPath)
        {
            _fileStorage.Delete(oldVideoPath);
        }
    }

    // Saves to {companyId}/courses/{courseId}/modules/{moduleId}/ and returns the relative path stored on the module.
    private async Task<string> SaveVideoAsync(IFormFile file, string extension, Guid companyId, Guid courseId, Guid moduleId)
    {
        // The random digits keep two uploads in the same second from colliding.
        var fileName = $"video_{DateTime.UtcNow:yyyyMMddHHmmss}_{Random.Shared.Next(100000, 1000000)}{extension}";
        var relativePath = $"{companyId}/courses/{courseId}/modules/{moduleId}/{fileName}";

        await _fileStorage.SaveAsync(file, relativePath);

        return relativePath;
    }

    private long GetMaxVideoSizeBytes()
    {
        var maxSizeMb = _configuration.GetValue<int?>(MaxVideoSizeKey) ?? DefaultMaxVideoSizeMb;

        if (maxSizeMb <= 0)
        {
            maxSizeMb = DefaultMaxVideoSizeMb;
        }

        return Math.Min(maxSizeMb * 1024L * 1024, MaxVideoUploadCeilingBytes);
    }

    private static string? ValidateFields(
        string? title, string? description, string? content,
        out string trimmedTitle, out string? trimmedDescription, out string? trimmedContent)
    {
        trimmedTitle = title?.Trim() ?? string.Empty;
        trimmedDescription = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        trimmedContent = string.IsNullOrWhiteSpace(content) ? null : content.Trim();

        if (string.IsNullOrEmpty(trimmedTitle))
        {
            return "Module title is required and cannot contain only spaces.";
        }

        if (trimmedTitle.Length > MaxTitleLength)
        {
            return $"Module title cannot exceed {MaxTitleLength} characters.";
        }

        if (trimmedDescription != null && trimmedDescription.Length > MaxDescriptionLength)
        {
            return $"Description cannot exceed {MaxDescriptionLength} characters.";
        }

        if (trimmedContent != null && trimmedContent.Length > MaxContentLength)
        {
            return $"Content cannot exceed {MaxContentLength} characters.";
        }

        return null;
    }

    private string? ValidateVideo(IFormFile file, out string extension)
    {
        extension = string.Empty;

        if (file.Length == 0)
        {
            return "The selected video is empty.";
        }

        var maxSizeBytes = GetMaxVideoSizeBytes();

        if (file.Length > maxSizeBytes)
        {
            return $"Video must be {maxSizeBytes / (1024 * 1024)} MB or smaller.";
        }

        extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!AllowedVideoTypes.TryGetValue(extension, out var contentType)
            || !string.Equals(file.ContentType?.Split(';')[0].Trim(), contentType, StringComparison.OrdinalIgnoreCase)
            || !HasValidSignature(file, extension))
        {
            return "Only MP4 and WEBM videos are allowed.";
        }

        return null;
    }

    // Checks the leading bytes so a renamed file cannot pass as a video.
    private static bool HasValidSignature(IFormFile file, string extension)
    {
        var header = new byte[12];
        int read;

        using (var stream = file.OpenReadStream())
        {
            read = stream.ReadAtLeast(header, header.Length, throwOnEndOfStream: false);
        }

        if (read < header.Length)
        {
            return false;
        }

        return extension switch
        {
            ".mp4" => header.AsSpan(4, 4).SequenceEqual("ftyp"u8),
            ".webm" => header.AsSpan(0, 4).SequenceEqual(new byte[] { 0x1A, 0x45, 0xDF, 0xA3 }),
            _ => false
        };
    }
}
