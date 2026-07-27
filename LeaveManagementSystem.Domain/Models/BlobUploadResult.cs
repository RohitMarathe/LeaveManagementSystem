namespace LeaveManagementSystem.Domain.Application_Models
{
    public sealed class BlobUploadResult
    {
        public string BlobName { get; init; } = string.Empty;

        public string BlobUrl { get; init; } = string.Empty;

        public string ContentType { get; init; } = string.Empty;

        public long FileSize { get; init; }
    }
}
