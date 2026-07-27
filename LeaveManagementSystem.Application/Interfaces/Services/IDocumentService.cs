using LeaveManagementSystem.Domain.Dtos.Document;
using LeaveManagementSystem.Domain.Models.Document;

namespace LeaveManagementSystem.Application.Interfaces.Services;

public interface IDocumentService
{
    Task<DocumentResponse> UploadAsync(
        UploadDocumentRequest request,
        CancellationToken cancellationToken);

    Task<Stream> DownloadAsync(
        int documentId,
        CancellationToken cancellationToken);

    Task DeleteAsync(
        int documentId,
        CancellationToken cancellationToken);
}