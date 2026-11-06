using LeaveManagementSystem.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeaveManagementSystem.Infrastructure.Persistence.Configurations;

public sealed class OutboxMessageConfiguration
    : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");

        builder.HasKey(message => message.Id);

        builder.Property(message => message.Id)
            .ValueGeneratedNever();

        builder.Property(message => message.EventType)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(message => message.Payload)
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder.Property(message => message.OccurredAtUtc)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(message => message.CreatedAtUtc)
            .HasColumnType("datetime2")
            .HasDefaultValueSql("SYSUTCDATETIME()")
            .IsRequired();

        builder.Property(message => message.ProcessedAtUtc)
            .HasColumnType("datetime2");

        builder.Property(message => message.NextAttemptAtUtc)
            .HasColumnType("datetime2");

        builder.Property(message => message.LastError)
            .HasMaxLength(2000);

        builder.Property(message => message.Attempts)
            .HasDefaultValue(0)
            .IsRequired();

        builder.HasIndex(message => new
        {
            message.ProcessedAtUtc,
            message.NextAttemptAtUtc,
            message.CreatedAtUtc
        })
        .HasDatabaseName("IX_OutboxMessages_PublishingStatus");
    }
}
