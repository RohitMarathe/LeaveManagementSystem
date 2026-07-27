namespace LeaveManagementSystem.Domain.Models.Document
{
    public sealed class DocumentResponse
    {
        public int Id { get; init; }

        public string FileName { get; init; } = string.Empty;

        public string BlobUrl { get; init; } = string.Empty;

        public string ContentType { get; init; } = string.Empty;

        public long FileSize { get; init; }

        public DateTime UploadedOn { get; init; }
    }
}
