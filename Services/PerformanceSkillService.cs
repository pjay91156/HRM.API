using HRM.API.Models;
using HRM.API.Repositories;
using HRM.API.Responses;

namespace HRM.API.Services;

public interface IPerformanceSkillService
{
    Task<ApiResponse<Guid>> CreateAsync(CreatePerformanceSkillDto request, Guid companyId, Guid userId);

    Task<ApiResponse<bool>> UpdateAsync(UpdatePerformanceSkillDto request, Guid companyId, Guid userId);

    Task<ApiResponse<bool>> DeleteAsync(Guid id, Guid userId);

    Task<ApiResponse<List<PerformanceSkillResponse>>> GetByCategoryAsync(Guid categoryId, Guid companyId);

    Task<ApiResponse<PerformanceSkillResponse>> GetByIdAsync(Guid id);
}

public class PerformanceSkillService : IPerformanceSkillService
{
    private const int MaxSkillNameLength = 150;
    private const int MaxDescriptionLength = 500;
    private const decimal MinWeightage = 0m;
    private const decimal MaxWeightage = 100m;
    private const decimal MaxTotalWeightage = 100m;

    private readonly IPerformanceSkillRepository _performanceSkillRepository;

    public PerformanceSkillService(IPerformanceSkillRepository performanceSkillRepository)
    {
        _performanceSkillRepository = performanceSkillRepository;
    }

    private static PerformanceSkillResponse ToResponse(PerformanceSkill skill)
    {
        return new PerformanceSkillResponse
        {
            Id = skill.Id,
            PerformanceCategoryId = skill.PerformanceCategoryId,
            SkillName = skill.SkillName,
            Description = skill.Description,
            Weightage = skill.Weightage,
            DisplayOrder = skill.DisplayOrder,
            IsActive = skill.IsActive
        };
    }

