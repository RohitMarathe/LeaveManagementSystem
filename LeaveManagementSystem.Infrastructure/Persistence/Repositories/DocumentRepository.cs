using LeaveManagementSystem.Application.Interfaces.Repositories;
using LeaveManagementSystem.Domain.Entities;
using LeaveManagementSystem.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagementSystem.Infrastructure.Persistence.Repositories;

public sealed class DocumentRepository
    : GenericRepository<Document, int>, IDocumentRepository
{
    private readonly LeaveManagementDbContext _context;

    public DocumentRepository(
        LeaveManagementDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<List<Document>> GetByEmployeeIdAsync(
        int employeeId,
        CancellationToken cancellationToken)
    {
        return await _context.Documents
            .AsNoTracking()
            .Where(x => x.EmployeeId == employeeId)
            .OrderByDescending(x => x.UploadedOn)
            .ToListAsync(cancellationToken);
    }
}