
using LeaveManagementSystem.Domain.Common;
using LeaveManagementSystem.Domain.Dtos.LeaveRequest;

namespace LeaveManagementSystem.Application.Interfaces.Services;

public interface ILeaveRequestService
{
    Task<ApiResponse<LeaveRequestResponseDto>> SubmitAsync(
        CreateLeaveRequestDto request,
        CancellationToken cancellationToken = default);
}
