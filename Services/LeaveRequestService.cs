using HRM.API.DTOs;
using HRM.API.Models;
using HRM.API;
using HRM.API.Services;
using HRM.API.Responses;
using HRM.API.Enums;
using HRM.API.Repositories;

namespace HRM.API.Services;

public interface ILeaveRequestService
{
    Task<ApiResponse<string>> ApplyLeaveAsync(
        ApplyLeaveRequestDto request, Guid userId);

    Task<ApiResponse<List<LeaveRequestResponse>>> GetLeavesAsync();

    Task<ApiResponse<List<LeaveRequestResponse>>> GetEmployeeLeavesAsync(
        Guid employeeId);


    Task<ApiResponse<string>> ApproveRejectLeaveAsync(
        Guid approverId,
        LeaveApprovalRequestDto request);

    Task<ApiResponse<string>> CancelLeaveAsync(Guid leaveRequestId, Guid UpdatedBy);
    Task<ApiResponse<List<TeamLeaveRequestResponse>>> GetTeamLeaveRequestsAsync(Guid userId);
    Task<
    ApiResponse<List<TeamLeaveCalendarResponse>>>
    GetTeamLeaveCalendarAsync(Guid managerId);
}


public class LeaveRequestService : ILeaveRequestService
{
    private readonly ILeaveRequestRepository _leaveRequestRepository;
    private readonly IEmployeeRepository _employeeRepository;

    public LeaveRequestService(
        ILeaveRequestRepository leaveRequestRepository,
        IEmployeeRepository employeeRepository)
    {
        _leaveRequestRepository = leaveRequestRepository;
        _employeeRepository = employeeRepository;
    }

    public async Task<ApiResponse<string>> ApplyLeaveAsync(
        ApplyLeaveRequestDto request, Guid userId)
    {
        try
        {
            if (request.FromDate.Date > request.ToDate.Date)
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "From date cannot be greater than To date."
                };
            }

