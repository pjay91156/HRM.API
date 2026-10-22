using HRM.API.Enums;
using HRM.API.Models;
using HRM.API.Repositories;
using HRM.API.Responses;

namespace HRM.API.Services;

public interface ICourseService
{
    Task<ApiResponse<List<CourseResponse>>> GetAllAsync(Guid companyId);

    Task<ApiResponse<CourseResponse>> GetByIdAsync(Guid id, Guid companyId);

    Task<ApiResponse<Guid>> CreateAsync(CreateCourseDto request, Guid companyId, Guid userId);

    Task<ApiResponse<bool>> UpdateAsync(UpdateCourseDto request, Guid companyId, Guid userId);

    Task<ApiResponse<bool>> SetActiveAsync(Guid id, bool isActive, Guid companyId, Guid userId);

    Task<ApiResponse<CourseThumbnailFile>> GetThumbnailAsync(Guid id, Guid companyId);
}

public class CourseService : ICourseService
{
    public const string NotFoundMessage = "Course not found.";

    private const int MaxNameLength = 200;
    private const int MaxDescriptionLength = 2000;
    private const long MaxThumbnailSizeBytes = 2 * 1024 * 1024; // 2 MB

    // Stored in the folder configured in appsettings.json, outside wwwroot, so thumbnails are never
    // reachable via static file hosting;
    // they are only served through the authorized, company-scoped thumbnail endpoint.
    private const string StorageRootPathKey = "FileStorage:RootPath";
    private const string CourseFolder = "courses";

    private static readonly Dictionary<string, string> AllowedThumbnailTypes = new()
    {
        { ".jpg", "image/jpeg" },
        { ".jpeg", "image/jpeg" },
        { ".png", "image/png" },
        { ".webp", "image/webp" }
    };

    private readonly ICourseRepository _courseRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CourseService> _logger;

    public CourseService(
        ICourseRepository courseRepository,
        IConfiguration configuration,
        ILogger<CourseService> logger)
    {
        _courseRepository = courseRepository;
        _configuration = configuration;
        _logger = logger;
    }

    private static CourseResponse ToResponse(Course course)
    {
        return new CourseResponse
        {
            Id = course.Id,
            Name = course.Name,
            Description = course.Description,
            Thumbnail = course.Thumbnail,
            Status = course.Status,
            IsActive = course.IsActive,
            CreatedAt = course.CreatedAt,
            UpdatedAt = course.UpdatedAt
        };
    }

