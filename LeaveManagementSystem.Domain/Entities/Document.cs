namespace LeaveManagementSystem.Domain.Entities
{
    public class Document
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }

        public string FileName { get; set; } = default!;

        public string BlobName { get; set; } = default!;

        public string ContainerName { get; set; } = default!;

        public string ContentType { get; set; } = default!;

        public long FileSize { get; set; }

        public DateTime UploadedOn { get; set; }

        public string UploadedBy { get; set; } = default!;

        public Employee Employee { get; set; } = default!;
    }
}
