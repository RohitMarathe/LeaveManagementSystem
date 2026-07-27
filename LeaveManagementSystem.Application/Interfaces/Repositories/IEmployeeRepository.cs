using LeaveManagementSystem.Domain.Entities;

namespace LeaveManagementSystem.Application.Interfaces.Repositories
{
    public interface IEmployeeRepository
        : IGenericRepository<Employee, int>
    {
        // Employee-specific methods will be added here.
    }
}