    public async Task<ApiResponse<List<CourseResponse>>> GetAllAsync(Guid companyId)
    {
        try
        {
            var courses = await _courseRepository.GetAllAsync(companyId);

            return new ApiResponse<List<CourseResponse>>
            {
                Success = true,
                Message = "Courses retrieved successfully.",
                Data = courses.Select(ToResponse).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving courses for company {CompanyId}", companyId);

            return new ApiResponse<List<CourseResponse>>
            {
                Success = false,
                Message = "An error occurred while retrieving courses."
            };
        }
    }

    public async Task<ApiResponse<CourseResponse>> GetByIdAsync(Guid id, Guid companyId)
    {
        try
        {
            var course = await _courseRepository.GetByIdAsync(id, companyId);

            if (course == null)
            {
                return new ApiResponse<CourseResponse> { Success = false, Message = NotFoundMessage };
            }

            return new ApiResponse<CourseResponse>
            {
                Success = true,
                Message = "Course retrieved successfully.",
                Data = ToResponse(course)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving course {CourseId}", id);

            return new ApiResponse<CourseResponse>
            {
                Success = false,
                Message = "An error occurred while retrieving the course."
            };
        }
    }

    public async Task<ApiResponse<Guid>> CreateAsync(CreateCourseDto request, Guid companyId, Guid userId)
    {
        try
        {
            var nameValidation = ValidateName(request.Name, out var trimmedName);

            if (nameValidation != null)
            {
                return new ApiResponse<Guid> { Success = false, Message = nameValidation };
            }

            var descriptionValidation = ValidateDescription(request.Description, out var trimmedDescription);

            if (descriptionValidation != null)
            {
                return new ApiResponse<Guid> { Success = false, Message = descriptionValidation };
            }

            var statusValidation = ValidateStatus(request.Status, out var status);

            if (statusValidation != null)
            {
                return new ApiResponse<Guid> { Success = false, Message = statusValidation };
            }

            if (status == CourseStatus.Archived)
            {
                return new ApiResponse<Guid>
                {
                    Success = false,
                    Message = "A new course cannot be created as Archived."
                };
            }

            string? extension = null;

            if (request.Thumbnail != null)
            {
                var thumbnailValidation = ValidateThumbnail(request.Thumbnail, out extension);

                if (thumbnailValidation != null)
                {
                    return new ApiResponse<Guid> { Success = false, Message = thumbnailValidation };
                }
            }

            var nameExists = await _courseRepository.ExistsByNameAsync(trimmedName, companyId);

            if (nameExists)
            {
                return new ApiResponse<Guid>
                {
                    Success = false,
                    Message = "A course with this name already exists."
                };
            }

            var courseId = Guid.NewGuid();
            string? thumbnailPath = null;

            if (request.Thumbnail != null)
            {
                thumbnailPath = await SaveThumbnailAsync(request.Thumbnail, extension!, companyId, courseId);
            }

            var course = new Course
            {
                Id = courseId,
                CompanyId = companyId,
                Name = trimmedName,
                Description = trimmedDescription,
                Thumbnail = thumbnailPath,
                Status = status.ToString(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            try
            {
                await _courseRepository.AddAsync(course);

                await _courseRepository.SaveChangesAsync();
            }
            catch
            {
                // Don't leave an orphaned file behind when the record could not be saved.
                DeleteThumbnailFile(thumbnailPath);
                throw;
            }

            return new ApiResponse<Guid>
            {
                Success = true,
                Message = "Course created successfully.",
                Data = course.Id
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating course for company {CompanyId}", companyId);

            return new ApiResponse<Guid>
            {
                Success = false,
                Message = "An error occurred while creating the course."
            };
        }
    }

    public async Task<ApiResponse<bool>> UpdateAsync(UpdateCourseDto request, Guid companyId, Guid userId)
    {
        try
        {
            var course = await _courseRepository.GetByIdAsync(request.Id, companyId);

            if (course == null)
            {
                return new ApiResponse<bool> { Success = false, Message = NotFoundMessage };
            }

            if (!course.IsActive)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Inactive courses cannot be edited. Activate the course first."
                };
            }

            var nameValidation = ValidateName(request.Name, out var trimmedName);

            if (nameValidation != null)
            {
                return new ApiResponse<bool> { Success = false, Message = nameValidation };
            }

            var descriptionValidation = ValidateDescription(request.Description, out var trimmedDescription);

            if (descriptionValidation != null)
            {
                return new ApiResponse<bool> { Success = false, Message = descriptionValidation };
            }

            var statusValidation = ValidateStatus(request.Status, out var status);

            if (statusValidation != null)
            {
                return new ApiResponse<bool> { Success = false, Message = statusValidation };
            }

            string? extension = null;

            if (request.Thumbnail != null)
            {
                var thumbnailValidation = ValidateThumbnail(request.Thumbnail, out extension);

                if (thumbnailValidation != null)
                {
                    return new ApiResponse<bool> { Success = false, Message = thumbnailValidation };
                }
            }

            var nameExists = await _courseRepository.ExistsByNameAsync(trimmedName, companyId, course.Id);

            if (nameExists)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "A course with this name already exists."
                };
            }

            string? newThumbnailPath = null;

            if (request.Thumbnail != null)
            {
                newThumbnailPath = await SaveThumbnailAsync(request.Thumbnail, extension!, companyId, course.Id);
            }

            var oldThumbnailPath = course.Thumbnail;

            course.Name = trimmedName;
            course.Description = trimmedDescription;
            course.Status = status.ToString();
            course.UpdatedAt = DateTime.UtcNow;
            course.UpdatedBy = userId;

            if (newThumbnailPath != null)
            {
                course.Thumbnail = newThumbnailPath;
            }
            else if (request.RemoveThumbnail)
            {
                course.Thumbnail = null;
            }

            try
            {
                _courseRepository.Update(course);

                await _courseRepository.SaveChangesAsync();
            }
            catch
            {
                DeleteThumbnailFile(newThumbnailPath);
                throw;
            }

            // The old file is removed only after the record points at the new one.
            if (oldThumbnailPath != null && oldThumbnailPath != course.Thumbnail)
            {
                DeleteThumbnailFile(oldThumbnailPath);
            }

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Course updated successfully.",
                Data = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating course {CourseId}", request.Id);

            return new ApiResponse<bool>
            {
                Success = false,
                Message = "An error occurred while updating the course."
            };
        }
    }

    public async Task<ApiResponse<bool>> SetActiveAsync(Guid id, bool isActive, Guid companyId, Guid userId)
    {
        try
        {
            var course = await _courseRepository.GetByIdAsync(id, companyId);

            if (course == null)
            {
                return new ApiResponse<bool> { Success = false, Message = NotFoundMessage };
            }

            if (course.IsActive == isActive)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = isActive ? "Course is already active." : "Course is already inactive."
                };
            }

            course.IsActive = isActive;
            course.UpdatedAt = DateTime.UtcNow;
            course.UpdatedBy = userId;

            _courseRepository.Update(course);

            await _courseRepository.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Message = isActive ? "Course activated successfully." : "Course deactivated successfully.",
                Data = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing active state of course {CourseId}", id);

            return new ApiResponse<bool>
            {
                Success = false,
                Message = "An error occurred while updating the course."
            };
        }
    }

    public async Task<ApiResponse<CourseThumbnailFile>> GetThumbnailAsync(Guid id, Guid companyId)
    {
        try
        {
            var course = await _courseRepository.GetByIdAsync(id, companyId);

            if (course == null)
            {
                return new ApiResponse<CourseThumbnailFile> { Success = false, Message = NotFoundMessage };
            }

            var absolutePath = ResolveThumbnailPath(course.Thumbnail);

            if (absolutePath == null
                || !File.Exists(absolutePath)
                || !AllowedThumbnailTypes.TryGetValue(Path.GetExtension(absolutePath).ToLowerInvariant(), out var contentType))
            {
                return new ApiResponse<CourseThumbnailFile>
                {
                    Success = false,
                    Message = "Course thumbnail not found."
                };
            }

            return new ApiResponse<CourseThumbnailFile>
            {
                Success = true,
                Message = "Course thumbnail retrieved successfully.",
                Data = new CourseThumbnailFile
                {
                    Content = await File.ReadAllBytesAsync(absolutePath),
                    ContentType = contentType
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving thumbnail of course {CourseId}", id);

            return new ApiResponse<CourseThumbnailFile>
            {
                Success = false,
                Message = "An error occurred while retrieving the course thumbnail."
            };
        }
    }

    // The upload root comes only from appsettings.json (FileStorage:RootPath) and must be an
    // absolute path, so it never depends on where the application happens to be running from.
    private string GetStorageRoot()
    {
        var rootPath = _configuration[StorageRootPathKey];

        if (string.IsNullOrWhiteSpace(rootPath) || !Path.IsPathFullyQualified(rootPath))
        {
            throw new InvalidOperationException(
                $"'{StorageRootPathKey}' must be set to an absolute folder path in appsettings.json.");
        }

        return Path.GetFullPath(rootPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

    // Returns the absolute path for a stored relative path, or null when the
    // value is empty or would resolve outside the upload root.
    private string? ResolveThumbnailPath(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return null;
        }

        var root = GetStorageRoot();
        var absolutePath = Path.GetFullPath(Path.Combine(root, relativePath));

        if (!absolutePath.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return absolutePath;
    }

    // Saves to {companyId}/courses/{courseId}/ and returns the relative path stored on the course.
    private async Task<string> SaveThumbnailAsync(IFormFile file, string extension, Guid companyId, Guid courseId)
    {
        var relativeFolder = $"{companyId}/{CourseFolder}/{courseId}";
        var folder = Path.Combine(GetStorageRoot(), companyId.ToString(), CourseFolder, courseId.ToString());
        Directory.CreateDirectory(folder);

        // The random digits keep two uploads in the same second from colliding.
        var fileName = $"thumbnail_{DateTime.UtcNow:yyyyMMddHHmmss}_{Random.Shared.Next(100000, 1000000)}{extension}";

        await using (var stream = new FileStream(Path.Combine(folder, fileName), FileMode.CreateNew))
        {
            await file.CopyToAsync(stream);
        }

        return $"{relativeFolder}/{fileName}";
    }

    private void DeleteThumbnailFile(string? relativePath)
    {
        try
        {
            var absolutePath = ResolveThumbnailPath(relativePath);

            if (absolutePath != null && File.Exists(absolutePath))
            {
                File.Delete(absolutePath);
            }
        }
        catch (Exception ex)
        {
            // Cleanup is best effort; a leftover file must not fail the request.
            _logger.LogWarning(ex, "Could not delete course thumbnail {Thumbnail}", relativePath);
        }
    }

    private static string? ValidateName(string? name, out string trimmedName)
    {
        trimmedName = name?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(trimmedName))
        {
            return "Course name is required and cannot contain only spaces.";
        }

        if (trimmedName.Length > MaxNameLength)
        {
            return $"Course name cannot exceed {MaxNameLength} characters.";
        }

        return null;
    }

    private static string? ValidateDescription(string? description, out string trimmedDescription)
    {
        trimmedDescription = description?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(trimmedDescription))
        {
            return "Description is required and cannot contain only spaces.";
        }

        if (trimmedDescription.Length > MaxDescriptionLength)
        {
            return $"Description cannot exceed {MaxDescriptionLength} characters.";
        }

        return null;
    }

    private static string? ValidateStatus(string? value, out CourseStatus status)
    {
        status = default;

        var trimmed = value?.Trim();

        // Matched against the names only; Enum.TryParse would also accept numbers and "A,B" combinations.
        var match = Enum.GetNames<CourseStatus>()
            .FirstOrDefault(x => string.Equals(x, trimmed, StringComparison.OrdinalIgnoreCase));

        if (match == null)
        {
            return "Course status must be Draft, Published or Archived.";
        }

        status = Enum.Parse<CourseStatus>(match);

        return null;
    }

    private static string? ValidateThumbnail(IFormFile file, out string extension)
    {
        extension = string.Empty;

        if (file.Length == 0)
        {
            return "The selected thumbnail is empty.";
        }

        if (file.Length > MaxThumbnailSizeBytes)
        {
            return "Thumbnail must be 2 MB or smaller.";
        }

        extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!AllowedThumbnailTypes.ContainsKey(extension) || !HasValidSignature(file, extension))
        {
            return "Only JPG, PNG, and WEBP thumbnails are allowed.";
        }

        return null;
    }

    // Checks the leading bytes so a renamed file cannot pass as a picture.
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
            ".jpg" or ".jpeg" => header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF,
            ".png" => header.AsSpan(0, 8).SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }),
            ".webp" => header.AsSpan(0, 4).SequenceEqual("RIFF"u8) && header.AsSpan(8, 4).SequenceEqual("WEBP"u8),
            _ => false
        };
    }
}
