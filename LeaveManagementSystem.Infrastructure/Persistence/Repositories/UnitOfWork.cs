using LeaveManagementSystem.Application.Interfaces.Repositories;
using LeaveManagementSystem.Infrastructure.Persistence.Context;
using LeaveManagementSystem.Infrastructure.Persistence.Entities;

namespace LeaveManagementSystem.Infrastructure.Persistence.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly LeaveManagementDbContext _context;

    private IEmployeeRepository? _employeeRepository;

    private ILeaveRequestRepository? _leaveRequestRepository;

    public UnitOfWork(LeaveManagementDbContext context)
    {
        _context = context;
    }

    public IEmployeeRepository Employees
    {
        get
        {
            _employeeRepository ??= new EmployeeRepository(_context);
            return _employeeRepository;
        }
    }

    public ILeaveRequestRepository LeaveRequests
    {
        get
        {
            _leaveRequestRepository ??= new LeaveRequestRepository(_context);
            return _leaveRequestRepository;
        }
    }

    public void AddOutboxMessage(OutboxMessage message)
    {
        _context.OutboxMessages.Add(message);
    }

    public async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}