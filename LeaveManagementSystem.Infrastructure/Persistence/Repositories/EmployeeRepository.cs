using LeaveManagementSystem.Application.Interfaces.Repositories;
using LeaveManagementSystem.Domain.Entities;
using LeaveManagementSystem.Infrastructure.Persistence.Context;

namespace LeaveManagementSystem.Infrastructure.Persistence.Repositories;

public class EmployeeRepository
    : GenericRepository<Employee, int>, IEmployeeRepository
{
    public EmployeeRepository(LeaveManagementDbContext context)
        : base(context)
    {
    }
}