using System.Text.Json;
using Azure.Messaging.ServiceBus;
using LeaveManagementSystem.Application.Interfaces.Repositories;
using LeaveManagementSystem.Application.Models.Outbox;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace LeaveManagementSystem.Functions.Functions;

public sealed class OutboxPublisherFunction
{
    private const int DefaultBatchSize = 50;
    private const int LockDurationSeconds = 300;
    private static readonly TimeSpan BaseRetryDelay = TimeSpan.FromSeconds(15);
    private static readonly TimeSpan MaximumRetryDelay = TimeSpan.FromMinutes(15);

    private readonly IOutboxPublisherRepository _outboxRepository;
    private readonly ServiceBusSender _sender;
    private readonly ILogger<OutboxPublisherFunction> _logger;

    public OutboxPublisherFunction(
        IOutboxPublisherRepository outboxRepository,
        ServiceBusSender sender,
        ILogger<OutboxPublisherFunction> logger)
    {
        _outboxRepository = outboxRepository;
        _sender = sender;
        _logger = logger;
    }

    [Function(nameof(OutboxPublisherFunction))]
    public async Task Run(
        [TimerTrigger("%OutboxPublisherSchedule%")] TimerInfo timerInfo,
        CancellationToken cancellationToken)
    {
        var lockId = Guid.NewGuid();
        var batchSize = GetBatchSize();

        var messages = await _outboxRepository.ClaimBatchAsync(
            lockId,
            batchSize,
            LockDurationSeconds,
            cancellationToken);

        if (messages.Count == 0)
        {
            _logger.LogDebug("No pending Outbox messages were found.");
            return;
        }

        _logger.LogInformation(
            "Claimed {MessageCount} Outbox messages with lock {LockId}.",
            messages.Count,
            lockId);

        await PublishClaimedMessagesAsync(messages, lockId, cancellationToken);
    }

    private async Task PublishClaimedMessagesAsync(
        IReadOnlyList<OutboxMessageToPublish> messages,
        Guid lockId,
        CancellationToken cancellationToken)
    {
        ServiceBusMessageBatch? messageBatch = null;
        var batchMessages = new List<OutboxMessageToPublish>();

        try
        {
            messageBatch = await _sender.CreateMessageBatchAsync(cancellationToken);

            foreach (var outboxMessage in messages)
            {
                ServiceBusMessage serviceBusMessage;
                try
                {
                    serviceBusMessage = CreateServiceBusMessage(outboxMessage);
                }
                catch (Exception exception) when (
                    exception is JsonException or InvalidOperationException)
                {
                    await ScheduleFailureAsync(
                        outboxMessage,
                        lockId,
                        exception,
                        cancellationToken);
                    continue;
                }

                if (messageBatch!.TryAddMessage(serviceBusMessage))
                {
                    batchMessages.Add(outboxMessage);
                    continue;
                }

                if (batchMessages.Count > 0)
                {
                    await SendBatchAsync(messageBatch, batchMessages, lockId, cancellationToken);
                    messageBatch!.Dispose();
                    messageBatch = await _sender.CreateMessageBatchAsync(cancellationToken);
                    batchMessages = [];
                }

                if (!messageBatch!.TryAddMessage(serviceBusMessage))
                {
                    await ScheduleFailureAsync(
                        outboxMessage,
                        lockId,
                        new InvalidOperationException(
                            "The event is too large to fit in an empty Service Bus message batch."),
                        cancellationToken);
                }
                else
                {
                    batchMessages.Add(outboxMessage);
                }
            }

            if (batchMessages.Count > 0 && messageBatch is not null)
            {
                await SendBatchAsync(messageBatch, batchMessages, lockId, cancellationToken);
            }
        }
        finally
        {
            if (messageBatch is not null)
            {
                messageBatch?.Dispose();
            }
        }
    }

    private async Task SendBatchAsync(
        ServiceBusMessageBatch messageBatch,
        IReadOnlyList<OutboxMessageToPublish> batchMessages,
        Guid lockId,
        CancellationToken cancellationToken)
    {
        try
        {
            await _sender.SendMessagesAsync(messageBatch, cancellationToken);

            foreach (var message in batchMessages)
            {
                await _outboxRepository.MarkPublishedAsync(
                    message.Id,
                    lockId,
                    DateTime.UtcNow,
                    cancellationToken);

                _logger.LogInformation(
                    "Published Outbox event {EventId} of type {EventType}.",
                    message.Id,
                    message.EventType);
            }
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogError(
                exception,
                "Failed to publish a batch containing {MessageCount} Outbox messages.",
                batchMessages.Count);

            foreach (var message in batchMessages)
            {
                await ScheduleFailureAsync(message, lockId, exception, cancellationToken);
            }
        }
    }

    private async Task ScheduleFailureAsync(
        LeaveManagementSystem.Application.Models.Outbox.OutboxMessageToPublish message,
        Guid lockId,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var nextAttempt = message.Attempts + 1;
        var exponentialSeconds = Math.Min(
            BaseRetryDelay.TotalSeconds * Math.Pow(2, Math.Min(nextAttempt - 1, 10)),
            MaximumRetryDelay.TotalSeconds);

        // Small deterministic spread avoids every failed event retrying at the same instant.
        var jitterSeconds = (message.Id.GetHashCode() & 0x7) + 1;
        var nextAttemptAtUtc = DateTime.UtcNow.AddSeconds(
            Math.Min(exponentialSeconds + jitterSeconds, MaximumRetryDelay.TotalSeconds));

        var error = exception.ToString();
        if (error.Length > 2000)
        {
            error = error[..2000];
        }

        await _outboxRepository.ScheduleRetryAsync(
            message.Id,
            lockId,
            nextAttemptAtUtc,
            error,
            cancellationToken);

        _logger.LogWarning(
            exception,
            "Scheduled Outbox event {EventId} for retry at {NextAttemptAtUtc}.",
            message.Id,
            nextAttemptAtUtc);
    }

    private static ServiceBusMessage CreateServiceBusMessage(
        OutboxMessageToPublish outboxMessage)
    {
        using var document = JsonDocument.Parse(outboxMessage.Payload);
        var message = new ServiceBusMessage(BinaryData.FromString(outboxMessage.Payload))
        {
            MessageId = outboxMessage.Id.ToString("D"),
            ContentType = "application/json",
            Subject = outboxMessage.EventType,
            CorrelationId = document.RootElement.TryGetProperty("correlationId", out var correlationId)
                ? correlationId.GetString()
                : null
        };

        message.ApplicationProperties["eventType"] = outboxMessage.EventType;
        message.ApplicationProperties["schemaVersion"] =
            document.RootElement.TryGetProperty("schemaVersion", out var schemaVersion)
                ? schemaVersion.GetInt32()
                : 1;

        return message;
    }

    private static int GetBatchSize()
    {
        var configuredValue = Environment.GetEnvironmentVariable("OutboxPublisherBatchSize");
        return int.TryParse(configuredValue, out var batchSize) && batchSize is > 0 and <= 500
            ? batchSize
            : DefaultBatchSize;
    }
}
