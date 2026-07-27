using System.ComponentModel.DataAnnotations;

namespace LeaveManagementSystem.Infrastructure.Configuration
{
    public sealed class BlobStorageSettings
    {
        public const string SectionName = "Storage";
        [Required]
        public string AccountName { get; init; } = string.Empty;
        [Required]
        public string ContainerName { get; init; } = string.Empty;
    }
}
