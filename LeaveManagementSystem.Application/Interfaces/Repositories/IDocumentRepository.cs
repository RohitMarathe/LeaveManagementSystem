using LeaveManagementSystem.Domain.Entities;

namespace LeaveManagementSystem.Application.Interfaces.Repositories;

public interface IDocumentRepository
    : IGenericRepository<Document, int>
{
    Task<List<Document>> GetByEmployeeIdAsync(
        int employeeId,
        CancellationToken cancellationToken);
}