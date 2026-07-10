using HRM.API.Models;
using HRM.API.Repositories;
using HRM.API.Responses;

namespace HRM.API.Services;

public interface IPerformanceTemplateService
{
    Task<ApiResponse<Guid>> CreateAsync(CreatePerformanceTemplateDto request, Guid companyId, Guid userId);

    Task<ApiResponse<bool>> UpdateAsync(UpdatePerformanceTemplateDto request, Guid companyId, Guid userId);

    Task<ApiResponse<bool>> DeleteAsync(Guid id, Guid userId);

    Task<ApiResponse<List<PerformanceTemplateResponse>>> GetAllAsync(Guid companyId);

    Task<ApiResponse<PerformanceTemplateResponse>> GetByIdAsync(Guid id);
}

public class PerformanceTemplateService : IPerformanceTemplateService
{
    private readonly IPerformanceTemplateRepository _performanceTemplateRepository;

    public PerformanceTemplateService(IPerformanceTemplateRepository performanceTemplateRepository)
    {
        _performanceTemplateRepository = performanceTemplateRepository;
    }

    private static PerformanceTemplateResponse ToResponse(PerformanceTemplate template)
    {
        return new PerformanceTemplateResponse
        {
            Id = template.Id,
            TemplateName = template.TemplateName,
            Description = template.Description,
            DepartmentId = template.DepartmentId,
            DepartmentName = template.Department?.DepartmentName ?? string.Empty,
            IsActive = template.IsActive
        };
    }

    public async Task<ApiResponse<List<PerformanceTemplateResponse>>> GetAllAsync(Guid companyId)
    {
        try
        {
            var data = await _performanceTemplateRepository.GetAllByCompanyAsync(companyId);

            var result = data.Select(ToResponse).ToList();

            return new ApiResponse<List<PerformanceTemplateResponse>>
            {
                Success = true,
                Message = "Performance templates retrieved successfully.",
                Data = result
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<List<PerformanceTemplateResponse>>
            {
                Success = false,
                Message = "An error occurred while retrieving performance templates.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<PerformanceTemplateResponse>> GetByIdAsync(Guid id)
    {
        try
        {
            var template = await _performanceTemplateRepository.GetByIdAsync(id);

            if (template == null)
            {
                return new ApiResponse<PerformanceTemplateResponse>
                {
                    Success = false,
                    Message = "Performance template not found."
                };
            }

            return new ApiResponse<PerformanceTemplateResponse>
            {
                Success = true,
                Message = "Performance template retrieved successfully.",
                Data = ToResponse(template)
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<PerformanceTemplateResponse>
            {
                Success = false,
                Message = "An error occurred while retrieving the performance template.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<Guid>> CreateAsync(CreatePerformanceTemplateDto request, Guid companyId, Guid userId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.TemplateName))
            {
                return new ApiResponse<Guid>
                {
                    Success = false,
                    Message = "Template name is required."
                };
            }

            var departmentBelongsToCompany = await _performanceTemplateRepository
                .DepartmentBelongsToCompanyAsync(request.DepartmentId, companyId);

            if (!departmentBelongsToCompany)
            {
                return new ApiResponse<Guid>
                {
                    Success = false,
                    Message = "Selected department was not found in your company."
                };
            }

            var departmentAlreadyHasTemplate = await _performanceTemplateRepository
                .ExistsByDepartmentAsync(request.DepartmentId);

            if (departmentAlreadyHasTemplate)
            {
                return new ApiResponse<Guid>
                {
                    Success = false,
                    Message = "A performance template already exists for this department."
                };
            }

            var performanceTemplate = new PerformanceTemplate
            {
                Id = Guid.NewGuid(),
                TemplateName = request.TemplateName,
                Description = request.Description,
                DepartmentId = request.DepartmentId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            await _performanceTemplateRepository.AddAsync(performanceTemplate);

            await _performanceTemplateRepository.SaveChangesAsync();

            return new ApiResponse<Guid>
            {
                Success = true,
                Message = "Performance template created successfully.",
                Data = performanceTemplate.Id
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<Guid>
            {
                Success = false,
                Message = "An error occurred while creating the performance template.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<bool>> UpdateAsync(UpdatePerformanceTemplateDto request, Guid companyId, Guid userId)
    {
        try
        {
            var performanceTemplate = await _performanceTemplateRepository.GetByIdAsync(request.Id);

            if (performanceTemplate == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Performance template not found."
                };
            }

            if (string.IsNullOrWhiteSpace(request.TemplateName))
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Template name is required."
                };
            }

            var departmentBelongsToCompany = await _performanceTemplateRepository
                .DepartmentBelongsToCompanyAsync(request.DepartmentId, companyId);

            if (!departmentBelongsToCompany)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Selected department was not found in your company."
                };
            }

            var departmentAlreadyHasTemplate = await _performanceTemplateRepository
                .ExistsByDepartmentAsync(request.DepartmentId, request.Id);

            if (departmentAlreadyHasTemplate)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "A performance template already exists for this department."
                };
            }

            performanceTemplate.TemplateName = request.TemplateName;
            performanceTemplate.Description = request.Description;
            performanceTemplate.DepartmentId = request.DepartmentId;
            performanceTemplate.UpdatedAt = DateTime.UtcNow;
            performanceTemplate.UpdatedBy = userId;

            _performanceTemplateRepository.Update(performanceTemplate);

            await _performanceTemplateRepository.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Performance template updated successfully.",
                Data = true
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "An error occurred while updating the performance template.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid id, Guid userId)
    {
        try
        {
            var performanceTemplate = await _performanceTemplateRepository.GetByIdAsync(id);

            if (performanceTemplate == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Performance template not found."
                };
            }

            performanceTemplate.IsActive = false;
            performanceTemplate.UpdatedAt = DateTime.UtcNow;
            performanceTemplate.UpdatedBy = userId;

            _performanceTemplateRepository.Update(performanceTemplate);

            await _performanceTemplateRepository.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Performance template deleted successfully.",
                Data = true
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "An error occurred while deleting the performance template.",
                Errors = new List<string> { ex.Message }
            };
        }
    }
}
