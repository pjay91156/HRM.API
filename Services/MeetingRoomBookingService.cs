using HRM.API.Enums;
using HRM.API.Models;
using HRM.API.Repositories;
using HRM.API.Responses;

namespace HRM.API.Services;

public interface IMeetingRoomBookingService
{
    Task<ApiResponse<List<MeetingRoomBookingResponse>>> GetAllAsync(
        Guid userId,
        Guid? floorId,
        Guid? meetingRoomId,
        DateOnly? fromDate,
        DateOnly? toDate,
        MeetingRoomBookingStatus? status);

    Task<ApiResponse<List<MeetingRoomBookingResponse>>> GetMyBookingsAsync(Guid userId);

    Task<ApiResponse<Guid>> CreateAsync(CreateMeetingRoomBookingDto request, Guid userId);

    Task<ApiResponse<bool>> CancelAsync(Guid id, Guid userId, bool isAdmin);
}

public class MeetingRoomBookingService : IMeetingRoomBookingService
{
    private const int MaxReasonLength = 500;

    private readonly IMeetingRoomBookingRepository _bookingRepository;
    private readonly IMeetingRoomRepository _meetingRoomRepository;
    private readonly IEmployeeRepository _employeeRepository;

    public MeetingRoomBookingService(
        IMeetingRoomBookingRepository bookingRepository,
        IMeetingRoomRepository meetingRoomRepository,
        IEmployeeRepository employeeRepository)
    {
        _bookingRepository = bookingRepository;
        _meetingRoomRepository = meetingRoomRepository;
        _employeeRepository = employeeRepository;
    }

    private static MeetingRoomBookingResponse ToResponse(MeetingRoomBooking booking, Guid requesterEmployeeId, bool isAdmin)
    {
        var isUpcoming = booking.BookingDate > DateOnly.FromDateTime(DateTime.UtcNow) ||
            (booking.BookingDate == DateOnly.FromDateTime(DateTime.UtcNow) && booking.EndTime > TimeOnly.FromDateTime(DateTime.UtcNow));

        var isOwner = booking.EmployeeId == requesterEmployeeId;

        return new MeetingRoomBookingResponse
        {
            Id = booking.Id,
            MeetingRoomId = booking.MeetingRoomId,
            MeetingRoomName = booking.MeetingRoom?.Name ?? string.Empty,
            FloorId = booking.MeetingRoom?.FloorId ?? Guid.Empty,
            FloorName = booking.MeetingRoom?.Floor?.Name ?? string.Empty,
            EmployeeId = booking.EmployeeId,
            EmployeeName = booking.Employee?.User == null
                ? string.Empty
                : $"{booking.Employee.User.FirstName} {booking.Employee.User.LastName}",
            BookingDate = booking.BookingDate,
            StartTime = booking.StartTime,
            EndTime = booking.EndTime,
            Reason = booking.Reason,
            NumberOfAttendees = booking.NumberOfAttendees,
            Status = booking.Status.ToString(),
            CanCancel = booking.Status == MeetingRoomBookingStatus.Confirmed && isUpcoming && (isOwner || isAdmin)
        };
    }

