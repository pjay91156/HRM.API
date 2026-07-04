using HRM.API.Data;
using HRM.API.Models;
using HRM.API.Repositories;
using HRM.API.Responses;

public interface IDesignationService
{
    Task<ApiResponse<IEnumerable<DesignationResponse>>> GetDesignationsByDepartmentIdAsync(Guid departmentId);
    Task<ApiResponse<Guid?>> AddDesignationAsync(DesignationDto request,Guid userId);
    Task<ApiResponse<IEnumerable<DesignationResponse>>> GetDesignationsAsync();
    Task<ApiResponse<object>> DeleteDesignationAsync(Guid designationId, Guid userId);
}

public class DesignationService : IDesignationService
{
    private readonly IDesignationRepository _designationRepository;
    private readonly ApplicationDbContext _context;
   

    public DesignationService(IDesignationRepository designationRepository,ApplicationDbContext context)
    {
        _designationRepository = designationRepository;
        _context=context;
    }

    public async Task<ApiResponse<IEnumerable<DesignationResponse>>> GetDesignationsByDepartmentIdAsync(Guid departmentId)
    {
        var designations = await _designationRepository.GetDesignationsByDepartmentIdAsync(departmentId);

        return new ApiResponse<IEnumerable<DesignationResponse>>
        {
            Success = true,
            Message = "Designations fetched successfully.",
            Data = designations
        };
    }
    public async Task<ApiResponse<Guid?>> AddDesignationAsync(DesignationDto request,Guid userId)
{
    try
    {
        var designation = new Designation
        {
            Id=new Guid(),
             DesignationName= request.Name,
             CreatedBy=userId,
             IsActive=true,
            DepartmentId = request.DepartmentId
        };

        _context.Designations.Add(designation);
        await _context.SaveChangesAsync();

        return new ApiResponse<Guid?>
        {
            Success = true,
            Message = "Designation added successfully.",
            Data = designation.Id
        };
    }
    catch (Exception ex)
    {
        return new ApiResponse<Guid?>
        {
            Success = false,
            Message = ex.Message,
            Data = null
        };
    }
}
  public async Task<ApiResponse<IEnumerable<DesignationResponse>>> GetDesignationsAsync()
    {
        var designations = await _designationRepository.GetDesignationsAsync();

        return new ApiResponse<IEnumerable<DesignationResponse>>
        {
            Success = true,
            Message = "Designations fetched successfully.",
            Data = designations
        };
    }
    public async Task<ApiResponse<object>> DeleteDesignationAsync(Guid designationId, Guid userId)
{
    var designation = await _designationRepository.GetByIdAsync(designationId);

    if (designation == null)
    {
        return new ApiResponse<object>
        {
            Success = false,
            Message = "Designation not found."
        };
    }

    designation.IsActive = false;
    designation.UpdatedBy = userId;
    designation.UpdatedAt = DateTime.UtcNow;

    await _designationRepository.SaveChangesAsync();

    return new ApiResponse<object>
    {
        Success = true,
        Message = "Designation deactivated successfully."
    };
}
}