using LeaveManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagementSystem.Infrastructure.Persistence.Context
{
    public class LeaveManagementDbContext: DbContext
    {
        public LeaveManagementDbContext(
            DbContextOptions<LeaveManagementDbContext> options)
            : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }

        public DbSet<Document> Documents { get; set; }

        public DbSet<LeaveRequest> LeaveRequests { get; set; }

        public DbSet<LeaveManagementSystem.Infrastructure.Persistence.Entities.OutboxMessage> OutboxMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(LeaveManagementDbContext).Assembly);
        }
    }
}
