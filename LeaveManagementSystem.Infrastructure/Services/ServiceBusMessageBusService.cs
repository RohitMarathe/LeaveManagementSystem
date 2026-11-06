using Azure.Messaging.ServiceBus;
using LeaveManagementSystem.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace LeaveManagementSystem.Infrastructure.Services;

public sealed class ServiceBusMessageBusService : IMessageBusService
{
    private readonly ServiceBusClient _serviceBusClient;
    private readonly ILogger<ServiceBusMessageBusService> _logger;

    public ServiceBusMessageBusService(
        ServiceBusClient serviceBusClient,
        ILogger<ServiceBusMessageBusService> logger)
    {
        _serviceBusClient = serviceBusClient;
        _logger = logger;
    }

    public async Task SendMessageAsync<TMessage>(
        string queueOrTopicName,
        TMessage message,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(queueOrTopicName);
        ArgumentNullException.ThrowIfNull(message);

        var json = JsonSerializer.Serialize(message);

        await using var sender =
            _serviceBusClient.CreateSender(queueOrTopicName);

        var serviceBusMessage = new ServiceBusMessage(json)
        {
            ContentType = "application/json",
            Subject = typeof(TMessage).Name
        };

        await sender.SendMessageAsync(
            serviceBusMessage,
            cancellationToken);

        _logger.LogInformation(
            "Message sent successfully to {queueOrTopicName}. MessageType: {MessageType}",
            queueOrTopicName,
            typeof(TMessage).Name);
    }
}
