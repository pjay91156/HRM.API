using HRM.API.Models;
using HRM.API.Repositories;
using HRM.API.Responses;

namespace HRM.API.Services;

public interface IPerformanceRatingService
{
    Task<ApiResponse<Guid>> CreateAsync(CreatePerformanceRatingDto request, Guid userId);

    Task<ApiResponse<bool>> UpdateAsync(UpdatePerformanceRatingDto request, Guid userId);

    Task<ApiResponse<bool>> DeleteAsync(Guid id, Guid userId);

    Task<ApiResponse<List<PerformanceRatingResponse>>> GetAllAsync();

    Task<ApiResponse<PerformanceRatingResponse>> GetByIdAsync(Guid id);
}

public class PerformanceRatingService : IPerformanceRatingService
{
    private const byte MinRating = 1;
    private const byte MaxRating = 5;

    private readonly IPerformanceRatingRepository _performanceRatingRepository;

    public PerformanceRatingService(IPerformanceRatingRepository performanceRatingRepository)
    {
        _performanceRatingRepository = performanceRatingRepository;
    }

    public async Task<ApiResponse<List<PerformanceRatingResponse>>> GetAllAsync()
    {
        try
        {
            var data = await _performanceRatingRepository.GetAllAsync();

            var result = data.Select(x => new PerformanceRatingResponse
            {
                Id = x.Id,
                Rating = x.Rating,
                RatingName = x.RatingName,
                Description = x.Description,
                DisplayOrder = x.DisplayOrder,
                IsActive = x.IsActive
            }).ToList();

            return new ApiResponse<List<PerformanceRatingResponse>>
            {
                Success = true,
                Message = "Performance ratings retrieved successfully.",
                Data = result
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<List<PerformanceRatingResponse>>
            {
                Success = false,
                Message = "An error occurred while retrieving performance ratings.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<PerformanceRatingResponse>> GetByIdAsync(Guid id)
    {
        try
        {
            var rating = await _performanceRatingRepository.GetByIdAsync(id);

            if (rating == null)
            {
                return new ApiResponse<PerformanceRatingResponse>
                {
                    Success = false,
                    Message = "Performance rating not found."
                };
            }

            return new ApiResponse<PerformanceRatingResponse>
            {
                Success = true,
                Message = "Performance rating retrieved successfully.",
                Data = new PerformanceRatingResponse
                {
                    Id = rating.Id,
                    Rating = rating.Rating,
                    RatingName = rating.RatingName,
                    Description = rating.Description,
                    DisplayOrder = rating.DisplayOrder,
                    IsActive = rating.IsActive
                }
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<PerformanceRatingResponse>
            {
                Success = false,
                Message = "An error occurred while retrieving the performance rating.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<Guid>> CreateAsync(CreatePerformanceRatingDto request, Guid userId)
    {
        try
        {
            if (request.Rating < MinRating || request.Rating > MaxRating)
            {
                return new ApiResponse<Guid>
                {
                    Success = false,
                    Message = $"Rating must be between {MinRating} and {MaxRating} stars."
                };
            }

            if (string.IsNullOrWhiteSpace(request.RatingName))
            {
                return new ApiResponse<Guid>
                {
                    Success = false,
                    Message = "Rating name is required."
                };
            }

            var ratingExists = await _performanceRatingRepository.ExistsByRatingAsync(request.Rating);

            if (ratingExists)
            {
                return new ApiResponse<Guid>
                {
                    Success = false,
                    Message = "A performance rating with this star value already exists."
                };
            }

            var nameExists = await _performanceRatingRepository.ExistsByNameAsync(request.RatingName);

            if (nameExists)
            {
                return new ApiResponse<Guid>
                {
                    Success = false,
                    Message = "A performance rating with this name already exists."
                };
            }

            var performanceRating = new PerformanceRating
            {
                Id = Guid.NewGuid(),
                Rating = request.Rating,
                RatingName = request.RatingName,
                Description = request.Description,
                DisplayOrder = request.DisplayOrder,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            await _performanceRatingRepository.AddAsync(performanceRating);

            await _performanceRatingRepository.SaveChangesAsync();

            return new ApiResponse<Guid>
            {
                Success = true,
                Message = "Performance rating created successfully.",
                Data = performanceRating.Id
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<Guid>
            {
                Success = false,
                Message = "An error occurred while creating the performance rating.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<bool>> UpdateAsync(UpdatePerformanceRatingDto request, Guid userId)
    {
        try
        {
            var performanceRating = await _performanceRatingRepository.GetByIdAsync(request.Id);

            if (performanceRating == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Performance rating not found."
                };
            }

            if (request.Rating < MinRating || request.Rating > MaxRating)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = $"Rating must be between {MinRating} and {MaxRating} stars."
                };
            }

            var ratingExists = await _performanceRatingRepository.ExistsByRatingAsync(request.Rating, request.Id);

            if (ratingExists)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "A performance rating with this star value already exists."
                };
            }

            var nameExists = await _performanceRatingRepository.ExistsByNameAsync(request.RatingName, request.Id);

            if (nameExists)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "A performance rating with this name already exists."
                };
            }

            performanceRating.Rating = request.Rating;
            performanceRating.RatingName = request.RatingName;
            performanceRating.Description = request.Description;
            performanceRating.DisplayOrder = request.DisplayOrder;
            performanceRating.UpdatedAt = DateTime.UtcNow;
            performanceRating.UpdatedBy = userId;

            _performanceRatingRepository.Update(performanceRating);

            await _performanceRatingRepository.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Performance rating updated successfully.",
                Data = true
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "An error occurred while updating the performance rating.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id, Guid userId)
    {
        try
        {
            var performanceRating = await _performanceRatingRepository.GetByIdAsync(id);

            if (performanceRating == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Performance rating not found."
                };
            }

            performanceRating.IsActive = false;
            performanceRating.UpdatedAt = DateTime.UtcNow;
            performanceRating.UpdatedBy = userId;

            _performanceRatingRepository.Update(performanceRating);

            await _performanceRatingRepository.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Performance rating deleted successfully.",
                Data = true
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "An error occurred while deleting the performance rating.",
                Errors = new List<string> { ex.Message }
            };
        }
    }
}
