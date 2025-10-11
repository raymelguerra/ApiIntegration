using Application.Exceptions;
using Domain.Exceptions;
using Infrastructure.Exceptions;
using Infrastructure.Policies;
using Microsoft.Extensions.Logging;
using Polly;

namespace Examples;

/// <summary>
/// Practical examples of using the exception handling layer
/// </summary>
public class ExceptionHandlingExamples
{
    // EXAMPLE 1: Domain Layer - Entity Not Found
    public class ProductService
    {
        public Product GetProduct(int id)
        {
            var product = FindProductInDatabase(id);
            if (product == null)
            {
                throw new EntityNotFoundException(nameof(Product), id);
            }
            return product;
        }

        private Product? FindProductInDatabase(int id) => null; // Stub
    }

    // EXAMPLE 2: Domain Layer - Business Rule Validation
    public class OrderService
    {
        public void PlaceOrder(Order order)
        {
            if (order.Items.Count == 0)
            {
                throw new BusinessRuleValidationException(
                    "Cannot place an order without items");
            }

            if (order.TotalAmount < 0)
            {
                throw new BusinessRuleValidationException(
                    "Order total cannot be negative");
            }

            // Process order...
        }
    }

    // EXAMPLE 3: Application Layer - Validation Exception
    public class CreateProductValidator
    {
        public void Validate(CreateProductRequest request)
        {
            var errors = new Dictionary<string, string[]>();

            if (string.IsNullOrWhiteSpace(request.Name))
                errors.Add(nameof(request.Name), new[] { "Product name is required" });

            if (request.Price <= 0)
                errors.Add(nameof(request.Price), new[] { "Price must be greater than zero" });

            if (request.Stock < 0)
                errors.Add(nameof(request.Stock), new[] { "Stock cannot be negative" });

            if (errors.Any())
                throw new ValidationException(errors);
        }
    }

    // EXAMPLE 4: Application Layer - Command Handler with Exception Wrapping
    public class UpdateProductCommandHandler
    {
        private readonly IProductRepository _repository;
        private readonly ILogger<UpdateProductCommandHandler> _logger;

        public UpdateProductCommandHandler(
            IProductRepository repository,
            ILogger<UpdateProductCommandHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task Handle(UpdateProductCommand command)
        {
            try
            {
                var product = await _repository.GetByIdAsync(command.Id);
                product.Update(command.Name, command.Price);
                await _repository.SaveAsync(product);
            }
            catch (EntityNotFoundException)
            {
                // Let domain exceptions bubble up
                throw;
            }
            catch (Exception ex)
            {
                // Wrap unexpected exceptions
                _logger.LogError(ex, "Failed to update product {ProductId}", command.Id);
                throw new CommandHandlerException(nameof(UpdateProductCommand), ex);
            }
        }
    }

    // EXAMPLE 5: Infrastructure Layer - External API with Exception Handling
    public class ExternalApiClient
    {
        private readonly HttpClient _httpClient;

        public ExternalApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<MaterialResponse> GetMaterialAsync(string code)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/materials/{code}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new ExternalApiException(
                        "MaterialAPI",
                        (int)response.StatusCode,
                        errorContent);
                }

