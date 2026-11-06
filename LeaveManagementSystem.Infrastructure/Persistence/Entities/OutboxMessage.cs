namespace LeaveManagementSystem.Infrastructure.Persistence.Entities;

/// <summary>
/// Durable record of an integration event waiting to be published.
/// This is infrastructure state, not a business-domain entity.
/// </summary>
public sealed class OutboxMessage
{
    public Guid Id { get; set; }

    public required string EventType { get; set; }

    public required string Payload { get; set; }

    public DateTime OccurredAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? ProcessedAtUtc { get; set; }

    public int Attempts { get; set; }

    public DateTime? NextAttemptAtUtc { get; set; }

    public string? LastError { get; set; }

    public Guid? LockId { get; set; }

    public DateTime? LockedUntilUtc { get; set; }

    public DateTime? FailedAtUtc { get; set; }
}
