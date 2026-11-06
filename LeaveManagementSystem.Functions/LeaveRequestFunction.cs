using Azure.Messaging.ServiceBus;
using Azure.Core;
using LeaveManagementSystem.Contracts;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace LeaveManagementSystem.Functions;

public class LeaveRequestFunction
{
    private readonly ILogger<LeaveRequestFunction> _logger;

    public LeaveRequestFunction(
        ILogger<LeaveRequestFunction> logger)
    {
        _logger = logger;
    }

    [Function("ProcessLeaveRequest")]
    public void Run(
        [ServiceBusTrigger(
            "leave-requests",
            Connection = "ServiceBusConnection")]
        ServiceBusReceivedMessage receivedMessage)
    {
        ProcessLeaveRequestMessage? request =
            receivedMessage.Body
                .ToObjectFromJson<ProcessLeaveRequestMessage>();

        if (request is null ||
            request.MessageVersion != 1 ||
            request.LeaveRequestId <= 0)
        {
            throw new InvalidOperationException(
                "Invalid leave request message.");
        }

        _logger.LogInformation(
            "Message ID: {MessageId}",
            receivedMessage.MessageId);

        _logger.LogInformation(
            "Processing leave request {LeaveRequestId}",
            request.LeaveRequestId);
    }
}