using System.Reflection;
using Application.DependencyInjections;
using Infrastructure.DependencyInjections;
using Api.Middleware;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new NullReferenceException();
builder.Services.AddInfrastructure(connectionString);
builder.Services.AddApplication();
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
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
            Name = "Development Team",
            Email = "raymel.ramos@businessinsights.es",
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
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "API Integration v1");
        options.RoutePrefix = string.Empty; // Set Swagger UI at app's root
        options.DocumentTitle = "API Integration - Swagger UI";
        options.DisplayRequestDuration();
    });
}

// Register exception handling middleware first
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();