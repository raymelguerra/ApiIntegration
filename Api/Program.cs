using System.Reflection;
using System.Text.Json.Serialization;

using Api.HealthChecks;
using Api.Middleware;

using Application.DependencyInjections;

using HealthChecks.UI.Client;

using Infrastructure.DependencyInjections;

using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

// Configure Health Checks
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
}

builder.Services.AddHealthChecks()
    .AddCheck<ApiHealthCheck>("api", tags: new[]
    {
        "api",
        "ready"
    })
    .AddCheck<DatabaseHealthCheck>("database", tags: new[]
    {
        "database",
        "ready"
    })
    .AddCheck<QuartzHealthCheck>("quartz", tags: new[]
    {
        "scheduler",
        "ready"
    })
    .AddNpgSql(
    connectionString,
    name: "postgres_connection",
    tags: ["database", "infrastructure"],
    timeout: TimeSpan.FromSeconds(5));

// Configure controllers to use string enums
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Configure Swagger/OpenAPI with XML documentation
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "API Integration - Sync Scheduler",
        Description = "API for managing synchronization between A3 and GIM and execution history",
        Contact = new OpenApiContact
        {
            Name = "Development Team", Email = "raymel.ramos@businessinsights.es"
        }
    });

    // Include XML comments from the API project
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    // Enable annotations for better descriptions
    options.EnableAnnotations();

    // Add custom schema IDs to avoid conflicts
    options.CustomSchemaIds(type => type.FullName?.Replace("+", "."));

    // Configure enums to display as strings in Swagger
    options.UseInlineDefinitionsForEnums();
});

var app = builder.Build();

// Apply database migrations automatically (delegated to Infrastructure layer)
app.Services.ApplyDatabaseMigrations();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "API Integration v1");
        options.RoutePrefix = string.Empty;// Set Swagger UI at app's root
        options.DocumentTitle = "API Integration - Swagger UI";
        options.DisplayRequestDuration();
    });
}

// Register exception handling middleware first
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Map Health Check endpoints
app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => true, ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"), ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false,// Just check if the API is running
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.Run();