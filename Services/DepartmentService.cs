using HRM.API.Models;
using HRM.API.Repositories;
using HRM.API.Responses;

public interface IDepartmentService
{
    Task<List<DepartmentResponse>> GetDepartmentsByCompanyIdAsync(Guid companyId);
    Task<ApiResponse<string>> AddDepartmentAsync(DepartmentDto request,Guid companyId,Guid userId);
    Task<ApiResponse<bool>> UpdateDepartmentAsync(UpdateDepartmentDto request, Guid userId);
    Task<ApiResponse<bool>> DeleteDepartmentAsync(Guid departmentId);
}
public class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _departmentRepository;

    public DepartmentService(IDepartmentRepository departmentRepository)
    {
        _departmentRepository = departmentRepository;
    }

    public async Task<List<DepartmentResponse>> GetDepartmentsByCompanyIdAsync(Guid companyId)
    {
        return await _departmentRepository.GetDepartmentsByCompanyIdAsync(companyId);
    }
    public async Task<ApiResponse<string>> AddDepartmentAsync(DepartmentDto request,Guid companyId,Guid userId)
    {
         var Id = Guid.NewGuid();
         var department = new Department
        {
            Id = Id,
            CompanyId = companyId,
            DepartmentName = request.DepartmentName,            
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };
        await _departmentRepository.AddDepartmnetAsync(department);          

        return new ApiResponse<string>
        {
            Success = true,
            Message = "Department added successfully.",
            Data = department.Id.ToString()
        };
    }
    public async Task<ApiResponse<bool>> UpdateDepartmentAsync(UpdateDepartmentDto request, Guid userId)
    {
        var department = await _departmentRepository.GetByIdAsync(request.Id);

        if (department == null)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Department not found.",
                Data = false
            };
        }

        department.DepartmentName = request.DepartmentName;
        department.UpdatedAt = DateTime.UtcNow;
        department.UpdatedBy = userId;

        await _departmentRepository.SaveChangesAsync();

        return new ApiResponse<bool>
        {
            Success = true,
            Message = "Department updated successfully.",
            Data = true
        };
    }

    public async Task<ApiResponse<bool>> DeleteDepartmentAsync(Guid departmentId)
    {
        var deleted = await _departmentRepository.DeleteDepartmentAsync(departmentId);

        if (!deleted)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Deparment not found.",
                Data = false
            };
        }

        return new ApiResponse<bool>
        {
            Success = true,
            Message = "Department deleted successfully.",
            Data = true
        };
    }
}