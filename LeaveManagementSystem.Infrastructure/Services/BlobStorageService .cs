using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using LeaveManagementSystem.Application.Interfaces.Services;
using LeaveManagementSystem.Domain.Application_Models;
using LeaveManagementSystem.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LeaveManagementSystem.Infrastructure.Services;

public sealed class BlobStorageService : IStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly BlobStorageSettings _settings;
    private readonly ILogger<BlobStorageService> _logger;

    public BlobStorageService(
        BlobServiceClient blobServiceClient,
        IOptions<BlobStorageSettings> options,
        ILogger<BlobStorageService> logger)
    {
        _blobServiceClient = blobServiceClient;
        _settings = options.Value;
        _logger = logger;
    }

    public async Task<BlobUploadResult> UploadFileAsync(
        Stream stream,
        string fileName,
        string contentType,
        long fileSize,
        string containerName,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Starting blob upload for file {FileName}",
            fileName);

        var containerClient =
            _blobServiceClient.GetBlobContainerClient(containerName);

        var blobName =
            $"{Guid.NewGuid():N}_{fileName}";

        var blobClient =
            containerClient.GetBlobClient(blobName);

        var uploadOptions = new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders
            {
                ContentType = contentType
            }
        };

        await blobClient.UploadAsync(
            stream,
            uploadOptions,
            cancellationToken);

        _logger.LogInformation(
            "Blob uploaded successfully. Blob Name : {BlobName}",
            blobName);

        return new BlobUploadResult
        {
            BlobName = blobName,

            BlobUrl = blobClient.Uri.AbsoluteUri,

            ContentType = contentType,

            FileSize = fileSize
        };
    }

    public async Task<Stream> DownloadFileAsync(
        string containerName,
        string blobName,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Downloading blob {BlobName}",
            blobName);

        var containerClient =
            _blobServiceClient.GetBlobContainerClient(containerName);

        var blobClient =
            containerClient.GetBlobClient(blobName);

        if (!await blobClient.ExistsAsync(cancellationToken))
        {
            throw new FileNotFoundException(
                $"Blob '{blobName}' does not exist.");
        }

        var response =
            await blobClient.DownloadStreamingAsync(
                cancellationToken: cancellationToken);

        return response.Value.Content;
    }

    public async Task DeleteFileAsync(
        string containerName,
        string blobName,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Deleting blob {BlobName}",
            blobName);

        var containerClient =
            _blobServiceClient.GetBlobContainerClient(containerName);

        var blobClient =
            containerClient.GetBlobClient(blobName);

        await blobClient.DeleteIfExistsAsync(
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Blob deleted successfully. BlobName : {BlobName}",
            blobName);
    }
}