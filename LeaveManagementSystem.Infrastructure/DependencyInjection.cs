using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using LeaveManagementSystem.Application.Interfaces.Repositories;
using LeaveManagementSystem.Application.Interfaces.Services;
using LeaveManagementSystem.Application.Services;
using LeaveManagementSystem.Infrastructure.Configuration;
using LeaveManagementSystem.Infrastructure.Persistence;
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
        // Register Database Context
        services.AddDbContext<LeaveManagementDbContext>(options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"));
        });

        // Register Repositories
        services.AddScoped(
            typeof(IGenericRepository<,>),
            typeof(GenericRepository<,>));

        services.AddScoped<IEmployeeRepository, EmployeeRepository>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IOutboxWriter, OutboxWriter>();

        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();

        // Register Blob Storage Configuration
        services.AddOptions<BlobStorageSettings>()
            .Bind(configuration.GetSection(BlobStorageSettings.SectionName))
            .ValidateOnStart();

        // Register Azure BlobServiceClient
        services.AddSingleton<BlobServiceClient>(sp =>
        {
            var settings = sp
                .GetRequiredService<
                    Microsoft.Extensions.Options.IOptions<BlobStorageSettings>>()
                .Value;

            Uri serviceUri = new Uri(
                $"https://{settings.AccountName}.blob.core.windows.net");

            return new BlobServiceClient(serviceUri, new DefaultAzureCredential());
        });

        services.AddScoped<IStorageService, BlobStorageService>();

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

        // Existing Queue workflow registration. The Topic publisher will
        // have its own sender abstraction in the publishing phase.
        services.AddSingleton<ServiceBusSender>(sp =>
        {
            var client = sp.GetRequiredService<ServiceBusClient>();
            return client.CreateSender("leave-requests");
        });

        services.AddScoped<IMessageBusService, ServiceBusMessageBusService>();

        return services;
    }
}
