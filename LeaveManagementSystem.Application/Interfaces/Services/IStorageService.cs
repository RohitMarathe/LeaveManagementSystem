using LeaveManagementSystem.Domain.Application_Models;

namespace LeaveManagementSystem.Application.Interfaces.Services;

public interface IStorageService
{
    Task<BlobUploadResult> UploadFileAsync(
        Stream stream,
        string fileName,
        string contentType,
        long fileSize,
        string containerName,
        CancellationToken cancellationToken);

    Task<Stream> DownloadFileAsync(
        string containerName,
        string blobName,
        CancellationToken cancellationToken);

    Task DeleteFileAsync(
        string containerName,
        string blobName,
        CancellationToken cancellationToken);
}