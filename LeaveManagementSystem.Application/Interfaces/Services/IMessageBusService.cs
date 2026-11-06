namespace LeaveManagementSystem.Application.Interfaces.Services;

public interface IMessageBusService
{
    Task SendMessageAsync<TMessage>(
        string destination,
        TMessage message,
        CancellationToken cancellationToken = default);
}
