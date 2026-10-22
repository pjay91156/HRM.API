using HRM.API.Enums;
using HRM.API.Models;
using HRM.API.Repositories;
using HRM.API.Responses;

namespace HRM.API.Services;

public interface IHolidayService
{
    Task<ApiResponse<List<HolidayResponse>>> GetHolidaysAsync(int? year, HolidayType? holidayType, string? location, string? search);

    Task<ApiResponse<HolidayResponse>> GetByIdAsync(Guid id);

    Task<ApiResponse<Guid>> CreateAsync(HolidayDto request, Guid userId);

    Task<ApiResponse<bool>> UpdateAsync(UpdateHolidayDto request, Guid userId);

    Task<ApiResponse<bool>> DeleteAsync(Guid id);

    Task<ApiResponse<HolidayDashboardResponse>> GetDashboardSummaryAsync(int year);

    Task<ApiResponse<List<OptionalHolidayResponse>>> GetOptionalHolidaysAsync(Guid employeeId, int year);

    Task<ApiResponse<bool>> SelectOptionalHolidaysAsync(Guid employeeId, int year, SelectOptionalHolidaysDto request, Guid userId);
}

public class HolidayService : IHolidayService
{
    private const int MaxOptionalHolidaySelections = 3;
    private const long MaxFileSizeBytes = 2 * 1024 * 1024; // 2 MB
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

