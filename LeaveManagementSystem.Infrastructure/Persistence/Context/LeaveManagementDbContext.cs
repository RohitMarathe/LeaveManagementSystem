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
    }
}
