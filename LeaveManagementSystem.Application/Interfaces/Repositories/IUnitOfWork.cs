namespace LeaveManagementSystem.Application.Interfaces.Repositories
{
    public interface IUnitOfWork
    {
        IEmployeeRepository Employees { get; }

        Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default);
    }
}