    public async Task<ApiResponse<List<PerformanceSkillResponse>>> GetByCategoryAsync(Guid categoryId, Guid companyId)
    {
        try
        {
            var categoryBelongsToCompany = await _performanceSkillRepository
                .CategoryBelongsToCompanyAsync(categoryId, companyId);

            if (!categoryBelongsToCompany)
            {
                return new ApiResponse<List<PerformanceSkillResponse>>
                {
                    Success = false,
                    Message = "Performance category was not found in your company."
                };
            }

            var data = await _performanceSkillRepository.GetByCategoryIdAsync(categoryId);

            var result = data.Select(ToResponse).ToList();

            return new ApiResponse<List<PerformanceSkillResponse>>
            {
                Success = true,
                Message = "Performance skills retrieved successfully.",
                Data = result
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<List<PerformanceSkillResponse>>
            {
                Success = false,
                Message = "An error occurred while retrieving performance skills.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<PerformanceSkillResponse>> GetByIdAsync(Guid id)
    {
        try
        {
            var skill = await _performanceSkillRepository.GetByIdAsync(id);

            if (skill == null)
            {
                return new ApiResponse<PerformanceSkillResponse>
                {
                    Success = false,
                    Message = "Performance skill not found."
                };
            }

            return new ApiResponse<PerformanceSkillResponse>
            {
                Success = true,
                Message = "Performance skill retrieved successfully.",
                Data = ToResponse(skill)
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<PerformanceSkillResponse>
            {
                Success = false,
                Message = "An error occurred while retrieving the performance skill.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<Guid>> CreateAsync(CreatePerformanceSkillDto request, Guid companyId, Guid userId)
    {
        try
        {
            var categoryBelongsToCompany = await _performanceSkillRepository
                .CategoryBelongsToCompanyAsync(request.PerformanceCategoryId, companyId);

            if (!categoryBelongsToCompany)
            {
                return new ApiResponse<Guid>
                {
                    Success = false,
                    Message = "Performance category was not found in your company."
                };
            }

            var nameValidation = ValidateSkillName(request.SkillName, out var trimmedName);

            if (nameValidation != null)
            {
                return new ApiResponse<Guid> { Success = false, Message = nameValidation };
            }

            var descriptionValidation = ValidateDescription(request.Description);

            if (descriptionValidation != null)
            {
                return new ApiResponse<Guid> { Success = false, Message = descriptionValidation };
            }

            var nameExists = await _performanceSkillRepository
                .ExistsByNameAsync(request.PerformanceCategoryId, trimmedName);

            if (nameExists)
            {
                return new ApiResponse<Guid>
                {
                    Success = false,
                    Message = "A skill with this name already exists in this category."
                };
            }

            var weightageValidation = ValidateWeightage(request.Weightage);

            if (weightageValidation != null)
            {
                return new ApiResponse<Guid> { Success = false, Message = weightageValidation };
            }

            var existingTotal = await _performanceSkillRepository
                .GetTotalWeightageAsync(request.PerformanceCategoryId);

            if (existingTotal + request.Weightage > MaxTotalWeightage)
            {
                var remaining = MaxTotalWeightage - existingTotal;

                return new ApiResponse<Guid>
                {
                    Success = false,
                    Message = $"Total skill weightage cannot exceed 100%. Remaining allowed weightage is {remaining}%."
                };
            }

            var displayOrderExists = await _performanceSkillRepository
                .ExistsByDisplayOrderAsync(request.PerformanceCategoryId, request.DisplayOrder);

            if (displayOrderExists)
            {
                return new ApiResponse<Guid>
                {
                    Success = false,
                    Message = $"Display order {request.DisplayOrder} is already used in this category."
                };
            }

            var performanceSkill = new PerformanceSkill
            {
                Id = Guid.NewGuid(),
                PerformanceCategoryId = request.PerformanceCategoryId,
                SkillName = trimmedName,
                Description = request.Description,
                Weightage = request.Weightage,
                DisplayOrder = request.DisplayOrder,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            await _performanceSkillRepository.AddAsync(performanceSkill);

            await _performanceSkillRepository.SaveChangesAsync();

            return new ApiResponse<Guid>
            {
                Success = true,
                Message = "Performance skill created successfully.",
                Data = performanceSkill.Id
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<Guid>
            {
                Success = false,
                Message = "An error occurred while creating the performance skill.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<bool>> UpdateAsync(UpdatePerformanceSkillDto request, Guid companyId, Guid userId)
    {
        try
        {
            var performanceSkill = await _performanceSkillRepository.GetByIdAsync(request.Id);

            if (performanceSkill == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Performance skill not found."
                };
            }

            var categoryBelongsToCompany = await _performanceSkillRepository
                .CategoryBelongsToCompanyAsync(performanceSkill.PerformanceCategoryId, companyId);

            if (!categoryBelongsToCompany)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Performance category was not found in your company."
                };
            }

            var nameValidation = ValidateSkillName(request.SkillName, out var trimmedName);

            if (nameValidation != null)
            {
                return new ApiResponse<bool> { Success = false, Message = nameValidation };
            }

            var descriptionValidation = ValidateDescription(request.Description);

            if (descriptionValidation != null)
            {
                return new ApiResponse<bool> { Success = false, Message = descriptionValidation };
            }

            var nameExists = await _performanceSkillRepository
                .ExistsByNameAsync(performanceSkill.PerformanceCategoryId, trimmedName, request.Id);

            if (nameExists)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "A skill with this name already exists in this category."
                };
            }

            var weightageValidation = ValidateWeightage(request.Weightage);

            if (weightageValidation != null)
            {
                return new ApiResponse<bool> { Success = false, Message = weightageValidation };
            }

            var existingTotal = await _performanceSkillRepository
                .GetTotalWeightageAsync(performanceSkill.PerformanceCategoryId, request.Id);

            if (existingTotal + request.Weightage > MaxTotalWeightage)
            {
                var remaining = MaxTotalWeightage - existingTotal;

                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = $"Total skill weightage cannot exceed 100%. Remaining allowed weightage is {remaining}%."
                };
            }

            var displayOrderExists = await _performanceSkillRepository
                .ExistsByDisplayOrderAsync(performanceSkill.PerformanceCategoryId, request.DisplayOrder, request.Id);

            if (displayOrderExists)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = $"Display order {request.DisplayOrder} is already used in this category."
                };
            }

            performanceSkill.SkillName = trimmedName;
            performanceSkill.Description = request.Description;
            performanceSkill.Weightage = request.Weightage;
            performanceSkill.DisplayOrder = request.DisplayOrder;
            performanceSkill.UpdatedAt = DateTime.UtcNow;
            performanceSkill.UpdatedBy = userId;

            _performanceSkillRepository.Update(performanceSkill);

            await _performanceSkillRepository.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Performance skill updated successfully.",
                Data = true
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "An error occurred while updating the performance skill.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id, Guid userId)
    {
        try
        {
            var performanceSkill = await _performanceSkillRepository.GetByIdAsync(id);

            if (performanceSkill == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Performance skill not found."
                };
            }

            performanceSkill.IsActive = false;
            performanceSkill.UpdatedAt = DateTime.UtcNow;
            performanceSkill.UpdatedBy = userId;

            _performanceSkillRepository.Update(performanceSkill);

            await _performanceSkillRepository.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Performance skill deleted successfully.",
                Data = true
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "An error occurred while deleting the performance skill.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    private static string? ValidateSkillName(string? skillName, out string trimmedName)
    {
        trimmedName = skillName?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(trimmedName))
        {
            return "Skill name is required and cannot contain only spaces.";
        }

        if (trimmedName.Length > MaxSkillNameLength)
        {
            return $"Skill name cannot exceed {MaxSkillNameLength} characters.";
        }

        return null;
    }

    private static string? ValidateDescription(string? description)
    {
        if (!string.IsNullOrEmpty(description) && description.Length > MaxDescriptionLength)
        {
            return $"Description cannot exceed {MaxDescriptionLength} characters.";
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
