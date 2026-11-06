using LeaveManagementSystem.Application.Interfaces.Services;
using LeaveManagementSystem.Application.Services;
using LeaveManagementSystem.Domain.AutoMapperProfile;
using Microsoft.Extensions.DependencyInjection;

namespace LeaveManagementSystem.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddScoped<IEmployeeService, EmployeeService>();

        services.AddScoped<ILeaveRequestService, LeaveRequestService>();

        services.AddAutoMapper(cfg => { }, typeof(EmployeeMappingProfile).Assembly);

        return services;
    }
}