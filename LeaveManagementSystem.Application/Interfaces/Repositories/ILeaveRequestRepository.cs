
using LeaveManagementSystem.Domain.Entities;

namespace LeaveManagementSystem.Application.Interfaces.Repositories;

public interface ILeaveRequestRepository
    : IGenericRepository<LeaveRequest, int>
{
}
