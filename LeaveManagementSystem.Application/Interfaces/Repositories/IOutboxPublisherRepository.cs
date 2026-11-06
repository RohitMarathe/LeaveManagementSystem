using LeaveManagementSystem.Application.Models.Outbox;

namespace LeaveManagementSystem.Application.Interfaces.Repositories;

/// <summary>
/// Coordinates safe claiming and state transitions for Outbox messages.
/// </summary>
public interface IOutboxPublisherRepository
{
    Task<IReadOnlyList<OutboxMessageToPublish>> ClaimBatchAsync(
        Guid lockId,
        int batchSize,
        int lockDurationSeconds,
        CancellationToken cancellationToken = default);

    Task MarkPublishedAsync(
        Guid messageId,
        Guid lockId,
        DateTime processedAtUtc,
        CancellationToken cancellationToken = default);

    Task ScheduleRetryAsync(
        Guid messageId,
        Guid lockId,
        DateTime nextAttemptAtUtc,
        string error,
        CancellationToken cancellationToken = default);
}
