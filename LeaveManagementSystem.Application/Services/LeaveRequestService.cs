using LeaveManagementSystem.Application.Interfaces.Repositories;
using LeaveManagementSystem.Domain.Common;
using LeaveManagementSystem.Domain.Dtos.LeaveRequest;
using LeaveManagementSystem.Domain.Enums;
using Microsoft.Extensions.Logging;
using System.Net;

namespace LeaveManagementSystem.Application.Services;

public sealed class LeaveRequestService : ILeaveRequestService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILeaveRequestSubmissionRepository _submissionRepository;
    private readonly ILogger<LeaveRequestService> _logger;

    public LeaveRequestService(
        IUnitOfWork unitOfWork,
        ILeaveRequestSubmissionRepository submissionRepository,
        ILogger<LeaveRequestService> logger)
    {
        _unitOfWork = unitOfWork;
        _submissionRepository = submissionRepository;
        _logger = logger;
    }

    public async Task<ApiResponse<LeaveRequestResponseDto>> SubmitAsync(
        CreateLeaveRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (request.StartDate > request.EndDate)
        {
            return ApiResponse<LeaveRequestResponseDto>.Failure(
                message: "The leave start date cannot be after the end date.",
                statusCode: HttpStatusCode.BadRequest);
        }

        if (string.IsNullOrWhiteSpace(request.LeaveType) ||
            string.IsNullOrWhiteSpace(request.Reason))
        {
            return ApiResponse<LeaveRequestResponseDto>.Failure(
                message: "Leave type and reason are required.",
                statusCode: HttpStatusCode.BadRequest);
        }

        var employee = await _unitOfWork.Employees.GetByIdAsync(
            request.EmployeeId,
            cancellationToken);

        if (employee is null)
        {
            return ApiResponse<LeaveRequestResponseDto>.Failure(
                message: $"Employee with ID {request.EmployeeId} was not found.",
                statusCode: HttpStatusCode.NotFound);
        }

        if (!employee.IsActive)
        {
            return ApiResponse<LeaveRequestResponseDto>.Failure(
                message: "Inactive employees cannot submit leave requests.",
                statusCode: HttpStatusCode.BadRequest);
        }

        if (employee.ManagerId is null)
        {
            return ApiResponse<LeaveRequestResponseDto>.Failure(
                message: "A manager must be assigned to the employee before submitting a leave request.",
                statusCode: HttpStatusCode.BadRequest);
        }

        var submittedAtUtc = DateTime.UtcNow;
        var eventId = Guid.NewGuid();
        var correlationId = Guid.NewGuid().ToString("N");
        var leaveType = request.LeaveType.Trim();
        var reason = request.Reason.Trim();

        // The stored procedure owns the transaction. It either inserts both
        // the LeaveRequest and Outbox event, or inserts neither.
        var leaveRequestId = await _submissionRepository.SubmitWithOutboxAsync(
            employee.Id,
            leaveType,
            request.StartDate,
            request.EndDate,
            reason,
            eventId,
            correlationId,
            submittedAtUtc,
            cancellationToken);

        _logger.LogInformation(
            "Leave request and Outbox event committed. LeaveRequestId: {LeaveRequestId}, EventId: {EventId}, EmployeeId: {EmployeeId}",
            leaveRequestId,
            eventId,
            employee.Id);

        var responseDto = new LeaveRequestResponseDto
        {
            Id = leaveRequestId,
            EmployeeId = employee.Id,
            LeaveType = leaveType,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Reason = reason,
            Status = LeaveRequestStatus.Pending.ToString(),
            SubmittedAtUtc = submittedAtUtc
        };

        return ApiResponse<LeaveRequestResponseDto>.Success(
            data: responseDto,
            message: "Leave request submitted successfully.",
            statusCode: HttpStatusCode.Created);
    }
}
