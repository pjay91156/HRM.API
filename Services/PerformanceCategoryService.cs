using HRM.API.Models;
using HRM.API.Repositories;
using HRM.API.Responses;

namespace HRM.API.Services;

public interface IPerformanceCategoryService
{
    Task<ApiResponse<Guid>> CreateAsync(CreatePerformanceCategoryDto request, Guid companyId, Guid userId);

    Task<ApiResponse<bool>> UpdateAsync(UpdatePerformanceCategoryDto request, Guid companyId, Guid userId);

    Task<ApiResponse<bool>> DeleteAsync(Guid id, Guid userId);

    Task<ApiResponse<List<PerformanceCategoryResponse>>> GetByTemplateAsync(Guid templateId, Guid companyId);

    Task<ApiResponse<PerformanceCategoryResponse>> GetByIdAsync(Guid id);
}

public class PerformanceCategoryService : IPerformanceCategoryService
{
    private const int MaxCategoryNameLength = 100;
    private const decimal MinWeightage = 0m;
    private const decimal MaxWeightage = 100m;
    private const decimal MaxTotalWeightage = 100m;

    private readonly IPerformanceCategoryRepository _performanceCategoryRepository;

    public PerformanceCategoryService(IPerformanceCategoryRepository performanceCategoryRepository)
    {
        _performanceCategoryRepository = performanceCategoryRepository;
    }

    private static PerformanceCategoryResponse ToResponse(PerformanceCategory category)
    {
        return new PerformanceCategoryResponse
        {
            Id = category.Id,
            PerformanceTemplateId = category.PerformanceTemplateId,
            CategoryName = category.CategoryName,
            Weightage = category.Weightage,
            DisplayOrder = category.DisplayOrder,
            IsActive = category.IsActive
        };
    }

