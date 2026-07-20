using HRM.API.Models;
using HRM.API.Repositories;
using HRM.API.Responses;

namespace HRM.API.Services;

public interface IEmployeePerformanceReviewService
{
    Task<ApiResponse<List<PerformanceReviewListItemResponse>>> GetMyReviewsAsync(Guid userId);
    Task<ApiResponse<PerformanceReviewDetailResponse>> GetMyReviewDetailAsync(Guid userId, Guid reviewId);
    Task<ApiResponse<string>> SubmitEmployeeReviewAsync(Guid userId, Guid reviewId, SubmitEmployeePerformanceReviewDto request);
    Task<ApiResponse<List<PerformanceReviewListItemResponse>>> GetMyTeamReviewsAsync(Guid userId);
    Task<ApiResponse<PerformanceReviewDetailResponse>> GetTeamReviewDetailAsync(Guid userId, Guid reviewId);
    Task<ApiResponse<string>> SubmitManagerReviewAsync(Guid userId, Guid reviewId, SubmitManagerPerformanceReviewDto request);
}

public class EmployeePerformanceReviewService : IEmployeePerformanceReviewService
{
    private readonly IEmployeePerformanceReviewRepository _repository;
    private readonly IEmployeeRepository _employeeRepository;

    public EmployeePerformanceReviewService(
        IEmployeePerformanceReviewRepository repository,
        IEmployeeRepository employeeRepository)
    {
        _repository = repository;
        _employeeRepository = employeeRepository;
    }

    private static string GetStatusLabel(byte status) => status switch
    {
        1 => "Submitted",
        2 => "Completed",
        _ => "Not Started"
    };

    private static PerformanceReviewListItemResponse MapListItem(EmployeePerformanceReview review, bool forManagerList)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var cycle = review.PerformanceCycle;

        var windowOpen = forManagerList
            ? today >= cycle.ManagerReviewStart && today <= cycle.ManagerReviewEnd
            : today >= cycle.EmployeeReviewStart && today <= cycle.EmployeeReviewEnd;

        var cycleEnded = today > cycle.ManagerReviewEnd;

