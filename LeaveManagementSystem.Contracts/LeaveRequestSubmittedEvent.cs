namespace LeaveManagementSystem.Contracts;

/// <summary>
/// Represents the fact that an employee has submitted a leave request.
/// This event is published to the leave-events Service Bus topic.
/// </summary>
public sealed class LeaveRequestSubmittedEvent
{
    /// <summary>
    /// Stable identifier for this event across retries and redelivery.
    /// </summary>
    public Guid EventId { get; init; }

    /// <summary>
    /// Version of this event contract.
    /// Increment when introducing a breaking contract change.
    /// </summary>
    public int SchemaVersion { get; init; } = 1;

    /// <summary>
    /// Identifier used to trace this operation across the API and consumers.
    /// </summary>
    public string? CorrelationId { get; init; }

    public int LeaveRequestId { get; init; }

    public int EmployeeId { get; init; }

    public required string EmployeeName { get; init; }

    public required string EmployeeEmail { get; init; }

    public required string LeaveType { get; init; }

    public DateOnly StartDate { get; init; }

    public DateOnly EndDate { get; init; }

    public DateTime SubmittedAtUtc { get; init; }
}
