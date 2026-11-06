namespace LeaveManagementSystem.Application.Models.Outbox;

/// <summary>
/// A claimed Outbox event returned to the publisher.
/// </summary>
public sealed record OutboxMessageToPublish(
    Guid Id,
    string EventType,
    string Payload,
    DateTime OccurredAtUtc,
    int Attempts);