        return new PerformanceReviewListItemResponse
        {
            Id = review.Id,
            PerformanceCycleId = review.PerformanceCycleId,
            CycleName = cycle.CycleName,
            ReviewPeriodStart = cycle.ReviewPeriodStart,
            ReviewPeriodEnd = cycle.ReviewPeriodEnd,
            EmployeeReviewStart = cycle.EmployeeReviewStart,
            EmployeeReviewEnd = cycle.EmployeeReviewEnd,
            ManagerReviewStart = cycle.ManagerReviewStart,
            ManagerReviewEnd = cycle.ManagerReviewEnd,
            EmployeeName = review.Employee?.User == null
                ? string.Empty
                : $"{review.Employee.User.FirstName} {review.Employee.User.LastName}",
            Status = GetStatusLabel(review.Status),
            IsWindowOpen = windowOpen,
            IsHistory = review.Status == 2 || cycleEnded
        };
    }

    public async Task<ApiResponse<List<PerformanceReviewListItemResponse>>> GetMyReviewsAsync(Guid userId)
    {
        try
        {
            var employee = await _employeeRepository.GetByUserIdAsync(userId);

            if (employee == null)
            {
                return new ApiResponse<List<PerformanceReviewListItemResponse>>
                {
                    Success = false,
                    Message = "Employee is not found."
                };
            }

            var reviews = await _repository.GetOrCreateReviewsForEmployeeAsync(employee.Id);

            var response = reviews
                .Select(x => MapListItem(x, forManagerList: false))
                .ToList();

            return new ApiResponse<List<PerformanceReviewListItemResponse>>
            {
                Success = true,
                Message = "Performance reviews retrieved successfully.",
                Data = response
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<List<PerformanceReviewListItemResponse>>
            {
                Success = false,
                Message = "Error while retrieving performance reviews.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    private async Task<PerformanceReviewDetailResponse?> BuildDetailAsync(EmployeePerformanceReview review)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var cycle = review.PerformanceCycle;

        var categories = await _repository.GetTemplateCategoriesAsync(review.Employee.DepartmentId);
        var skillReviews = await _repository.GetSkillReviewsAsync(review.Id);

        var categoryResponses = categories.Select(category => new PerformanceReviewCategoryResponse
        {
            CategoryId = category.Id,
            CategoryName = category.CategoryName,
            Weightage = category.Weightage,
            Skills = category.PerformanceSkills
                .Where(s => s.IsActive)
                .OrderBy(s => s.DisplayOrder)
                .Select(skill =>
                {
                    var existing = skillReviews.FirstOrDefault(x => x.PerformanceSkillId == skill.Id);

                    return new PerformanceReviewSkillResponse
                    {
                        SkillId = skill.Id,
                        SkillName = skill.SkillName,
                        Description = skill.Description,
                        Weightage = skill.Weightage,
                        EmployeeRating = existing?.EmployeeRating,
                        EmployeeComment = existing?.EmployeeComment,
                        ManagerRating = existing?.ManagerRating,
                        ManagerComment = existing?.ManagerComment
                    };
                })
                .ToList()
        }).ToList();

        return new PerformanceReviewDetailResponse
        {
            Id = review.Id,
            CycleName = cycle.CycleName,
            ReviewPeriodStart = cycle.ReviewPeriodStart,
            ReviewPeriodEnd = cycle.ReviewPeriodEnd,
            EmployeeReviewStart = cycle.EmployeeReviewStart,
            EmployeeReviewEnd = cycle.EmployeeReviewEnd,
            ManagerReviewStart = cycle.ManagerReviewStart,
            ManagerReviewEnd = cycle.ManagerReviewEnd,
            EmployeeName = $"{review.Employee.User.FirstName} {review.Employee.User.LastName}",
            TemplateName = "",
            IsEmployeeWindowOpen = today >= cycle.EmployeeReviewStart && today <= cycle.EmployeeReviewEnd,
            IsManagerWindowOpen = today >= cycle.ManagerReviewStart && today <= cycle.ManagerReviewEnd,
            IsEmployeeSubmitted = review.Status >= 1,
            IsManagerCompleted = review.Status == 2,
            OverallEmployeeComment = review.OverallEmployeeComment,
            OverallManagerComment = review.OverallManagerComment,
            Categories = categoryResponses
        };
    }

    public async Task<ApiResponse<PerformanceReviewDetailResponse>> GetMyReviewDetailAsync(Guid userId, Guid reviewId)
    {
        try
        {
            var employee = await _employeeRepository.GetByUserIdAsync(userId);

            if (employee == null)
            {
                return new ApiResponse<PerformanceReviewDetailResponse> { Success = false, Message = "Employee is not found." };
            }

            var review = await _repository.GetByIdAsync(reviewId);

            if (review == null || review.EmployeeId != employee.Id)
            {
                return new ApiResponse<PerformanceReviewDetailResponse> { Success = false, Message = "Performance review not found." };
            }

            var detail = await BuildDetailAsync(review);

            return new ApiResponse<PerformanceReviewDetailResponse>
            {
                Success = true,
                Message = "Performance review retrieved successfully.",
                Data = detail
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<PerformanceReviewDetailResponse>
            {
                Success = false,
                Message = "Error while retrieving performance review.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<string>> SubmitEmployeeReviewAsync(Guid userId, Guid reviewId, SubmitEmployeePerformanceReviewDto request)
    {
        try
        {
            var employee = await _employeeRepository.GetByUserIdAsync(userId);

            if (employee == null)
            {
                return new ApiResponse<string> { Success = false, Message = "Employee is not found." };
            }

            var review = await _repository.GetByIdAsync(reviewId);

            if (review == null || review.EmployeeId != employee.Id)
            {
                return new ApiResponse<string> { Success = false, Message = "Performance review not found." };
            }

            if (review.Status >= 1)
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "This self review has already been submitted and can no longer be edited."
                };
            }

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            if (today < review.PerformanceCycle.EmployeeReviewStart || today > review.PerformanceCycle.EmployeeReviewEnd)
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "This review is only editable during the employee review period."
                };
            }

            await _repository.SaveEmployeeReviewAsync(review, request.SkillRatings, request.OverallComment, request.IsDraft);

            return new ApiResponse<string>
            {
                Success = true,
                Message = request.IsDraft
                    ? "Self review saved as draft."
                    : "Self review submitted successfully."
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<string>
            {
                Success = false,
                Message = "Error while submitting self review.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<List<PerformanceReviewListItemResponse>>> GetMyTeamReviewsAsync(Guid userId)
    {
        try
        {
            var manager = await _employeeRepository.GetByUserIdAsync(userId);

            if (manager == null)
            {
                return new ApiResponse<List<PerformanceReviewListItemResponse>>
                {
                    Success = false,
                    Message = "Employee is not found."
                };
            }

            var reviews = await _repository.GetTeamReviewsAsync(manager.Id);

            var response = reviews
                .Select(x => MapListItem(x, forManagerList: true))
                .ToList();

            return new ApiResponse<List<PerformanceReviewListItemResponse>>
            {
                Success = true,
                Message = "Team performance reviews retrieved successfully.",
                Data = response
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<List<PerformanceReviewListItemResponse>>
            {
                Success = false,
                Message = "Error while retrieving team performance reviews.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<PerformanceReviewDetailResponse>> GetTeamReviewDetailAsync(Guid userId, Guid reviewId)
    {
        try
        {
            var manager = await _employeeRepository.GetByUserIdAsync(userId);

            if (manager == null)
            {
                return new ApiResponse<PerformanceReviewDetailResponse> { Success = false, Message = "Employee is not found." };
            }

            var review = await _repository.GetByIdAsync(reviewId);

            if (review == null || review.ManagerId != manager.Id)
            {
                return new ApiResponse<PerformanceReviewDetailResponse> { Success = false, Message = "Performance review not found." };
            }

            var detail = await BuildDetailAsync(review);

            return new ApiResponse<PerformanceReviewDetailResponse>
            {
                Success = true,
                Message = "Performance review retrieved successfully.",
                Data = detail
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<PerformanceReviewDetailResponse>
            {
                Success = false,
                Message = "Error while retrieving performance review.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<string>> SubmitManagerReviewAsync(Guid userId, Guid reviewId, SubmitManagerPerformanceReviewDto request)
    {
        try
        {
            var manager = await _employeeRepository.GetByUserIdAsync(userId);

            if (manager == null)
            {
                return new ApiResponse<string> { Success = false, Message = "Employee is not found." };
            }

            var review = await _repository.GetByIdAsync(reviewId);

            if (review == null || review.ManagerId != manager.Id)
            {
                return new ApiResponse<string> { Success = false, Message = "Performance review not found." };
            }

            if (review.Status == 2)
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "This manager review has already been completed and can no longer be edited."
                };
            }

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            if (today < review.PerformanceCycle.ManagerReviewStart || today > review.PerformanceCycle.ManagerReviewEnd)
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "This review is only editable during the manager review period."
                };
            }

            await _repository.SaveManagerReviewAsync(review, request.SkillRatings, request.OverallComment, request.IsDraft);

            return new ApiResponse<string>
            {
                Success = true,
                Message = request.IsDraft
                    ? "Manager review saved as draft."
                    : "Manager review submitted successfully."
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<string>
            {
                Success = false,
                Message = "Error while submitting manager review.",
                Errors = new List<string> { ex.Message }
            };
        }
    }
}