                return await response.Content.ReadFromJsonAsync<MaterialResponse>()
                    ?? throw new ExternalApiException("MaterialAPI", "Response was null");
            }
            catch (HttpRequestException ex)
            {
                throw new ExternalApiException("MaterialAPI", "Connection failed", ex);
            }
            catch (TaskCanceledException ex)
            {
                throw new ExternalApiException("MaterialAPI", "Request timed out", ex);
            }
        }
    }

    // EXAMPLE 6: Infrastructure Layer - Database Operations with Retry
    public class ProductRepository
    {
        private readonly DbContext _context;
        private readonly ILogger<ProductRepository> _logger;

        public ProductRepository(DbContext dbContext, ILogger<ProductRepository> logger)
        {
            _context = dbContext;
            _logger = logger;
        }

        public async Task SaveAsync(Product product)
        {
            var retryPolicy = ResiliencePolicies.GetDatabaseRetryPolicy(_logger);

            await retryPolicy.ExecuteAsync(async () =>
            {
                try
                {
                    _context.Products.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException ex)
                {
                    throw new DatabaseException("Failed to save product", ex);
                }
            });
        }

        public async Task<List<Product>> GetExpensiveReportAsync()
        {
            var retryPolicy = ResiliencePolicies.GetDatabaseRetryPolicy(_logger);
            var timeoutPolicy = ResiliencePolicies.GetTimeoutPolicy(TimeSpan.FromMinutes(2));
            var combinedPolicy = Policy.WrapAsync(timeoutPolicy, retryPolicy);

            return await combinedPolicy.ExecuteAsync(async () =>
            {
                try
                {
                    return await _context.Products
                        .Include(p => p.Orders)
                        .ToListAsync();
                }
                catch (Exception ex) when (ex.Message.Contains("timeout"))
                {
                    throw new DatabaseException("Query timed out", ex);
                }
            });
        }
    }

    // EXAMPLE 7: Quartz Job with Comprehensive Error Handling (already implemented)
    // See: Infrastructure/Jobs/GenericSyncJob.cs

    // EXAMPLE 8: Using Polly with External Services
    public class IntegrationService
    {
        private readonly ILogger<IntegrationService> _logger;
        private readonly HttpClient _httpClient;

        public IntegrationService(ILogger<IntegrationService> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<SyncResult> SyncDataAsync(string endpoint)
        {
            var apiRetry = ResiliencePolicies.GetExternalApiRetryPolicy(_logger, "SyncAPI");
            var apiCircuitBreaker = ResiliencePolicies.GetExternalApiCircuitBreakerPolicy(_logger, "SyncAPI");
            var timeout = ResiliencePolicies.GetTimeoutPolicy(TimeSpan.FromMinutes(5));

            // Wrap policies: timeout -> retry -> circuit breaker
            var policy = Policy.WrapAsync(timeout, apiRetry, apiCircuitBreaker);

            try
            {
                return await policy.ExecuteAsync(async () =>
                {
                    var response = await _httpClient.GetAsync(endpoint);
                    
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new ExternalApiException(
                            "SyncAPI",
                            (int)response.StatusCode,
                            "Sync request failed");
                    }

                    return await response.Content.ReadFromJsonAsync<SyncResult>()
                        ?? throw new ExternalApiException("SyncAPI", "Empty response");
                });
            }
            catch (Polly.CircuitBreaker.BrokenCircuitException)
            {
                throw new CircuitBreakerOpenException("SyncAPI");
            }
            catch (Polly.Timeout.TimeoutRejectedException ex)
            {
                throw new Infrastructure.Exceptions.TimeoutException("SyncDataAsync", TimeSpan.FromMinutes(5));
            }
        }
    }

    // EXAMPLE 9: Controller - No Try-Catch Needed!
    // The middleware handles everything automatically
    /*
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ISender _sender;

        public ProductsController(ISender sender)
        {
            _sender = sender;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> Get(int id)
        {
            // No try-catch needed!
            // If EntityNotFoundException is thrown, middleware returns 404
            var query = new GetProductQuery(id);
            var result = await _sender.Send(query);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<int>> Create(CreateProductRequest request)
        {
            // No try-catch needed!
            // If ValidationException is thrown, middleware returns 400 with details
            var command = new CreateProductCommand(request);
            var id = await _sender.Send(command);
            return CreatedAtAction(nameof(Get), new { id }, id);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, UpdateProductRequest request)
        {
            // Multiple exceptions might be thrown:
            // - EntityNotFoundException -> 404
            // - BusinessRuleValidationException -> 400
            // - DatabaseException -> 500
            // All handled automatically by middleware!
            var command = new UpdateProductCommand(id, request);
            await _sender.Send(command);
            return NoContent();
        }
    }
    */

    // EXAMPLE 10: Testing Exception Scenarios
    /*
    public class ExceptionHandlingTests
    {
        [Fact]
        public async Task GetProduct_WhenNotFound_ReturnsNotFound()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/products/999");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            
            var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            Assert.Equal("ENTITY_NOT_FOUND", problem.Extensions["errorCode"]);
            Assert.Contains("Product", problem.Detail);
        }

        [Fact]
        public async Task CreateProduct_WithInvalidData_ReturnsBadRequest()
        {
            // Arrange
            var client = _factory.CreateClient();
            var request = new CreateProductRequest { Name = "", Price = -10 };

            // Act
            var response = await client.PostAsJsonAsync("/api/products", request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            
            var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            Assert.Equal("VALIDATION_ERROR", problem.Extensions["errorCode"]);
            Assert.Contains("Name", problem.Errors.Keys);
            Assert.Contains("Price", problem.Errors.Keys);
        }
    }
    */

    #region Supporting Types (for compilation)
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public void Update(string name, decimal price) { }
    }

    public class Order
    {
        public List<string> Items { get; set; } = new();
        public decimal TotalAmount { get; set; }
    }

    public class CreateProductRequest
    {
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public int Stock { get; set; }
    }

    public class UpdateProductRequest
    {
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
    }

    public class UpdateProductCommand
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
    }

    public class MaterialResponse { }
    public class SyncResult { }
    public class DbContext 
    { 
        public DbSet<Product> Products { get; set; } = null!;
        public Task<int> SaveChangesAsync() => Task.FromResult(0);
    }
    public class DbSet<T> where T : class
    {
        public void Update(T entity) { }
        public DbSet<T> Include(System.Linq.Expressions.Expression<Func<T, object>> navigationProperty) => this;
        public Task<List<T>> ToListAsync() => Task.FromResult(new List<T>());
    }
    public class DbUpdateException : Exception { }
    public interface IProductRepository 
    { 
        Task<Product> GetByIdAsync(int id);
        Task SaveAsync(Product product);
    }
    #endregion
}

