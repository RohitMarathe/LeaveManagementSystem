using LeaveManagementSystem.Application;
using LeaveManagementSystem.Infrastructure;
using Microsoft.Extensions.Logging.ApplicationInsights;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Register the Services
builder.Services.AddApplication(); // Register application services
builder.Services.AddInfrastructure(builder.Configuration); // Register infrastructure services

// Configuring the Application Insights
builder.Services.AddApplicationInsightsTelemetry();

builder.Logging.AddConsole().AddApplicationInsights(
    configureTelemetryConfiguration: (config) =>
    {
        var connectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            config.ConnectionString = connectionString;
        }
    },
    configureApplicationInsightsLoggerOptions: (options) => { }
);
var logLevel = builder.Configuration.GetValue<LogLevel>("Logging:LogLevel:Default");

builder.Logging.AddFilter<ApplicationInsightsLoggerProvider>(string.Empty, logLevel);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
