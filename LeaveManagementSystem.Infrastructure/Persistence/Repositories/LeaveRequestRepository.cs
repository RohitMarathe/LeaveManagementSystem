
using LeaveManagementSystem.Application.Interfaces.Repositories;
using LeaveManagementSystem.Domain.Entities;
using LeaveManagementSystem.Infrastructure.Persistence.Context;

namespace LeaveManagementSystem.Infrastructure.Persistence.Repositories;

public sealed class LeaveRequestRepository
    : GenericRepository<LeaveRequest, int>, ILeaveRequestRepository
{
    public LeaveRequestRepository(LeaveManagementDbContext context)
        : base(context)
    {
    }
}
