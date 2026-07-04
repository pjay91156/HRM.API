
using HRM.API.Repositories;
using HRM.API.Responses;
public interface IDashboardService
{
    Task<ApiResponse<DashboardResponse>> GetDashboardAsync(Guid companyId);
    Task<List<HeadcountChartResponse>> GetHeadcountTrendAsync(Guid companyId);
    Task<ApiResponse<List<DepartmentEmployeeCountResponse>>>
    GetDepartmentEmployeeCountsAsync();
}
public class DashboardService : IDashboardService
{
    private readonly IDashboardRepository _repository;

    public DashboardService(IDashboardRepository repository)
    {
        _repository = repository;
    }
    public async Task<ApiResponse<List<DepartmentEmployeeCountResponse>>>
    GetDepartmentEmployeeCountsAsync()
{
    try
    {
        var data =
            await _repository
                .GetDepartmentEmployeeCountsAsync();

        return new ApiResponse<List<DepartmentEmployeeCountResponse>>
        {
            Success = true,
            Message = "Department wise employee count fetched successfully.",
            Data = data
        };
    }
    catch (Exception ex)
    {
        return new ApiResponse<List<DepartmentEmployeeCountResponse>>
        {
            Success = false,
            Message = "Failed to fetch department statistics.",
            Errors = new List<string> { ex.Message }
        };
    }
}

    public async Task<ApiResponse<DashboardResponse>> GetDashboardAsync(Guid companyId)
    {
        try
        {
            var data = await _repository.GetDashboardAsync(companyId);

            return new ApiResponse<DashboardResponse>
            {
                Success = true,
                Message = "Dashboard data retrieved successfully.",
                Data = data
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<DashboardResponse>
            {
                Success = false,
                Message = ex.Message,
                Data = null
            };
        }
    }
    public async Task<List<HeadcountChartResponse>> GetHeadcountTrendAsync(Guid companyId)
    {
        return await _repository.GetHeadcountTrendAsync(companyId);
    }
}