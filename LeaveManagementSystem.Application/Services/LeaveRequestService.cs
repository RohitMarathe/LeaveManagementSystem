using LeaveManagementSystem.Application.Interfaces.Repositories;
using LeaveManagementSystem.Application.Interfaces.Services;
using LeaveManagementSystem.Contracts;
using LeaveManagementSystem.Domain.Common;
using LeaveManagementSystem.Domain.Dtos.LeaveRequest;
using LeaveManagementSystem.Domain.Entities;
using LeaveManagementSystem.Domain.Enums;
using Microsoft.Extensions.Logging;
using System.Net;

namespace LeaveManagementSystem.Application.Services;

public sealed class LeaveRequestService : ILeaveRequestService
{
    private const string QueueName = "leave-requests";
    private const string LeaveRequestSubmittedEventType = "LeaveRequestSubmitted";

    private readonly IUnitOfWork _unitOfWork;
    private readonly IOutboxWriter _outboxWriter;
    private readonly IMessageBusService _messageBus;
    private readonly ILogger<LeaveRequestService> _logger;

    public LeaveRequestService(
        IUnitOfWork unitOfWork,
        IOutboxWriter outboxWriter,
        IMessageBusService messageBus,
        ILogger<LeaveRequestService> logger)
    {
        _unitOfWork = unitOfWork;
        _outboxWriter = outboxWriter;
        _messageBus = messageBus;
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

        var leaveRequest = new LeaveRequest
        {
            EmployeeId = employee.Id,
            LeaveType = request.LeaveType.Trim(),
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Reason = request.Reason.Trim(),
            Status = LeaveRequestStatus.Pending,
            SubmittedAtUtc = submittedAtUtc
        };

        await _unitOfWork.LeaveRequests.AddAsync(
            leaveRequest,
            cancellationToken);

        // Save first to obtain the database-generated LeaveRequest ID.
        // The Outbox event is added and saved in the same DbContext transaction
        // in the follow-up SaveChanges call below only if we instead use a
        // client-generated ID. For an identity ID, this first save would split
        // the atomic operation, so use a transaction around both saves.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var eventId = Guid.NewGuid();
        var correlationId = Guid.NewGuid().ToString("N");

        var submittedEvent = new LeaveRequestSubmittedEvent
        {
            EventId = eventId,
            SchemaVersion = 1,
            CorrelationId = correlationId,
            LeaveRequestId = leaveRequest.Id,
            EmployeeId = employee.Id,
            EmployeeName = $"{employee.FirstName} {employee.LastName}".Trim(),
            EmployeeEmail = employee.Email,
            LeaveType = leaveRequest.LeaveType,
            StartDate = leaveRequest.StartDate,
            EndDate = leaveRequest.EndDate,
            SubmittedAtUtc = submittedAtUtc
        };

        _outboxWriter.Add(
            eventId,
            LeaveRequestSubmittedEventType,
            submittedEvent,
            submittedAtUtc);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Existing Queue workflow intentionally remains unchanged.
        var message = new ProcessLeaveRequestMessage
        {
            MessageVersion = 1,
            LeaveRequestId = leaveRequest.Id
        };

        await _messageBus.SendMessageAsync(
            QueueName,
            message,
            cancellationToken);

        _logger.LogInformation(
            "Leave request submitted. Outbox event {EventId} recorded and existing Queue message published. LeaveRequestId: {LeaveRequestId}, EmployeeId: {EmployeeId}",
            eventId,
            leaveRequest.Id,
            employee.Id);

        var responseDto = new LeaveRequestResponseDto
        {
            Id = leaveRequest.Id,
            EmployeeId = leaveRequest.EmployeeId,
            LeaveType = leaveRequest.LeaveType,
            StartDate = leaveRequest.StartDate,
            EndDate = leaveRequest.EndDate,
            Reason = leaveRequest.Reason,
            Status = leaveRequest.Status.ToString(),
            SubmittedAtUtc = leaveRequest.SubmittedAtUtc
        };

        return ApiResponse<LeaveRequestResponseDto>.Success(
            data: responseDto,
            message: "Leave request submitted successfully.",
            statusCode: HttpStatusCode.Created);
    }
}
