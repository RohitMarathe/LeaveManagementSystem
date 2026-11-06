using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.Monitor.OpenTelemetry.Exporter;
using LeaveManagementSystem.Application.Interfaces.Repositories;
using LeaveManagementSystem.Infrastructure.Persistence.Context;
using LeaveManagementSystem.Infrastructure.Persistence.Repositories;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING")))
{
    builder.Services.AddOpenTelemetry()
        .UseFunctionsWorkerDefaults()
        .UseAzureMonitorExporter();
}

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "ConnectionStrings:DefaultConnection is not configured for the Outbox Publisher Function.");

var serviceBusNamespace = builder.Configuration["ServiceBus:FullyQualifiedNamespace"]
    ?? throw new InvalidOperationException(
        "ServiceBus:FullyQualifiedNamespace is not configured for the Outbox Publisher Function.");

builder.Services.AddDbContext<LeaveManagementDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IOutboxPublisherRepository, OutboxPublisherRepository>();

builder.Services.AddSingleton(_ =>
    new ServiceBusClient(serviceBusNamespace, new DefaultAzureCredential()));

builder.Services.AddSingleton(serviceBusClient =>
    serviceBusClient.GetRequiredService<ServiceBusClient>().CreateSender("leave-events"));

builder.Build().Run();
