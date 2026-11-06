using LeaveManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeaveManagementSystem.Infrastructure.Persistence.Configurations;

public sealed class EmployeeConfiguration
    : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.HasOne(x => x.Manager)
            .WithMany(x => x.DirectReports)
            .HasForeignKey(x => x.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}