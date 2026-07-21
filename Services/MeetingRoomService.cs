using HRM.API.Models;
using HRM.API.Repositories;
using HRM.API.Responses;

namespace HRM.API.Services;

public interface IMeetingRoomService
{
    Task<ApiResponse<List<MeetingRoomResponse>>> GetAllAsync(Guid? floorId);

    Task<ApiResponse<MeetingRoomResponse>> GetByIdAsync(Guid id);

    Task<ApiResponse<Guid>> CreateAsync(CreateMeetingRoomDto request, Guid userId);

    Task<ApiResponse<bool>> UpdateAsync(UpdateMeetingRoomDto request, Guid userId);

    Task<ApiResponse<bool>> DeleteAsync(Guid id, Guid userId);
}

public class MeetingRoomService : IMeetingRoomService
{
    private const int MaxNameLength = 100;
    private const int MaxDescriptionLength = 500;

    private readonly IMeetingRoomRepository _meetingRoomRepository;
    private readonly IFloorRepository _floorRepository;
    private readonly IMeetingRoomAmenityRepository _amenityRepository;

    public MeetingRoomService(
        IMeetingRoomRepository meetingRoomRepository,
        IFloorRepository floorRepository,
        IMeetingRoomAmenityRepository amenityRepository)
    {
        _meetingRoomRepository = meetingRoomRepository;
        _floorRepository = floorRepository;
        _amenityRepository = amenityRepository;
    }

    private static MeetingRoomResponse ToResponse(MeetingRoom meetingRoom)
    {
        return new MeetingRoomResponse
        {
            Id = meetingRoom.Id,
            FloorId = meetingRoom.FloorId,
            FloorName = meetingRoom.Floor?.Name ?? string.Empty,
            Name = meetingRoom.Name,
            Capacity = meetingRoom.Capacity,
            Description = meetingRoom.Description,
            IsActive = meetingRoom.IsActive,
            Amenities = meetingRoom.MeetingRoomAmenityDetails
                .Where(d => d.IsActive && d.MeetingRoomAmenity.IsActive)
                .Select(d => MeetingRoomAmenityService.ToResponse(d.MeetingRoomAmenity))
                .OrderBy(a => a.Name)
                .ToList()
        };
    }