            if (request.LeaveDuration == LeaveDuration.HalfDay)
            {
                if (request.FromDate.Date != request.ToDate.Date)
                {
                    return new ApiResponse<string>
                    {
                        Success = false,
                        Message = "For half day leave, start date and end date must be same."
                    };
                }


            }
            var employee = await _employeeRepository.GetByUserIdAsync(userId);
            if (employee == null)
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "Employee is not found."
                };
            }
            decimal totalDays =
                request.LeaveDuration == LeaveDuration.HalfDay
                ? 0.5m
                : (request.ToDate.Date - request.FromDate.Date).Days + 1;

            var leaveRequest = new LeaveRequest
            {
                Id = Guid.NewGuid(),
                EmployeeId = employee.Id,
                LeaveTypeId = request.LeaveTypeId,
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                LeaveDuration = request.LeaveDuration,
                TotalDays = totalDays,
                Reason = request.Reason,
                Status = LeaveStatus.Pending,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            await _leaveRequestRepository.AddAsync(leaveRequest);

            return new ApiResponse<string>
            {
                Success = true,
                Message = "Leave applied successfully."
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<string>
            {
                Success = false,
                Message = "Error while applying leave.",
                Errors = new List<string> { ex.Message }
            };
        }
    }
    public async Task<ApiResponse<List<TeamLeaveRequestResponse>>> GetTeamLeaveRequestsAsync(Guid userId)
    {
        try
        {
            var employee = await _employeeRepository.GetByUserIdAsync(userId);
            var leaveRequests =
                await _leaveRequestRepository.GetTeamLeaveRequestsAsync(employee.Id);

            return new ApiResponse<List<TeamLeaveRequestResponse>>
            {
                Success = true,
                Message = "Team leave requests retrieved successfully.",
                Data = leaveRequests
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<List<TeamLeaveRequestResponse>>
            {
                Success = false,
                Message = "Failed to retrieve team leave requests.",
                Errors = new List<string>
            {
                ex.Message
            }
            };
        }
    }

    public async Task<ApiResponse<List<LeaveRequestResponse>>> GetLeavesAsync()
    {
        try
        {
            var leaveRequests =
                await _leaveRequestRepository.GetAllAsync();

            var response = leaveRequests
                .Select(x => new LeaveRequestResponse
                {
                    Id = x.Id,
                    EmployeeId = x.EmployeeId,
                    LeaveType = x.LeaveType,
                    FromDate = x.FromDate,
                    ToDate = x.ToDate,
                    TotalDays = x.TotalDays,
                    LeaveDuration = x.LeaveDuration,
                    Reason = x.Reason,
                    Status = x.Status,
                    ApproverComments = x.ApproverComments
                })
                .ToList();

            return new ApiResponse<List<LeaveRequestResponse>>
            {
                Success = true,
                Message = "Leave requests retrieved successfully.",
                Data = response
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<List<LeaveRequestResponse>>
            {
                Success = false,
                Message = "Error while retrieving leave requests.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<List<LeaveRequestResponse>>> GetEmployeeLeavesAsync(
        Guid userId)
    {
        try
        {
            var employee = await _employeeRepository.GetByUserIdAsync(userId);
            var leaveRequests =
                await _leaveRequestRepository.GetByEmployeeIdAsync(employee.Id);

            var response = leaveRequests
                .Select(x => new LeaveRequestResponse
                {
                    Id = x.Id,
                    EmployeeId = x.EmployeeId,
                    LeaveType = x.LeaveType,
                    FromDate = x.FromDate,
                    ToDate = x.ToDate,
                    TotalDays = x.TotalDays,
                    LeaveDuration = x.LeaveDuration,
                    Reason = x.Reason,
                    Status = x.Status,
                    ApproverComments = x.ApproverComments
                })
                .ToList();

            return new ApiResponse<List<LeaveRequestResponse>>
            {
                Success = true,
                Message = "Employee leave requests retrieved successfully.",
                Data = response
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<List<LeaveRequestResponse>>
            {
                Success = false,
                Message = "Error while retrieving employee leave requests.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<string>> ApproveRejectLeaveAsync(
        Guid approverId,
        LeaveApprovalRequestDto request)
    {
        try
        {
            var leaveRequest =
                await _leaveRequestRepository.GetByIdAsync(
                    request.LeaveRequestId);

            if (leaveRequest == null)
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "Leave request not found."
                };
            }

            if (leaveRequest.Status != LeaveStatus.Pending)
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "Only pending leave requests can be approved or rejected."
                };
            }

            leaveRequest.Status = request.Status;
            leaveRequest.ApprovedBy = approverId;
            leaveRequest.ApprovedAt = DateTime.UtcNow;
            leaveRequest.ApproverComments = request.Comments;
            leaveRequest.UpdatedAt = DateTime.UtcNow;
            leaveRequest.UpdatedBy = approverId;

            await _leaveRequestRepository.UpdateAsync(leaveRequest);

            return new ApiResponse<string>
            {
                Success = true,
                Message = $"Leave {request.Status} successfully."
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<string>
            {
                Success = false,
                Message = "Error while updating leave request.",
                Errors = new List<string> { ex.Message }
            };
        }
    }
    public async Task<
    ApiResponse<List<TeamLeaveCalendarResponse>>>
    GetTeamLeaveCalendarAsync(Guid managerId)
    {
        try
        {
            var employee= await _employeeRepository.GetByUserIdAsync(managerId);
            var leaves =
                await _leaveRequestRepository
                    .GetTeamLeaveCalendarAsync(employee.Id);

            return new ApiResponse<
                List<TeamLeaveCalendarResponse>>
            {
                Success = true,
                Message = "Team leave calendar retrieved successfully.",
                Data = leaves
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<
                List<TeamLeaveCalendarResponse>>
            {
                Success = false,
                Message = "Failed to retrieve team leave calendar.",
                Errors = new List<string>
            {
                ex.Message
            }
            };
        }
    }

    public async Task<ApiResponse<string>> CancelLeaveAsync(
        Guid leaveRequestId,
        Guid updatedBy)
    {
        try
        {
            var leaveRequest =
                await _leaveRequestRepository.GetByIdAsync(
                    leaveRequestId);

            if (leaveRequest == null)
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "Leave request not found."
                };
            }

            if (leaveRequest.Status != LeaveStatus.Pending)
            {
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "Only pending leave requests can be cancelled."
                };
            }

            leaveRequest.Status = LeaveStatus.Cancelled;
            leaveRequest.IsActive = false;
            leaveRequest.UpdatedAt = DateTime.UtcNow;
            leaveRequest.UpdatedBy = updatedBy;

            await _leaveRequestRepository.UpdateAsync(leaveRequest);

            return new ApiResponse<string>
            {
                Success = true,
                Message = "Leave cancelled successfully."
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<string>
            {
                Success = false,
                Message = "Error while cancelling leave.",
                Errors = new List<string> { ex.Message }
            };
        }
    }
}