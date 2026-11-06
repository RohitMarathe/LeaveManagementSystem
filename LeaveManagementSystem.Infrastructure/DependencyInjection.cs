using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using LeaveManagementSystem.Application.Interfaces.Repositories;
using LeaveManagementSystem.Application.Interfaces.Services;
using LeaveManagementSystem.Application.Services;
using LeaveManagementSystem.Infrastructure.Configuration;
using LeaveManagementSystem.Infrastructure.Persistence.Context;
using LeaveManagementSystem.Infrastructure.Persistence.Repositories;
using LeaveManagementSystem.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LeaveManagementSystem.Infrastructure;

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
        services.AddScoped<
            ILeaveRequestSubmissionRepository,
            LeaveRequestSubmissionRepository>();

        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();

        services.AddOptions<BlobStorageSettings>()
            .Bind(configuration.GetSection(BlobStorageSettings.SectionName))
            .ValidateOnStart();

        services.AddSingleton<BlobServiceClient>(sp =>
        {
            var settings = sp
                .GetRequiredService<
                    Microsoft.Extensions.Options.IOptions<BlobStorageSettings>>()
                .Value;

            var serviceUri = new Uri(
                $"https://{settings.AccountName}.blob.core.windows.net");

            return new BlobServiceClient(serviceUri, new DefaultAzureCredential());
        });

        services.AddScoped<IStorageService, BlobStorageService>();

        // Shared Service Bus client for the Topic publisher and consumers.
        services.AddSingleton<ServiceBusClient>(sp =>
        {
            var namespaceName =
                configuration["ServiceBus:FullyQualifiedNamespace"]
                ?? throw new InvalidOperationException(
                    "Service Bus namespace is not configured.");

            return new ServiceBusClient(
                namespaceName,
                new DefaultAzureCredential());
        });

        return services;
    }
}
