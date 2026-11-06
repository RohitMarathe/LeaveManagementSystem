namespace LeaveManagementSystem.Application.Interfaces.Services;

/// <summary>
/// Adds an integration event to the current persistence unit of work.
/// The implementation stores it in the Outbox using the same DbContext
/// used by the application's repositories.
/// </summary>
public interface IOutboxWriter
{
    void Add(
        Guid eventId,
        string eventType,
        object integrationEvent,
        DateTime occurredAtUtc);
}