    public async Task<ApiResponse<List<MeetingRoomBookingResponse>>> GetAllAsync(
        Guid userId,
        Guid? floorId,
        Guid? meetingRoomId,
        DateOnly? fromDate,
        DateOnly? toDate,
        MeetingRoomBookingStatus? status)
    {
        try
        {
            var requester = await _employeeRepository.GetByUserIdAsync(userId);

            if (requester == null)
            {
                return new ApiResponse<List<MeetingRoomBookingResponse>>
                {
                    Success = false,
                    Message = "No employee profile found for the current user."
                };
            }

            var bookings = await _bookingRepository.GetAllAsync(floorId, meetingRoomId, null, fromDate, toDate, status);

            return new ApiResponse<List<MeetingRoomBookingResponse>>
            {
                Success = true,
                Message = "Bookings retrieved successfully.",
                Data = bookings.Select(b => ToResponse(b, requester.Id, false)).ToList()
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<List<MeetingRoomBookingResponse>>
            {
                Success = false,
                Message = "An error occurred while retrieving bookings.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<List<MeetingRoomBookingResponse>>> GetMyBookingsAsync(Guid userId)
    {
        try
        {
            var requester = await _employeeRepository.GetByUserIdAsync(userId);

            if (requester == null)
            {
                return new ApiResponse<List<MeetingRoomBookingResponse>>
                {
                    Success = false,
                    Message = "No employee profile found for the current user."
                };
            }

            var bookings = await _bookingRepository.GetAllAsync(null, null, requester.Id, null, null, null);

            return new ApiResponse<List<MeetingRoomBookingResponse>>
            {
                Success = true,
                Message = "Your bookings retrieved successfully.",
                Data = bookings.Select(b => ToResponse(b, requester.Id, false)).ToList()
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<List<MeetingRoomBookingResponse>>
            {
                Success = false,
                Message = "An error occurred while retrieving your bookings.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<Guid>> CreateAsync(CreateMeetingRoomBookingDto request, Guid userId)
    {
        try
        {
            var requester = await _employeeRepository.GetByUserIdAsync(userId);

            if (requester == null)
            {
                return new ApiResponse<Guid>
                {
                    Success = false,
                    Message = "No employee profile found for the current user."
                };
            }

            var reasonValidation = ValidateReason(request.Reason, out var trimmedReason);

            if (reasonValidation != null)
            {
                return new ApiResponse<Guid> { Success = false, Message = reasonValidation };
            }

            if (request.StartTime >= request.EndTime)
            {
                return new ApiResponse<Guid> { Success = false, Message = "Start time must be earlier than end time." };
            }

            if (request.NumberOfAttendees.HasValue && request.NumberOfAttendees.Value <= 0)
            {
                return new ApiResponse<Guid> { Success = false, Message = "Number of attendees must be greater than 0." };
            }

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            if (request.BookingDate < today)
            {
                return new ApiResponse<Guid> { Success = false, Message = "Booking date cannot be in the past." };
            }

            if (request.BookingDate == today && request.StartTime < TimeOnly.FromDateTime(DateTime.UtcNow))
            {
                return new ApiResponse<Guid> { Success = false, Message = "Start time cannot be in the past." };
            }

            var meetingRoom = await _meetingRoomRepository.GetByIdAsync(request.MeetingRoomId);

            if (meetingRoom == null)
            {
                return new ApiResponse<Guid> { Success = false, Message = "Meeting room not found." };
            }

            if (request.NumberOfAttendees.HasValue && request.NumberOfAttendees.Value > meetingRoom.Capacity)
            {
                return new ApiResponse<Guid>
                {
                    Success = false,
                    Message = $"Number of attendees exceeds the room capacity of {meetingRoom.Capacity}."
                };
            }

            var hasOverlap = await _bookingRepository.HasOverlappingBookingAsync(
                request.MeetingRoomId, request.BookingDate, request.StartTime, request.EndTime);

            if (hasOverlap)
            {
                return new ApiResponse<Guid>
                {
                    Success = false,
                    Message = "This room is already booked for the selected time slot."
                };
            }

            var booking = new MeetingRoomBooking
            {
                Id = Guid.NewGuid(),
                MeetingRoomId = request.MeetingRoomId,
                EmployeeId = requester.Id,
                BookingDate = request.BookingDate,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                Reason = trimmedReason,
                NumberOfAttendees = request.NumberOfAttendees,
                Status = MeetingRoomBookingStatus.Confirmed,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            await _bookingRepository.AddAsync(booking);

            await _bookingRepository.SaveChangesAsync();

            return new ApiResponse<Guid>
            {
                Success = true,
                Message = "Meeting room booked successfully.",
                Data = booking.Id
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<Guid>
            {
                Success = false,
                Message = "An error occurred while creating the booking.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<ApiResponse<bool>> CancelAsync(Guid id, Guid userId, bool isAdmin)
    {
        try
        {
            var requester = await _employeeRepository.GetByUserIdAsync(userId);

            if (requester == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "No employee profile found for the current user."
                };
            }

            var booking = await _bookingRepository.GetByIdAsync(id);

            if (booking == null)
            {
                return new ApiResponse<bool> { Success = false, Message = "Booking not found." };
            }

            if (booking.EmployeeId != requester.Id && !isAdmin)
            {
                return new ApiResponse<bool> { Success = false, Message = "You are not authorized to cancel this booking." };
            }

            if (booking.Status != MeetingRoomBookingStatus.Confirmed)
            {
                return new ApiResponse<bool> { Success = false, Message = "Only confirmed bookings can be cancelled." };
            }

            booking.Status = MeetingRoomBookingStatus.Cancelled;
            booking.UpdatedAt = DateTime.UtcNow;
            booking.UpdatedBy = userId;

            _bookingRepository.Update(booking);

            await _bookingRepository.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Message = "Booking cancelled successfully.",
                Data = true
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "An error occurred while cancelling the booking.",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    private static string? ValidateReason(string? reason, out string trimmedReason)
    {
        trimmedReason = reason?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(trimmedReason))
        {
            return "Reason is required and cannot contain only spaces.";
        }

        if (trimmedReason.Length > MaxReasonLength)
        {
            return $"Reason cannot exceed {MaxReasonLength} characters.";
        }

        return null;
    }
}
