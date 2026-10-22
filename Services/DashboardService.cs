
using HRM.API.Repositories;
using HRM.API.Responses;
public interface IDashboardService
{
    Task<ApiResponse<DashboardResponse>> GetDashboardAsync(Guid companyId);
    Task<List<HeadcountChartResponse>> GetHeadcountTrendAsync(Guid companyId);
    Task<ApiResponse<List<DepartmentEmployeeCountResponse>>>
    GetDepartmentEmployeeCountsAsync();
    Task<ApiResponse<DashboardTodayAttendanceResponse>> GetTodayAttendanceSummaryAsync(Guid companyId);
    Task<ApiResponse<PendingApprovalsResponse>> GetPendingApprovalsSummaryAsync(Guid userId);
}
public class DashboardService : IDashboardService
{
    private readonly IDashboardRepository _repository;
    private readonly IEmployeeRepository _employeeRepository;

    public DashboardService(IDashboardRepository repository, IEmployeeRepository employeeRepository)
    {
        _repository = repository;
        _employeeRepository = employeeRepository;
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

    public async Task<ApiResponse<DashboardTodayAttendanceResponse>> GetTodayAttendanceSummaryAsync(Guid companyId)
    {
        try
        {
            var data = await _repository.GetTodayAttendanceSummaryAsync(companyId);

            return new ApiResponse<DashboardTodayAttendanceResponse>
            {
                Success = true,
                Message = "Today's attendance summary retrieved successfully.",
                Data = data
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<DashboardTodayAttendanceResponse>
            {
                Success = false,
                Message = "Failed to fetch today's attendance summary.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<PendingApprovalsResponse>> GetPendingApprovalsSummaryAsync(Guid userId)
    {
        try
        {
            var employee = await _employeeRepository.GetByUserIdAsync(userId);
            if (employee == null)
            {
                return new ApiResponse<PendingApprovalsResponse>
                {
                    Success = false,
                    Message = "Employee not found for the given user ID."
                };
            }

            var data = await _repository.GetPendingApprovalsSummaryAsync(employee.Id);

            return new ApiResponse<PendingApprovalsResponse>
            {
                Success = true,
                Message = "Pending approvals summary retrieved successfully.",
                Data = data
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<PendingApprovalsResponse>
            {
                Success = false,
                Message = "Failed to fetch pending approvals summary.",
                Errors = new List<string> { ex.Message }
            };
        }
    }
}