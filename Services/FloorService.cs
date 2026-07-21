using HRM.API.Models;
using HRM.API.Repositories;
using HRM.API.Responses;

namespace HRM.API.Services;

public interface IFloorService
{
    Task<ApiResponse<List<FloorResponse>>> GetAllAsync();

    Task<ApiResponse<FloorResponse>> GetByIdAsync(Guid id);

    Task<ApiResponse<Guid>> CreateAsync(CreateFloorDto request, Guid userId);

    Task<ApiResponse<bool>> UpdateAsync(UpdateFloorDto request, Guid userId);

    Task<ApiResponse<bool>> DeleteAsync(Guid id, Guid userId);
}

public class FloorService : IFloorService
{
    private const int MaxNameLength = 100;
    private const int MaxDescriptionLength = 500;

    private readonly IFloorRepository _floorRepository;

    public FloorService(IFloorRepository floorRepository)
    {
        _floorRepository = floorRepository;
    }

    private static FloorResponse ToResponse(Floor floor)
    {
        return new FloorResponse
        {
            Id = floor.Id,
            Name = floor.Name,
            Description = floor.Description,
            IsActive = floor.IsActive
        };
    }

    public async Task<ApiResponse<List<FloorResponse>>> GetAllAsync()
    {
        try
        {
            var floors = await _floorRepository.GetAllAsync();

            return new ApiResponse<List<FloorResponse>>
            {
                Success = true,
                Message = "Floors retrieved successfully.",
                Data = floors.Select(ToResponse).ToList()
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<List<FloorResponse>>
            {
                Success = false,
                Message = "An error occurred while retrieving floors.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<FloorResponse>> GetByIdAsync(Guid id)
    {
        try
        {
            var floor = await _floorRepository.GetByIdAsync(id);

            if (floor == null)
            {
                return new ApiResponse<FloorResponse>
                {
                    Success = false,
                    Message = "Floor not found."
                };
            }

            return new ApiResponse<FloorResponse>
            {
                Success = true,
                Message = "Floor retrieved successfully.",
                Data = ToResponse(floor)
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<FloorResponse>
            {
                Success = false,
                Message = "An error occurred while retrieving the floor.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<Guid>> CreateAsync(CreateFloorDto request, Guid userId)
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

            var nameExists = await _floorRepository.ExistsByNameAsync(trimmedName);

            if (nameExists)
            {
                return new ApiResponse<Guid>
                {
                    Success = false,
                    Message = "A floor with this name already exists."
                };
            }

            var floor = new Floor
            {
                Id = Guid.NewGuid(),
                Name = trimmedName,
                Description = trimmedDescription,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            await _floorRepository.AddAsync(floor);

            await _floorRepository.SaveChangesAsync();

            return new ApiResponse<Guid>
            {
                Success = true,
                Message = "Floor created successfully.",
                Data = floor.Id
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<Guid>
            {
                Success = false,
                Message = "An error occurred while creating the floor.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<bool>> UpdateAsync(UpdateFloorDto request, Guid userId)
    {
        try
        {
            var floor = await _floorRepository.GetByIdAsync(request.Id);

            if (floor == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Floor not found."
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

            var nameExists = await _floorRepository.ExistsByNameAsync(trimmedName, request.Id);

            if (nameExists)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "A floor with this name already exists."
                };
            }

            floor.Name = trimmedName;
            floor.Description = trimmedDescription;
            floor.UpdatedAt = DateTime.UtcNow;
            floor.UpdatedBy = userId;

            _floorRepository.Update(floor);

            await _floorRepository.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Floor updated successfully.",
                Data = true
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "An error occurred while updating the floor.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id, Guid userId)
    {
        try
        {
            var floor = await _floorRepository.GetByIdAsync(id);

            if (floor == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Floor not found."
                };
            }

            floor.IsActive = false;
            floor.UpdatedAt = DateTime.UtcNow;
            floor.UpdatedBy = userId;

            _floorRepository.Update(floor);

            await _floorRepository.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Floor deleted successfully.",
                Data = true
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "An error occurred while deleting the floor.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    private static string? ValidateName(string? name, out string trimmedName)
    {
        trimmedName = name?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(trimmedName))
        {
            return "Floor name is required and cannot contain only spaces.";
        }

        if (trimmedName.Length > MaxNameLength)
        {
            return $"Floor name cannot exceed {MaxNameLength} characters.";
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