    public async Task<ApiResponse<List<MeetingRoomResponse>>> GetAllAsync(Guid? floorId)
    {
        try
        {
            var meetingRooms = await _meetingRoomRepository.GetAllAsync(floorId);

            return new ApiResponse<List<MeetingRoomResponse>>
            {
                Success = true,
                Message = "Meeting rooms retrieved successfully.",
                Data = meetingRooms.Select(ToResponse).ToList()
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<List<MeetingRoomResponse>>
            {
                Success = false,
                Message = "An error occurred while retrieving meeting rooms.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<MeetingRoomResponse>> GetByIdAsync(Guid id)
    {
        try
        {
            var meetingRoom = await _meetingRoomRepository.GetByIdAsync(id);

            if (meetingRoom == null)
            {
                return new ApiResponse<MeetingRoomResponse>
                {
                    Success = false,
                    Message = "Meeting room not found."
                };
            }

            return new ApiResponse<MeetingRoomResponse>
            {
                Success = true,
                Message = "Meeting room retrieved successfully.",
                Data = ToResponse(meetingRoom)
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<MeetingRoomResponse>
            {
                Success = false,
                Message = "An error occurred while retrieving the meeting room.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<Guid>> CreateAsync(CreateMeetingRoomDto request, Guid userId)
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

            var capacityValidation = ValidateCapacity(request.Capacity);

            if (capacityValidation != null)
            {
                return new ApiResponse<Guid> { Success = false, Message = capacityValidation };
            }

            var floor = await _floorRepository.GetByIdAsync(request.FloorId);

            if (floor == null)
            {
                return new ApiResponse<Guid> { Success = false, Message = "Floor not found." };
            }

            var amenityValidation = await ValidateAmenityIdsAsync(request.AmenityIds);

            if (amenityValidation != null)
            {
                return new ApiResponse<Guid> { Success = false, Message = amenityValidation };
            }

            var nameExists = await _meetingRoomRepository.ExistsByFloorAndNameAsync(request.FloorId, trimmedName);

            if (nameExists)
            {
                return new ApiResponse<Guid>
                {
                    Success = false,
                    Message = "A meeting room with this name already exists on this floor."
                };
            }

            var meetingRoom = new MeetingRoom
            {
                Id = Guid.NewGuid(),
                FloorId = request.FloorId,
                Name = trimmedName,
                Capacity = request.Capacity,
                Description = trimmedDescription,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            await _meetingRoomRepository.AddAsync(meetingRoom);

            foreach (var amenityId in request.AmenityIds.Distinct())
            {
                await _meetingRoomRepository.AddAmenityDetailAsync(new MeetingRoomAmenityDetail
                {
                    Id = Guid.NewGuid(),
                    MeetingRoomId = meetingRoom.Id,
                    MeetingRoomAmenityId = amenityId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId
                });
            }

            await _meetingRoomRepository.SaveChangesAsync();

            return new ApiResponse<Guid>
            {
                Success = true,
                Message = "Meeting room created successfully.",
                Data = meetingRoom.Id
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<Guid>
            {
                Success = false,
                Message = "An error occurred while creating the meeting room.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<bool>> UpdateAsync(UpdateMeetingRoomDto request, Guid userId)
    {
        try
        {
            var meetingRoom = await _meetingRoomRepository.GetByIdAsync(request.Id);

            if (meetingRoom == null)
            {
                return new ApiResponse<bool> { Success = false, Message = "Meeting room not found." };
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

            var capacityValidation = ValidateCapacity(request.Capacity);

            if (capacityValidation != null)
            {
                return new ApiResponse<bool> { Success = false, Message = capacityValidation };
            }

            var floor = await _floorRepository.GetByIdAsync(request.FloorId);

            if (floor == null)
            {
                return new ApiResponse<bool> { Success = false, Message = "Floor not found." };
            }

            var amenityValidation = await ValidateAmenityIdsAsync(request.AmenityIds);

            if (amenityValidation != null)
            {
                return new ApiResponse<bool> { Success = false, Message = amenityValidation };
            }

            var nameExists = await _meetingRoomRepository
                .ExistsByFloorAndNameAsync(request.FloorId, trimmedName, request.Id);

            if (nameExists)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "A meeting room with this name already exists on this floor."
                };
            }

            meetingRoom.FloorId = request.FloorId;
            meetingRoom.Name = trimmedName;
            meetingRoom.Capacity = request.Capacity;
            meetingRoom.Description = trimmedDescription;
            meetingRoom.UpdatedAt = DateTime.UtcNow;
            meetingRoom.UpdatedBy = userId;

            _meetingRoomRepository.Update(meetingRoom);

            await SyncAmenitiesAsync(meetingRoom.Id, request.AmenityIds, userId);

            await _meetingRoomRepository.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Meeting room updated successfully.",
                Data = true
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "An error occurred while updating the meeting room.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id, Guid userId)
    {
        try
        {
            var meetingRoom = await _meetingRoomRepository.GetByIdAsync(id);

            if (meetingRoom == null)
            {
                return new ApiResponse<bool> { Success = false, Message = "Meeting room not found." };
            }

            meetingRoom.IsActive = false;
            meetingRoom.UpdatedAt = DateTime.UtcNow;
            meetingRoom.UpdatedBy = userId;

            _meetingRoomRepository.Update(meetingRoom);

            await _meetingRoomRepository.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Meeting room deleted successfully.",
                Data = true
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "An error occurred while deleting the meeting room.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    private async Task SyncAmenitiesAsync(Guid roomId, List<Guid> requestedAmenityIds, Guid userId)
    {
        var requestedIds = requestedAmenityIds.Distinct().ToHashSet();
        var existingDetails = await _meetingRoomRepository.GetAmenityDetailsByRoomIdAsync(roomId);
        var existingByAmenityId = existingDetails.ToDictionary(d => d.MeetingRoomAmenityId);

        foreach (var detail in existingDetails)
        {
            var shouldBeActive = requestedIds.Contains(detail.MeetingRoomAmenityId);

            if (detail.IsActive != shouldBeActive)
            {
                detail.IsActive = shouldBeActive;
                detail.UpdatedAt = DateTime.UtcNow;
                detail.UpdatedBy = userId;
                _meetingRoomRepository.UpdateAmenityDetail(detail);
            }
        }

        foreach (var amenityId in requestedIds)
        {
            if (existingByAmenityId.ContainsKey(amenityId))
            {
                continue;
            }

            await _meetingRoomRepository.AddAmenityDetailAsync(new MeetingRoomAmenityDetail
            {
                Id = Guid.NewGuid(),
                MeetingRoomId = roomId,
                MeetingRoomAmenityId = amenityId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            });
        }
    }

    private async Task<string?> ValidateAmenityIdsAsync(List<Guid> amenityIds)
    {
        var distinctIds = amenityIds.Distinct().ToList();

        if (distinctIds.Count == 0)
        {
            return null;
        }

        var foundAmenities = await _amenityRepository.GetByIdsAsync(distinctIds);

        if (foundAmenities.Count != distinctIds.Count)
        {
            return "One or more selected amenities were not found.";
        }

        return null;
    }

    private static string? ValidateName(string? name, out string trimmedName)
    {
        trimmedName = name?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(trimmedName))
        {
            return "Meeting room name is required and cannot contain only spaces.";
        }

        if (trimmedName.Length > MaxNameLength)
        {
            return $"Meeting room name cannot exceed {MaxNameLength} characters.";
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

    private static string? ValidateCapacity(int capacity)
    {
        if (capacity <= 0)
        {
            return "Capacity must be greater than 0.";
        }

        return null;
    }
}
