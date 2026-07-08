using HRM.API.Models;
using HRM.API.Repositories;
using HRM.API.Responses;
using HRM.API.Services;

namespace HRM.API.Services;
public interface IPerformanceCycleService
{
    Task<ApiResponse<Guid>> CreateAsync(CreatePerformanceCycleDto request, Guid userId);

    Task<ApiResponse<bool>> UpdateAsync(UpdatePerformanceCycleDto request, Guid userId);

    Task<ApiResponse<bool>> DeleteAsync(Guid id, Guid userId);
    Task<ApiResponse<List<PerformanceCycleResponse>>> GetAllAsync();

Task<ApiResponse<PerformanceCycleResponse>> GetByIdAsync(Guid id);
}
public class PerformanceCycleService : IPerformanceCycleService
{
    private readonly IPerformanceCycleRepository _performanceCycleRepository;

    public PerformanceCycleService(IPerformanceCycleRepository performanceCycleRepository)
    {
        _performanceCycleRepository = performanceCycleRepository;
    }
    public async Task<ApiResponse<List<PerformanceCycleResponse>>> GetAllAsync()
{
    try
    {
        var data = await _performanceCycleRepository.GetAllAsync();

        var result = data.Select(x => new PerformanceCycleResponse
        {
            Id = x.Id,
            CycleName = x.CycleName,
            ReviewPeriodStart = x.ReviewPeriodStart,
            ReviewPeriodEnd = x.ReviewPeriodEnd,
            EmployeeReviewStart = x.EmployeeReviewStart,
            EmployeeReviewEnd = x.EmployeeReviewEnd,
            ManagerReviewStart = x.ManagerReviewStart,
            ManagerReviewEnd = x.ManagerReviewEnd,
            Status = x.Status
        }).ToList();

        return new ApiResponse<List<PerformanceCycleResponse>>
        {
            Success = true,
            Message = "Performance cycles retrieved successfully.",
            Data = result
        };
    }
    catch (Exception ex)
    {
        return new ApiResponse<List<PerformanceCycleResponse>>
        {
            Success = false,
            Message = "An error occurred while retrieving performance cycles.",
            Errors = new List<string> { ex.Message }
        };
    }
}
public async Task<ApiResponse<PerformanceCycleResponse>> GetByIdAsync(Guid id)
{
    try
    {
        var cycle = await _performanceCycleRepository.GetByIdAsync(id);

        if (cycle == null)
        {
            return new ApiResponse<PerformanceCycleResponse>
            {
                Success = false,
                Message = "Performance cycle not found."
            };
        }

        return new ApiResponse<PerformanceCycleResponse>
        {
            Success = true,
            Message = "Performance cycle retrieved successfully.",
            Data = new PerformanceCycleResponse
            {
                Id = cycle.Id,
                CycleName = cycle.CycleName,
                ReviewPeriodStart = cycle.ReviewPeriodStart,
                ReviewPeriodEnd = cycle.ReviewPeriodEnd,
                EmployeeReviewStart = cycle.EmployeeReviewStart,
                EmployeeReviewEnd = cycle.EmployeeReviewEnd,
                ManagerReviewStart = cycle.ManagerReviewStart,
                ManagerReviewEnd = cycle.ManagerReviewEnd,
                Status = cycle.Status
            }
        };
    }
    catch (Exception ex)
    {
        return new ApiResponse<PerformanceCycleResponse>
        {
            Success = false,
            Message = "An error occurred while retrieving the performance cycle.",
            Errors = new List<string> { ex.Message }
        };
    }
}
    public async Task<ApiResponse<Guid>> CreateAsync(CreatePerformanceCycleDto request, Guid userId)
{
    try
    {
        //Check duplicate cycle name
        var exists = await _performanceCycleRepository.ExistsAsync(request.CycleName);

        if (exists)
        {
            return new ApiResponse<Guid>
            {
                Success = false,
                Message = "Performance cycle already exists."
            };
        }

        //Validate dates
        if (request.ReviewPeriodStart > request.ReviewPeriodEnd)
        {
            return new ApiResponse<Guid>
            {
                Success = false,
                Message = "Review period start date cannot be greater than review period end date."
            };
        }

        if (request.EmployeeReviewStart > request.EmployeeReviewEnd)
        {
            return new ApiResponse<Guid>
            {
                Success = false,
                Message = "Employee review start date cannot be greater than employee review end date."
            };
        }

        if (request.ManagerReviewStart > request.ManagerReviewEnd)
        {
            return new ApiResponse<Guid>
            {
                Success = false,
                Message = "Manager review start date cannot be greater than manager review end date."
            };
        }

        var performanceCycle = new PerformanceCycle
        {
            Id = Guid.NewGuid(),
            CycleName = request.CycleName,
            ReviewPeriodStart = request.ReviewPeriodStart,
            ReviewPeriodEnd = request.ReviewPeriodEnd,
            EmployeeReviewStart = request.EmployeeReviewStart,
            EmployeeReviewEnd = request.EmployeeReviewEnd,
            ManagerReviewStart = request.ManagerReviewStart,
            ManagerReviewEnd = request.ManagerReviewEnd,
            Status = request.Status,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        await _performanceCycleRepository.AddAsync(performanceCycle);

        await _performanceCycleRepository.SaveChangesAsync();

        return new ApiResponse<Guid>
        {
            Success = true,
            Message = "Performance cycle created successfully.",
            Data = performanceCycle.Id
        };
    }
    catch (Exception ex)
    {
        return new ApiResponse<Guid>
        {
            Success = false,
            Message = "An error occurred while creating the performance cycle.",
            Errors = new List<string>
            {
                ex.Message
            }
        };
    }
}
public async Task<ApiResponse<bool>> UpdateAsync(UpdatePerformanceCycleDto request, Guid userId)
{
    try
    {
        // Check if performance cycle exists
        var performanceCycle = await _performanceCycleRepository.GetByIdAsync(request.Id);

        if (performanceCycle == null)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Performance cycle not found."
            };
        }

        // Check duplicate cycle name
        var exists = await _performanceCycleRepository.ExistsAsync(request.CycleName, request.Id);

        if (exists)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Performance cycle already exists."
            };
        }

        // Validate Review Period
        if (request.ReviewPeriodStart > request.ReviewPeriodEnd)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Review period start date cannot be greater than review period end date."
            };
        }

        // Validate Employee Review Period
        if (request.EmployeeReviewStart > request.EmployeeReviewEnd)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Employee review start date cannot be greater than employee review end date."
            };
        }

        // Validate Manager Review Period
        if (request.ManagerReviewStart > request.ManagerReviewEnd)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Manager review start date cannot be greater than manager review end date."
            };
        }

        // Update Entity
        performanceCycle.CycleName = request.CycleName;
        performanceCycle.ReviewPeriodStart = request.ReviewPeriodStart;
        performanceCycle.ReviewPeriodEnd = request.ReviewPeriodEnd;
        performanceCycle.EmployeeReviewStart = request.EmployeeReviewStart;
        performanceCycle.EmployeeReviewEnd = request.EmployeeReviewEnd;
        performanceCycle.ManagerReviewStart = request.ManagerReviewStart;
        performanceCycle.ManagerReviewEnd = request.ManagerReviewEnd;
        performanceCycle.Status = request.Status;
        performanceCycle.UpdatedAt = DateTime.UtcNow;
        performanceCycle.UpdatedBy = userId;

        _performanceCycleRepository.Update(performanceCycle);

        await _performanceCycleRepository.SaveChangesAsync();

        return new ApiResponse<bool>
        {
            Success = true,
            Message = "Performance cycle updated successfully.",
            Data = true
        };
    }
    catch (Exception ex)
    {
        return new ApiResponse<bool>
        {
            Success = false,
            Message = "An error occurred while updating the performance cycle.",
            Errors = new List<string>
            {
                ex.Message
            }
        };
    }
}
public async Task<ApiResponse<bool>> DeleteAsync(Guid id, Guid userId)
{
    try
    {
        var performanceCycle = await _performanceCycleRepository.GetByIdAsync(id);

        if (performanceCycle == null)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Performance cycle not found."
            };
        }

        performanceCycle.IsActive = false;
        performanceCycle.UpdatedAt = DateTime.UtcNow;
        performanceCycle.UpdatedBy = userId;

        _performanceCycleRepository.Update(performanceCycle);

        await _performanceCycleRepository.SaveChangesAsync();

        return new ApiResponse<bool>
        {
            Success = true,
            Message = "Performance cycle deleted successfully.",
            Data = true
        };
    }
    catch (Exception ex)
    {
        return new ApiResponse<bool>
        {
            Success = false,
            Message = "An error occurred while deleting the performance cycle.",
            Errors = new List<string>
            {
                ex.Message
            }
        };
    }
}


    //Methods will be added here
}