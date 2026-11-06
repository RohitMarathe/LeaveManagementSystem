using LeaveManagementSystem.Infrastructure.Persistence.Entities;

namespace LeaveManagementSystem.Application.Interfaces.Repositories
{
    public interface IUnitOfWork
    {
        IEmployeeRepository Employees { get; }

        ILeaveRequestRepository LeaveRequests { get; }

        void AddOutboxMessage(OutboxMessage message);

        Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default);
    }
}
