# Exception Handling Layer Documentation

## Overview

This project implements a comprehensive exception handling system following hexagonal architecture principles with full integration of Polly resilience policies and Quartz job exception handling.

## Architecture

### Exception Hierarchy

```
Exception
├── Domain.Exceptions.DomainException (Domain Layer)
│   ├── EntityNotFoundException
│   ├── EntityAlreadyExistsException
│   ├── BusinessRuleValidationException
│   └── InvalidEntityStateException
│
├── Application.Exceptions.ApplicationException (Application Layer)
│   ├── ValidationException
│   ├── UnauthorizedException
│   ├── ForbiddenException
│   ├── CommandHandlerException
│   ├── QueryHandlerException
│   └── SyncJobException
│
├── Infrastructure.Exceptions.InfrastructureException (Infrastructure Layer)
│   ├── DatabaseException
│   ├── ExternalApiException
│   ├── QuartzJobException
│   ├── CachingException
│   ├── MessageBrokerException
│   ├── CircuitBreakerOpenException
│   └── TimeoutException
│
└── Api.Exceptions.HttpException (API Layer)
    ├── BadRequestException
    ├── NotFoundException
    ├── ConflictException
    ├── InternalServerErrorException
    └── ServiceUnavailableException
```

## Components

### 1. Exception Classes

#### Domain Layer (`/Domain/Exceptions/DomainException.cs`)
- Base class: `DomainException`
- Business rule violations and entity-related errors
- Error codes for identification

#### Application Layer (`/Application/Exceptions/ApplicationException.cs`)
- Base class: `ApplicationException`
- Use case and validation errors
- Command/Query handler failures

#### Infrastructure Layer (`/Infrastructure/Exceptions/InfrastructureException.cs`)
- Base class: `InfrastructureException`
- External service failures
- Database and messaging errors
- Circuit breaker and timeout errors

#### API Layer (`/Api/Exceptions/HttpException.cs`)
- Base class: `HttpException`
- HTTP-specific exceptions with status codes
- Request/response errors

### 2. Exception Handling Middleware

**Location:** `/Api/Middleware/ExceptionHandlingMiddleware.cs`

**Features:**
- Global exception catching
- Automatic ProblemDetails conversion
- HTTP status code mapping
- Structured logging
- Environment-specific error details (stack traces only in Development)

**Response Format:**
```json
{
  "type": "https://httpstatuses.com/404",
  "title": "Not Found",
  "status": 404,
  "detail": "Entity with id '123' was not found.",
  "instance": "/api/entities/123",
  "errorCode": "ENTITY_NOT_FOUND",
  "traceId": "00-abc123...",
  "timestamp": "2025-10-06T10:30:00Z"
}
```

### 3. Polly Resilience Policies

#### HTTP Policies (`/Infrastructure/Policies/HttpPolicies.cs`)

**Retry Policy:**
- 3 retries with exponential backoff (2s, 5s, 10s)
- Handles transient HTTP errors (5xx)
- Handles rate limiting (429)

**Circuit Breaker:**
- Opens after 5 consecutive failures
- Remains open for 30 seconds
- Automatic half-open state testing

**Timeout Policy:**
- Default 30 seconds
- Configurable per client

#### General Resilience Policies (`/Infrastructure/Policies/ResiliencePolicies.cs`)

**Database Retry Policy:**
- 3 retries with exponential backoff
- Handles timeout, deadlock, connection errors
- Structured logging on each retry

**External API Policies:**
- Retry with backoff
- Circuit breaker pattern
- Custom per-API configuration

**Quartz Job Policies:**
- 2 retries for transient errors
- Skip retry for validation/business rule errors
- Timeout protection

### 4. Quartz Job Exception Handling

#### Enhanced Job (`/Infrastructure/Jobs/GenericSyncJob.cs`)
- Wraps execution with retry and timeout policies
- Catches and categorizes exceptions
- Converts to QuartzJobException with context

#### Job Listener (`/Infrastructure/Quartz/QuartzJobExceptionListener.cs`)
- Monitors all job executions
- Logs start, completion, and failures
- Stores exception details in JobDataMap
- Identifies transient vs non-transient errors

## Usage Examples

### 1. Throwing Domain Exceptions

```csharp
// In your domain entities or value objects
public class Product
{
    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice <= 0)
            throw new BusinessRuleValidationException("Price must be greater than zero");
        
        // Update logic
    }
}

// In your repository
public async Task<Product> GetByIdAsync(int id)
{
    var product = await _context.Products.FindAsync(id);
    if (product == null)
        throw new EntityNotFoundException(nameof(Product), id);
    
    return product;
}
```

### 2. Throwing Application Exceptions

```csharp
// In your command handlers
public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Unit>
{
    public async Task<Unit> Handle(UpdateProductCommand request, CancellationToken ct)
    {
        try
        {
            // Your logic
        }
        catch (Exception ex)
        {
            throw new CommandHandlerException(nameof(UpdateProductCommand), ex);
        }
    }
}

// Validation example
public void ValidateRequest(CreateProductRequest request)
{
    var errors = new Dictionary<string, string[]>();
    
    if (string.IsNullOrEmpty(request.Name))
        errors.Add("Name", new[] { "Name is required" });
    
    if (request.Price <= 0)
        errors.Add("Price", new[] { "Price must be greater than zero" });
    
    if (errors.Any())
        throw new ValidationException(errors);
}
```

### 3. Throwing Infrastructure Exceptions

