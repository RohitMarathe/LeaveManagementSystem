using LeaveManagementSystem.Application.Interfaces.Repositories;
using LeaveManagementSystem.Infrastructure.Persistence.Context;
using LeaveManagementSystem.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LeaveManagementSystem.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<LeaveManagementDbContext>(options =>
            {
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"));
            });

            services.AddScoped(
                typeof(IGenericRepository<,>),
                typeof(GenericRepository<,>));

            services.AddScoped<IEmployeeRepository, EmployeeRepository>();

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}