    public async Task<ApiResponse<List<PerformanceCategoryResponse>>> GetByTemplateAsync(Guid templateId, Guid companyId)
    {
        try
        {
            var templateBelongsToCompany = await _performanceCategoryRepository
                .TemplateBelongsToCompanyAsync(templateId, companyId);

            if (!templateBelongsToCompany)
            {
                return new ApiResponse<List<PerformanceCategoryResponse>>
                {
                    Success = false,
                    Message = "Performance template was not found in your company."
                };
            }

            var data = await _performanceCategoryRepository.GetByTemplateIdAsync(templateId);

            var result = data.Select(ToResponse).ToList();

            return new ApiResponse<List<PerformanceCategoryResponse>>
            {
                Success = true,
                Message = "Performance categories retrieved successfully.",
                Data = result
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<List<PerformanceCategoryResponse>>
            {
                Success = false,
                Message = "An error occurred while retrieving performance categories.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<PerformanceCategoryResponse>> GetByIdAsync(Guid id)
    {
        try
        {
            var category = await _performanceCategoryRepository.GetByIdAsync(id);

            if (category == null)
            {
                return new ApiResponse<PerformanceCategoryResponse>
                {
                    Success = false,
                    Message = "Performance category not found."
                };
            }

            return new ApiResponse<PerformanceCategoryResponse>
            {
                Success = true,
                Message = "Performance category retrieved successfully.",
                Data = ToResponse(category)
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<PerformanceCategoryResponse>
            {
                Success = false,
                Message = "An error occurred while retrieving the performance category.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<Guid>> CreateAsync(CreatePerformanceCategoryDto request, Guid companyId, Guid userId)
    {
        try
        {
            var templateBelongsToCompany = await _performanceCategoryRepository
                .TemplateBelongsToCompanyAsync(request.PerformanceTemplateId, companyId);

            if (!templateBelongsToCompany)
            {
                return new ApiResponse<Guid>
                {
                    Success = false,
                    Message = "Performance template was not found in your company."
                };
            }

            var nameValidation = ValidateCategoryName(request.CategoryName, out var trimmedName);

            if (nameValidation != null)
            {
                return new ApiResponse<Guid> { Success = false, Message = nameValidation };
            }

            var nameExists = await _performanceCategoryRepository
                .ExistsByNameAsync(request.PerformanceTemplateId, trimmedName);

            if (nameExists)
            {
                return new ApiResponse<Guid>
                {
                    Success = false,
                    Message = "A category with this name already exists in this template."
                };
            }

            var weightageValidation = ValidateWeightage(request.Weightage);

            if (weightageValidation != null)
            {
                return new ApiResponse<Guid> { Success = false, Message = weightageValidation };
            }

            var existingTotal = await _performanceCategoryRepository
                .GetTotalWeightageAsync(request.PerformanceTemplateId);

            if (existingTotal + request.Weightage > MaxTotalWeightage)
            {
                var remaining = MaxTotalWeightage - existingTotal;

                return new ApiResponse<Guid>
                {
                    Success = false,
                    Message = $"Total category weightage cannot exceed 100%. Remaining allowed weightage is {remaining}%."
                };
            }

            var displayOrderExists = await _performanceCategoryRepository
                .ExistsByDisplayOrderAsync(request.PerformanceTemplateId, request.DisplayOrder);

            if (displayOrderExists)
            {
                return new ApiResponse<Guid>
                {
                    Success = false,
                    Message = $"Display order {request.DisplayOrder} is already used in this template."
                };
            }

            var performanceCategory = new PerformanceCategory
            {
                Id = Guid.NewGuid(),
                PerformanceTemplateId = request.PerformanceTemplateId,
                CategoryName = trimmedName,
                Weightage = request.Weightage,
                DisplayOrder = request.DisplayOrder,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            await _performanceCategoryRepository.AddAsync(performanceCategory);

            await _performanceCategoryRepository.SaveChangesAsync();

            return new ApiResponse<Guid>
            {
                Success = true,
                Message = "Performance category created successfully.",
                Data = performanceCategory.Id
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<Guid>
            {
                Success = false,
                Message = "An error occurred while creating the performance category.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<bool>> UpdateAsync(UpdatePerformanceCategoryDto request, Guid companyId, Guid userId)
    {
        try
        {
            var performanceCategory = await _performanceCategoryRepository.GetByIdAsync(request.Id);

            if (performanceCategory == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Performance category not found."
                };
            }

            var templateBelongsToCompany = await _performanceCategoryRepository
                .TemplateBelongsToCompanyAsync(performanceCategory.PerformanceTemplateId, companyId);

            if (!templateBelongsToCompany)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Performance template was not found in your company."
                };
            }

            var nameValidation = ValidateCategoryName(request.CategoryName, out var trimmedName);

            if (nameValidation != null)
            {
                return new ApiResponse<bool> { Success = false, Message = nameValidation };
            }

            var nameExists = await _performanceCategoryRepository
                .ExistsByNameAsync(performanceCategory.PerformanceTemplateId, trimmedName, request.Id);

            if (nameExists)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "A category with this name already exists in this template."
                };
            }

            var weightageValidation = ValidateWeightage(request.Weightage);

            if (weightageValidation != null)
            {
                return new ApiResponse<bool> { Success = false, Message = weightageValidation };
            }

            var existingTotal = await _performanceCategoryRepository
                .GetTotalWeightageAsync(performanceCategory.PerformanceTemplateId, request.Id);

            if (existingTotal + request.Weightage > MaxTotalWeightage)
            {
                var remaining = MaxTotalWeightage - existingTotal;

                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = $"Total category weightage cannot exceed 100%. Remaining allowed weightage is {remaining}%."
                };
            }

            var displayOrderExists = await _performanceCategoryRepository
                .ExistsByDisplayOrderAsync(performanceCategory.PerformanceTemplateId, request.DisplayOrder, request.Id);

            if (displayOrderExists)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = $"Display order {request.DisplayOrder} is already used in this template."
                };
            }

            performanceCategory.CategoryName = trimmedName;
            performanceCategory.Weightage = request.Weightage;
            performanceCategory.DisplayOrder = request.DisplayOrder;
            performanceCategory.UpdatedAt = DateTime.UtcNow;
            performanceCategory.UpdatedBy = userId;

            _performanceCategoryRepository.Update(performanceCategory);

            await _performanceCategoryRepository.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Performance category updated successfully.",
                Data = true
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "An error occurred while updating the performance category.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id, Guid userId)
    {
        try
        {
            var performanceCategory = await _performanceCategoryRepository.GetByIdAsync(id);

            if (performanceCategory == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Performance category not found."
                };
            }

            performanceCategory.IsActive = false;
            performanceCategory.UpdatedAt = DateTime.UtcNow;
            performanceCategory.UpdatedBy = userId;

            _performanceCategoryRepository.Update(performanceCategory);

            await _performanceCategoryRepository.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Performance category deleted successfully.",
                Data = true
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "An error occurred while deleting the performance category.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    private static string? ValidateCategoryName(string? categoryName, out string trimmedName)
    {
        trimmedName = categoryName?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(trimmedName))
        {
            return "Category name is required and cannot contain only spaces.";
        }

        if (trimmedName.Length > MaxCategoryNameLength)
        {
            return $"Category name cannot exceed {MaxCategoryNameLength} characters.";
        }

        return null;
    }

    private static string? ValidateWeightage(decimal weightage)
    {
        if (weightage <= MinWeightage)
        {
            return "Weightage must be greater than 0.";
        }

        if (weightage > MaxWeightage)
        {
            return "Weightage cannot exceed 100%.";
        }

        return null;
    }
}
