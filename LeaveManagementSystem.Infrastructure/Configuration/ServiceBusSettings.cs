using System.ComponentModel.DataAnnotations;

namespace LeaveManagementSystem.Infrastructure.Configuration;

public sealed class ServiceBusSettings
{
    public const string SectionName = "ServiceBus";

    [Required]
    public string FullyQualifiedNamespace { get; init; } = string.Empty;

    [Required]
    public string QueueName { get; init; } = string.Empty;
}