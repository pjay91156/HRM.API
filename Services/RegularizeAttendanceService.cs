using HRM.API.Responses;
using HRM.API.Models;
using HRM.API.Repositories;
namespace HRM.API.Services;
public interface IRegularizeAttendanceService
{
    Task<ApiResponse<AttendanceSessionsResponse>>
        GetAttendanceSessionsAsync(
            Guid employeeId,
            DateOnly attendanceDate);
           Task<ApiResponse<object>> CreateRegularizationRequestAsync(
    RegularizationRequest request,
    Guid userId);
    Task<ApiResponse<List<ManagerPendingRegularizationResponse>>> GetPendingRequestsAsync(Guid userId);
}


public class RegularizeAttendanceService : IRegularizeAttendanceService
{
    private readonly IRegularizeAttendanceRepository _regularizeAttendanceRepository;
     private readonly IEmployeeRepository _employeeRepository;

    public RegularizeAttendanceService(
        IRegularizeAttendanceRepository regularizeAttendanceRepository,IEmployeeRepository employeeRepository)
    {
        _regularizeAttendanceRepository = regularizeAttendanceRepository;
        _employeeRepository=employeeRepository;
    }

    public async Task<ApiResponse<AttendanceSessionsResponse>>
        GetAttendanceSessionsAsync(
            Guid userId,
            DateOnly attendanceDate)
    {
        try
        {
           var employee=await _employeeRepository.GetByUserIdAsync(userId);
            var attendance =
                await _regularizeAttendanceRepository
                    .GetAttendanceSessionsAsync(
                        employee.Id,
                        attendanceDate);

            if (attendance == null)
            {
                return new ApiResponse<AttendanceSessionsResponse>
                {
                    Success = false,
                    Message = "Attendance not found.",
                    Errors = new List<string>
                    {
                        "No attendance record found for the selected date."
                    }
                };
            }

            return new ApiResponse<AttendanceSessionsResponse>
            {
                Success = true,
                Message = "Attendance sessions retrieved successfully.",
                Data = attendance
            };
        }
        catch (Exception ex)
        {
            // Ideally, log the exception using ILogger

            return new ApiResponse<AttendanceSessionsResponse>
            {
                Success = false,
                Message = "An unexpected error occurred.",
                Errors = new List<string>
                {
                    ex.Message
                }
            };
        }
    }
  public async Task<ApiResponse<object>> CreateRegularizationRequestAsync(
    RegularizationRequest request,
    Guid userId)
{
    var response = new ApiResponse<object>();

    try
    {
        if (request == null)
        {
            response.Success = false;
            response.Message = "Request cannot be null.";
            return response;
        }

        if (request.SessionChanges == null || !request.SessionChanges.Any())
        {
            response.Success = false;
            response.Message = "Please provide at least one session change.";
            return response;
        }

        var result = await _regularizeAttendanceRepository
            .CreateRegularizationRequestAsync(request, userId);

        if (!result)
        {
            response.Success = false;
            response.Message = "Unable to submit attendance regularization request.";
            return response;
        }

        response.Success = true;
        response.Message = "Attendance regularization request submitted successfully.";
        response.Data = null;

        return response;
    }
    catch (Exception ex)
    {
        response.Success = false;
        response.Message = ex.Message;

        return response;
    }
}
public async Task<ApiResponse<List<ManagerPendingRegularizationResponse>>> GetPendingRequestsAsync(Guid userId)
{
    var response = new ApiResponse<List<ManagerPendingRegularizationResponse>>();

    try
    {
        var employee = await _employeeRepository.GetByUserIdAsync(userId);
        if (employee == null)
        {
            response.Success = false;
            response.Message = "Employee not found for the given user ID.";
            return response;
        }
        var requests = await _regularizeAttendanceRepository
            .GetPendingRequestsAsync(employee.Id);

        response.Success = true;
        response.Message = requests.Any()
            ? "Pending regularization requests retrieved successfully."
            : "No pending regularization requests found.";

        response.Data = requests;

        return response;
    }
    catch (Exception ex)
    {
       

        response.Success = false;
        response.Message = "Failed to retrieve pending regularization requests.";
        response.Errors = new List<string>
        {
            ex.Message
            // In production, you may prefer a generic error message instead of exposing ex.Message.
        };

        return response;
    }
}

}