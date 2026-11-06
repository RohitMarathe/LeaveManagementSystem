namespace LeaveManagementSystem.Application.Interfaces.Repositories
{
    public interface IUnitOfWork
    {
        IEmployeeRepository Employees { get; }

        ILeaveRequestRepository LeaveRequests { get; }

        Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default);
    }
}
