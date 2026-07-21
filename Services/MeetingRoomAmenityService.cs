using HRM.API.Models;
using HRM.API.Repositories;
using HRM.API.Responses;

namespace HRM.API.Services;

public interface IMeetingRoomAmenityService
{
    Task<ApiResponse<List<MeetingRoomAmenityResponse>>> GetAllAsync();

    Task<ApiResponse<MeetingRoomAmenityResponse>> GetByIdAsync(Guid id);

    Task<ApiResponse<Guid>> CreateAsync(CreateMeetingRoomAmenityDto request, Guid userId);

    Task<ApiResponse<bool>> UpdateAsync(UpdateMeetingRoomAmenityDto request, Guid userId);

    Task<ApiResponse<bool>> DeleteAsync(Guid id, Guid userId);
}

public class MeetingRoomAmenityService : IMeetingRoomAmenityService
{
    private const int MaxNameLength = 100;
    private const int MaxDescriptionLength = 500;

    private readonly IMeetingRoomAmenityRepository _amenityRepository;

    public MeetingRoomAmenityService(IMeetingRoomAmenityRepository amenityRepository)
    {
        _amenityRepository = amenityRepository;
    }

    public static MeetingRoomAmenityResponse ToResponse(MeetingRoomAmenity amenity)
    {
        return new MeetingRoomAmenityResponse
        {
            Id = amenity.Id,
            Name = amenity.Name,
            Description = amenity.Description,
            IsActive = amenity.IsActive
        };
    }

    public async Task<ApiResponse<List<MeetingRoomAmenityResponse>>> GetAllAsync()
    {
        try
        {
            var amenities = await _amenityRepository.GetAllAsync();

            return new ApiResponse<List<MeetingRoomAmenityResponse>>
            {
                Success = true,
                Message = "Amenities retrieved successfully.",
                Data = amenities.Select(ToResponse).ToList()
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<List<MeetingRoomAmenityResponse>>
            {
                Success = false,
                Message = "An error occurred while retrieving amenities.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<MeetingRoomAmenityResponse>> GetByIdAsync(Guid id)
    {
        try
        {
            var amenity = await _amenityRepository.GetByIdAsync(id);

            if (amenity == null)
            {
                return new ApiResponse<MeetingRoomAmenityResponse>
                {
                    Success = false,
                    Message = "Amenity not found."
                };
            }

            return new ApiResponse<MeetingRoomAmenityResponse>
            {
                Success = true,
                Message = "Amenity retrieved successfully.",
                Data = ToResponse(amenity)
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<MeetingRoomAmenityResponse>
            {
                Success = false,
                Message = "An error occurred while retrieving the amenity.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<Guid>> CreateAsync(CreateMeetingRoomAmenityDto request, Guid userId)
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

            var nameExists = await _amenityRepository.ExistsByNameAsync(trimmedName);

            if (nameExists)
            {
                return new ApiResponse<Guid>
                {
                    Success = false,
                    Message = "An amenity with this name already exists."
                };
            }

            var amenity = new MeetingRoomAmenity
            {
                Id = Guid.NewGuid(),
                Name = trimmedName,
                Description = trimmedDescription,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            await _amenityRepository.AddAsync(amenity);

            await _amenityRepository.SaveChangesAsync();

            return new ApiResponse<Guid>
            {
                Success = true,
                Message = "Amenity created successfully.",
                Data = amenity.Id
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<Guid>
            {
                Success = false,
                Message = "An error occurred while creating the amenity.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<bool>> UpdateAsync(UpdateMeetingRoomAmenityDto request, Guid userId)
    {
        try
        {
            var amenity = await _amenityRepository.GetByIdAsync(request.Id);

            if (amenity == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Amenity not found."
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

            var nameExists = await _amenityRepository.ExistsByNameAsync(trimmedName, request.Id);

            if (nameExists)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An amenity with this name already exists."
                };
            }

            amenity.Name = trimmedName;
            amenity.Description = trimmedDescription;
            amenity.UpdatedAt = DateTime.UtcNow;
            amenity.UpdatedBy = userId;

            _amenityRepository.Update(amenity);

            await _amenityRepository.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Amenity updated successfully.",
                Data = true
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "An error occurred while updating the amenity.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id, Guid userId)
    {
        try
        {
            var amenity = await _amenityRepository.GetByIdAsync(id);

            if (amenity == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Amenity not found."
                };
            }

            amenity.IsActive = false;
            amenity.UpdatedAt = DateTime.UtcNow;
            amenity.UpdatedBy = userId;

            _amenityRepository.Update(amenity);

            await _amenityRepository.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Amenity deleted successfully.",
                Data = true
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "An error occurred while deleting the amenity.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    private static string? ValidateName(string? name, out string trimmedName)
    {
        trimmedName = name?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(trimmedName))
        {
            return "Amenity name is required and cannot contain only spaces.";
        }

        if (trimmedName.Length > MaxNameLength)
        {
            return $"Amenity name cannot exceed {MaxNameLength} characters.";
        }

        return null;
    }

    private static string? ValidateDescription(string? description, out string? trimmedDescription)
    {
        trimmedDescription = string.IsNullOrWhiteSpace(description) ? null : description.Trim();

        if (trimmedDescription != null && trimmedDescription.Length > MaxDescriptionLength)
        {
            return $"Description cannot exceed {MaxDescriptionLength} characters.";
        }

        return null;
    }
}
