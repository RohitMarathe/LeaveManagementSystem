namespace LeaveManagementSystem.Application.Interfaces.Repositories;

/// <summary>
/// Persists a leave request and its integration event as one database operation.
/// </summary>
public interface ILeaveRequestSubmissionRepository
{
    Task<int> SubmitWithOutboxAsync(
        int employeeId,
        string leaveType,
        DateOnly startDate,
        DateOnly endDate,
        string reason,
        Guid eventId,
        string correlationId,
        DateTime submittedAtUtc,
        CancellationToken cancellationToken = default);
}
