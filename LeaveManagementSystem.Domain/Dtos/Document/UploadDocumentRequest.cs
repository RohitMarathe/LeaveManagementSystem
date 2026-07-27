namespace LeaveManagementSystem.Domain.Dtos.Document
{
    public sealed class UploadDocumentRequest
    {
        public int EmployeeId { get; init; }

        public Stream FileStream { get; init; } = default!;

        public string FileName { get; init; } = string.Empty;

        public string ContentType { get; init; } = string.Empty;

        public long FileSize { get; init; }
    }
}
