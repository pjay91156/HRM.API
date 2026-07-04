using HRM.API.DTOs;
using HRM.API.Models;
using HRM.API.Repositories;
using HRM.API.Responses;

namespace HRM.API.Services;
public interface ILeaveTypeService
{
    Task<ApiResponse<List<LeaveTypeResponse>>> GetLeaveTypesAsync();

    Task<ApiResponse<string>> AddLeaveTypeAsync(
        LeaveTypeDto request,
        Guid userId);

    Task<ApiResponse<bool>> DeleteLeaveTypeAsync(
        Guid leaveTypeId);
}

public class LeaveTypeService : ILeaveTypeService
{
    private readonly ILeaveTypeRepository _leaveTypeRepository;

    public LeaveTypeService(
        ILeaveTypeRepository leaveTypeRepository)
    {
        _leaveTypeRepository = leaveTypeRepository;
    }

    public async Task<ApiResponse<List<LeaveTypeResponse>>> GetLeaveTypesAsync()
    {
        var leaveTypes =
            await _leaveTypeRepository.GetLeaveTypesAsync();

        return new ApiResponse<List<LeaveTypeResponse>>
        {
            Success = true,
            Message = "Leave types retrieved successfully.",
            Data = leaveTypes
        };
    }

    public async Task<ApiResponse<string>> AddLeaveTypeAsync(
        LeaveTypeDto request,
        Guid userId)
    {
        var leaveType = new LeaveType
        {
            Id = Guid.NewGuid(),
            LeaveName = request.LeaveName,
            LeaveCode = request.LeaveCode,
            Description = request.Description,
            DefaultDays = request.DefaultDays,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        await _leaveTypeRepository.AddLeaveTypeAsync(leaveType);

        await _leaveTypeRepository.SaveChangesAsync();

        return new ApiResponse<string>
        {
            Success = true,
            Message = "Leave type added successfully.",
            Data = leaveType.Id.ToString()
        };
    }

    public async Task<ApiResponse<bool>> DeleteLeaveTypeAsync(
        Guid leaveTypeId)
    {
        var deleted =
            await _leaveTypeRepository.DeleteLeaveTypeAsync(leaveTypeId);

        if (!deleted)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Leave type not found.",
                Data = false
            };
        }

        await _leaveTypeRepository.SaveChangesAsync();

        return new ApiResponse<bool>
        {
            Success = true,
            Message = "Leave type deleted successfully.",
            Data = true
        };
    }
}