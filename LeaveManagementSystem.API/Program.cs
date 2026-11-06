using LeaveManagementSystem.API.Middleware;
using LeaveManagementSystem.Application;
using LeaveManagementSystem.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Register the Services
builder.Services.AddApplication(); // Register application services
builder.Services.AddInfrastructure(builder.Configuration); // Register infrastructure services

// Register Swagger services BEFORE Build()
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configuring the Application Insights
builder.Services.AddApplicationInsightsTelemetry();

var app = builder.Build();

app.UseGlobalExceptionMiddleware();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