```csharp
// In your HTTP clients
public class A3ApiClient : IA3ApiClient
{
    private readonly HttpClient _httpClient;
    
    public async Task<MaterialDto> GetMaterialAsync(string code)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/materials/{code}");
            
            if (!response.IsSuccessStatusCode)
            {
                throw new ExternalApiException(
                    "A3 API", 
                    (int)response.StatusCode,
                    await response.Content.ReadAsStringAsync());
            }
            
            return await response.Content.ReadFromJsonAsync<MaterialDto>();
        }
        catch (HttpRequestException ex)
        {
            throw new ExternalApiException("A3 API", "Request failed", ex);
        }
    }
}

// In your repositories
public class ProductRepository
{
    public async Task SaveAsync(Product product)
    {
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new DatabaseException("Failed to save product", ex);
        }
    }
}
```

### 4. Using Polly Policies in Services

```csharp
// In your custom service
public class ReportService
{
    private readonly ILogger<ReportService> _logger;
    
    public async Task GenerateReportAsync()
    {
        var retryPolicy = ResiliencePolicies.GetDatabaseRetryPolicy(_logger);
        var timeoutPolicy = ResiliencePolicies.GetTimeoutPolicy(TimeSpan.FromMinutes(5));
        var combinedPolicy = Policy.WrapAsync(timeoutPolicy, retryPolicy);
        
        await combinedPolicy.ExecuteAsync(async () =>
        {
            // Your database-intensive operation
            await _repository.GenerateComplexReportAsync();
        });
    }
}
```

### 5. Handling Exceptions in Controllers

```csharp
// Controllers don't need try-catch blocks!
// The middleware handles everything automatically

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ISender _sender;
    
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> Get(int id)
    {
        // If not found, repository throws EntityNotFoundException
        // Middleware converts it to 404 response automatically
        var query = new GetProductQuery(id);
        var result = await _sender.Send(query);
        return Ok(result);
    }
    
    [HttpPost]
    public async Task<ActionResult<int>> Create(CreateProductCommand command)
    {
        // If validation fails, ValidationException is thrown
        // Middleware converts it to 400 with validation details
        var id = await _sender.Send(command);
        return CreatedAtAction(nameof(Get), new { id }, id);
    }
}
```

## HTTP Status Code Mapping

| Exception Type | HTTP Status | Error Code |
|---------------|-------------|------------|
| EntityNotFoundException | 404 Not Found | ENTITY_NOT_FOUND |
| EntityAlreadyExistsException | 409 Conflict | ENTITY_ALREADY_EXISTS |
| BusinessRuleValidationException | 400 Bad Request | BUSINESS_RULE_VIOLATION |
| ValidationException | 400 Bad Request | VALIDATION_ERROR |
| UnauthorizedException | 401 Unauthorized | UNAUTHORIZED |
| ForbiddenException | 403 Forbidden | FORBIDDEN |
| CircuitBreakerOpenException | 503 Service Unavailable | CIRCUIT_BREAKER_OPEN |
| TimeoutException | 408 Request Timeout | OPERATION_TIMEOUT |
| ExternalApiException | 502 Bad Gateway | EXTERNAL_API_ERROR |
| DatabaseException | 500 Internal Server Error | DATABASE_ERROR |

## Configuration

### Middleware Registration (Already done in Program.cs)

```csharp
app.UseMiddleware<ExceptionHandlingMiddleware>();
```

### HTTP Client Configuration (Already done)

```csharp
services.AddHttpClient<IA3ApiClient, A3ApiClient>()
    .AddPolicyHandler(HttpPolicies.GetHttpRetryPolicy())
    .AddPolicyHandler(HttpPolicies.GetHttpCircuitBreakerPolicy())
    .AddPolicyHandler(HttpPolicies.GetTimeoutPolicy(30));
```

### Quartz Job Listener (Already configured)

```csharp
options.AddJobListener<QuartzJobExceptionListener>();
```

## Benefits

1. **Consistency**: All exceptions follow the same structure
2. **Traceability**: Every error has a unique trace ID and error code
3. **Resilience**: Automatic retries and circuit breakers
4. **Observability**: Structured logging at every level
5. **User-Friendly**: Clean API responses with appropriate status codes
6. **Development-Friendly**: Stack traces in development environment
7. **Production-Safe**: Sensitive details hidden in production
8. **Testability**: Easy to test error scenarios

## Monitoring & Logging

All exceptions are automatically logged with:
- Exception type and message
- Error code
- Trace ID (correlation)
- Timestamp
- Request path
- Stack trace (development only)

Use these logs for:
- Error rate monitoring
- Circuit breaker state tracking
- Retry pattern analysis
- Performance bottleneck identification

## Best Practices

1. **Always throw specific exceptions** - Use the most specific exception type available
2. **Include context** - Add relevant information (entity names, IDs, etc.)
3. **Don't catch and ignore** - Let exceptions bubble up to the middleware
4. **Use error codes** - They help with monitoring and client-side handling
5. **Log at the source** - Log when throwing infrastructure exceptions
6. **Test error scenarios** - Write tests for exception paths
7. **Document error codes** - Keep a catalog of possible error codes

## Testing

```csharp
// Example test for exception handling
[Fact]
public async Task GetProduct_WhenNotFound_Returns404()
{
    // Arrange
    var productId = 999;
    
    // Act
    var response = await _client.GetAsync($"/api/products/{productId}");
    
    // Assert
    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    
    var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
    Assert.Equal("ENTITY_NOT_FOUND", problem.Extensions["errorCode"]);
}
```

## Next Steps

Consider adding:
- Health checks for circuit breaker states
- Metrics collection (Prometheus/Application Insights)
- Dead letter queue for failed jobs
- Notification system for critical errors
- Custom error pages for web UI

