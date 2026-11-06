using System.Text.Json;
using LeaveManagementSystem.Application.Interfaces.Services;
using LeaveManagementSystem.Infrastructure.Persistence.Context;
using LeaveManagementSystem.Infrastructure.Persistence.Entities;

namespace LeaveManagementSystem.Infrastructure.Persistence;

public sealed class OutboxWriter : IOutboxWriter
{
    private readonly LeaveManagementDbContext _context;

    private static readonly JsonSerializerOptions SerializerOptions =
        new(JsonSerializerDefaults.Web);

    public OutboxWriter(LeaveManagementDbContext context)
    {
        _context = context;
    }

    public void Add(
        Guid eventId,
        string eventType,
        object integrationEvent,
        DateTime occurredAtUtc)
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);

        if (eventId == Guid.Empty)
        {
            throw new ArgumentException(
                "Event ID must not be empty.", nameof(eventId));
        }

        if (string.IsNullOrWhiteSpace(eventType))
        {
            throw new ArgumentException(
                "Event type is required.", nameof(eventType));
        }

        if (occurredAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException(
                "Occurred time must be UTC.", nameof(occurredAtUtc));
        }

        var payload = JsonSerializer.Serialize(
            integrationEvent,
            integrationEvent.GetType(),
            SerializerOptions);

        _context.OutboxMessages.Add(new OutboxMessage
        {
            Id = eventId,
            EventType = eventType,
            Payload = payload,
            OccurredAtUtc = occurredAtUtc,
            CreatedAtUtc = DateTime.UtcNow,
            Attempts = 0
        });
    }
}