    private readonly IHolidayRepository _holidayRepository;
    private readonly IEmployeeOptionalHolidaySelectionRepository _optionalHolidaySelectionRepository;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public HolidayService(
        IHolidayRepository holidayRepository,
        IEmployeeOptionalHolidaySelectionRepository optionalHolidaySelectionRepository,
        IWebHostEnvironment webHostEnvironment)
    {
        _holidayRepository = holidayRepository;
        _optionalHolidaySelectionRepository = optionalHolidaySelectionRepository;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<ApiResponse<List<HolidayResponse>>> GetHolidaysAsync(int? year, HolidayType? holidayType, string? location, string? search)
    {
        try
        {
            var holidays = await _holidayRepository.GetHolidaysAsync(year, holidayType, location, search);

            return new ApiResponse<List<HolidayResponse>>
            {
                Success = true,
                Message = "Holidays retrieved successfully.",
                Data = holidays
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<List<HolidayResponse>>
            {
                Success = false,
                Message = "Error while retrieving holidays.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<HolidayResponse>> GetByIdAsync(Guid id)
    {
        try
        {
            var holiday = await _holidayRepository.GetByIdAsync(id);

            if (holiday == null)
            {
                return new ApiResponse<HolidayResponse> { Success = false, Message = "Holiday not found." };
            }

            return new ApiResponse<HolidayResponse>
            {
                Success = true,
                Message = "Holiday retrieved successfully.",
                Data = ToResponse(holiday)
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<HolidayResponse>
            {
                Success = false,
                Message = "Error while retrieving the holiday.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<Guid>> CreateAsync(HolidayDto request, Guid userId)
    {
        try
        {
            var validationError = ValidateHoliday(request.Name);

            if (validationError != null)
            {
                return new ApiResponse<Guid> { Success = false, Message = validationError };
            }

            string? imageUrl = null;

            if (request.Image != null)
            {
                var imageError = ValidateImage(request.Image, out var savedFileName);

                if (imageError != null)
                {
                    return new ApiResponse<Guid> { Success = false, Message = imageError };
                }

                await SaveImageAsync(request.Image, savedFileName);
                imageUrl = $"/uploads/holidays/{savedFileName}";
            }

            var holiday = new Holiday
            {
                Id = Guid.NewGuid(),
                Name = request.Name.Trim(),
                HolidayDate = request.HolidayDate.Date,
                // Type and location are no longer captured from the UI
                HolidayType = HolidayType.PublicHoliday,
                Location = request.Location?.Trim() ?? string.Empty,
                Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
                ImageUrl = imageUrl,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            await _holidayRepository.AddAsync(holiday);
            await _holidayRepository.SaveChangesAsync();

            return new ApiResponse<Guid>
            {
                Success = true,
                Message = "Holiday added successfully.",
                Data = holiday.Id
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<Guid>
            {
                Success = false,
                Message = "Error while adding the holiday.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<bool>> UpdateAsync(UpdateHolidayDto request, Guid userId)
    {
        try
        {
            var holiday = await _holidayRepository.GetByIdAsync(request.Id);

            if (holiday == null)
            {
                return new ApiResponse<bool> { Success = false, Message = "Holiday not found." };
            }

            var validationError = ValidateHoliday(request.Name);

            if (validationError != null)
            {
                return new ApiResponse<bool> { Success = false, Message = validationError };
            }

            if (request.Image != null)
            {
                var imageError = ValidateImage(request.Image, out var savedFileName);

                if (imageError != null)
                {
                    return new ApiResponse<bool> { Success = false, Message = imageError };
                }

                await SaveImageAsync(request.Image, savedFileName);
                holiday.ImageUrl = $"/uploads/holidays/{savedFileName}";
            }

            holiday.Name = request.Name.Trim();
            holiday.HolidayDate = request.HolidayDate.Date;
            holiday.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
            holiday.IsActive = request.IsActive;
            holiday.UpdatedAt = DateTime.UtcNow;
            holiday.UpdatedBy = userId;

            _holidayRepository.Update(holiday);
            await _holidayRepository.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Holiday updated successfully.",
                Data = true
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Error while updating the holiday.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id)
    {
        try
        {
            var deleted = await _holidayRepository.DeleteAsync(id);

            if (!deleted)
            {
                return new ApiResponse<bool> { Success = false, Message = "Holiday not found.", Data = false };
            }

            await _holidayRepository.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Holiday deleted successfully.",
                Data = true
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Error while deleting the holiday.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<HolidayDashboardResponse>> GetDashboardSummaryAsync(int year)
    {
        try
        {
            var counts = await _holidayRepository.GetDashboardCountsAsync(year);
            var upcoming = await _holidayRepository.GetUpcomingAsync(3);

            return new ApiResponse<HolidayDashboardResponse>
            {
                Success = true,
                Message = "Holiday dashboard summary retrieved successfully.",
                Data = new HolidayDashboardResponse
                {
                    TotalHolidays = counts.total,
                    PublicHolidayCount = counts.publicCount,
                    OptionalHolidayCount = counts.optionalCount,
                    CompanyHolidayCount = counts.companyCount,
                    UpcomingHolidays = upcoming
                }
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<HolidayDashboardResponse>
            {
                Success = false,
                Message = "Error while retrieving the holiday dashboard summary.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<List<OptionalHolidayResponse>>> GetOptionalHolidaysAsync(Guid employeeId, int year)
    {
        try
        {
            var holidays = await _holidayRepository.GetActiveOptionalHolidaysByYearAsync(year);
            var selectedIds = await _optionalHolidaySelectionRepository.GetSelectedHolidayIdsAsync(employeeId, year);
            var selectedSet = selectedIds.ToHashSet();

            var response = holidays
                .Select(x => new OptionalHolidayResponse
                {
                    Id = x.Id,
                    Name = x.Name,
                    HolidayDate = x.HolidayDate,
                    HolidayType = (int)x.HolidayType,
                    HolidayTypeName = x.HolidayType.ToString(),
                    Location = x.Location,
                    Description = x.Description,
                    ImageUrl = x.ImageUrl,
                    IsActive = x.IsActive,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt,
                    IsSelected = selectedSet.Contains(x.Id)
                })
                .ToList();

            return new ApiResponse<List<OptionalHolidayResponse>>
            {
                Success = true,
                Message = "Optional holidays retrieved successfully.",
                Data = response
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<List<OptionalHolidayResponse>>
            {
                Success = false,
                Message = "Error while retrieving optional holidays.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<bool>> SelectOptionalHolidaysAsync(Guid employeeId, int year, SelectOptionalHolidaysDto request, Guid userId)
    {
        try
        {
            var holidayIds = request.HolidayIds.Distinct().ToList();

            if (holidayIds.Count > MaxOptionalHolidaySelections)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = $"You can select up to {MaxOptionalHolidaySelections} optional holidays in a year."
                };
            }

            var availableHolidays = await _holidayRepository.GetActiveOptionalHolidaysByYearAsync(year);
            var availableIds = availableHolidays.Select(x => x.Id).ToHashSet();

            if (holidayIds.Any(id => !availableIds.Contains(id)))
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "One or more selected holidays are not valid optional holidays for this year."
                };
            }

            var selections = holidayIds
                .Select(holidayId => new EmployeeOptionalHolidaySelection
                {
                    Id = Guid.NewGuid(),
                    EmployeeId = employeeId,
                    HolidayId = holidayId,
                    Year = year,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId
                })
                .ToList();

            await _optionalHolidaySelectionRepository.ReplaceSelectionsAsync(employeeId, year, selections);

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Optional holiday selection saved successfully.",
                Data = true
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Error while saving optional holiday selection.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    private static HolidayResponse ToResponse(Holiday holiday)
    {
        return new HolidayResponse
        {
            Id = holiday.Id,
            Name = holiday.Name,
            HolidayDate = holiday.HolidayDate,
            HolidayType = (int)holiday.HolidayType,
            HolidayTypeName = holiday.HolidayType.ToString(),
            Location = holiday.Location,
            Description = holiday.Description,
            ImageUrl = holiday.ImageUrl,
            IsActive = holiday.IsActive,
            CreatedAt = holiday.CreatedAt,
            UpdatedAt = holiday.UpdatedAt
        };
    }

    private static string? ValidateHoliday(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "Holiday name is required.";
        }

        return null;
    }

    private static string? ValidateImage(IFormFile file, out string savedFileName)
    {
        savedFileName = string.Empty;

        if (file.Length == 0)
        {
            return "The selected image is empty.";
        }

        if (file.Length > MaxFileSizeBytes)
        {
            return "Image must be 2 MB or smaller.";
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!AllowedExtensions.Contains(extension))
        {
            return "Only JPG, PNG, and WEBP images are allowed.";
        }

        savedFileName = $"{Guid.NewGuid()}{extension}";

        return null;
    }

    private async Task SaveImageAsync(IFormFile file, string fileName)
    {
        var uploadsFolder = Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot", "uploads", "holidays");
        Directory.CreateDirectory(uploadsFolder);

        var filePath = Path.Combine(uploadsFolder, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);
    }
}
