# HugeGraph.NET

A strongly typed .NET client for executing Cypher queries against Apache HugeGraph databases. This library provides a modern, async API with support for dependency injection, logging, and OpenTelemetry tracing.

[![NuGet](https://img.shields.io/nuget/v/HugeGraph.Net.svg)](https://www.nuget.org/packages/HugeGraph.Net/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## Features

- ✅ **Type Safety**: Strongly typed response models and generic methods for result deserialization
- ✅ **Async/Await**: Full async API with `Task<CypherResponse<T>>` return types
- ✅ **Dependency Injection**: Built-in support for ASP.NET Core DI container
- ✅ **Logging**: Structured logging via `ILogger<T>` with configurable log levels
- ✅ **Telemetry**: OpenTelemetry integration for distributed tracing
- ✅ **Error Handling**: Custom exceptions with detailed error information
- ✅ **HTTP Optimization**: Automatic GET/POST selection based on query length
- ✅ **Authentication**: Support for Basic Auth and Bearer tokens
- ✅ **Testability**: Interface-based design for easy mocking in unit tests

## Installation

Install the package from NuGet:

```bash
dotnet add package HugeGraph.Net
```

## Quick Start

### Basic Usage

```csharp
using HugeGraph.Net;

// Configure the client
var options = new HugeGraphOptions
{
    BaseAddress = new Uri("http://localhost:8080"),
    GraphName = "hugegraph",
    RequestTimeout = TimeSpan.FromSeconds(30)
};

// Create the client
using var httpClient = new HttpClient();
using var client = new HugeGraphClient(httpClient, Options.Create(options));

// Execute a query with strongly typed results
var result = await client.ExecuteAsync<Person>("hugegraph", "g.V().hasLabel('person').limit(5)");

foreach (var person in result.Result.Data)
{
    Console.WriteLine($"Name: {person.Name}, Age: {person.Age}");
}

public class Person
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
}
```

### Dependency Injection (Recommended)

```csharp
using HugeGraph.Net.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var hostBuilder = Host.CreateDefaultBuilder()
    .ConfigureServices(services =>
    {
        // Register HugeGraph client
        services.AddHugeGraph(options =>
        {
            options.BaseAddress = new Uri("http://localhost:8080");
            options.GraphName = "hugegraph";
            options.Username = "admin";  // Optional
            options.Password = "admin";  // Optional
        });
    });

using var host = hostBuilder.Build();
await host.StartAsync();

// Use the client from DI
var client = host.Services.GetRequiredService<IHugeGraphClient>();
var result = await client.ExecuteAsync<dynamic>("hugegraph", "g.V().count()");
```

## Configuration

### HugeGraphOptions

```csharp
var options = new HugeGraphOptions
{
    BaseAddress = new Uri("http://localhost:8080"),
    GraphName = "hugegraph",              // Default graph name
    Username = "admin",                   // Basic auth username
    Password = "password",                // Basic auth password
    BearerToken = "your-bearer-token",    // Alternative to basic auth
    RequestTimeout = TimeSpan.FromSeconds(30),
    AdditionalHeaders = new Dictionary<string, string>
    {
        ["Custom-Header"] = "value"
    }
};
```

### ASP.NET Core Integration

In `Program.cs` or `Startup.cs`:

```csharp
builder.Services.AddHugeGraph(options =>
{
    options.BaseAddress = new Uri(builder.Configuration.GetConnectionString("HugeGraph")!);
    options.GraphName = builder.Configuration["HugeGraph:DefaultGraph"]!;
    options.Username = builder.Configuration["HugeGraph:Username"];
    options.Password = builder.Configuration["HugeGraph:Password"];
});
```

## API Reference

### IHugeGraphClient Interface

```csharp
public interface IHugeGraphClient
{
    // Execute query with strongly typed results
    Task<CypherResponse<TResult>> ExecuteAsync<TResult>(
        string graph, 
        string cypher, 
        CancellationToken cancellationToken = default);

    // Execute query with dynamic results
    Task<CypherResponse<dynamic>> ExecuteAsync(
        string graph, 
        string cypher, 
        CancellationToken cancellationToken = default);
}
```

### Response Models

```csharp
public class CypherResponse<T>
{
    public string RequestId { get; set; }
    public Status Status { get; set; }
    public Result<T> Result { get; set; }
}

public class Status
{
    public int Code { get; set; }
    public string Message { get; set; }
    public Dictionary<string, object>? Attributes { get; set; }
}

public class Result<T>
{
    public List<T> Data { get; set; }
    public Dictionary<string, object> Meta { get; set; }
}
```

## Error Handling

The library throws `HugeGraphException` for various error conditions:

```csharp
try
{
    var result = await client.ExecuteAsync<Person>("hugegraph", invalidQuery);
}
catch (HugeGraphException ex)
{
    Console.WriteLine($"HugeGraph error: {ex.Message}");
    
    if (ex.StatusCode.HasValue)
        Console.WriteLine($"HTTP Status: {ex.StatusCode}");
        
    if (ex.HugeGraphStatusCode.HasValue)
        Console.WriteLine($"HugeGraph Status: {ex.HugeGraphStatusCode}");
}
```

## Logging

The client integrates with .NET's `ILogger<T>` interface:

```csharp
services.AddLogging(builder =>
{
    builder.SetMinimumLevel(LogLevel.Debug);
    builder.AddConsole();
});

// The client will log:
// - Debug: Query execution details
// - Error: Failed requests and exceptions
```

## OpenTelemetry Integration

To enable distributed tracing, add the HugeGraph.Net activity source:

```csharp
services.AddOpenTelemetry()
    .WithTracing(builder =>
        builder.AddSource("HugeGraph.Net")
               .AddJaegerExporter());
```

The client emits activities with these tags:
- `http.method`: The HTTP method used (GET/POST)
- `http.url`: The request URL
- `db.statement`: The Cypher query string

## Testing

The library is designed for testability. Mock the `IHugeGraphClient` interface in your tests:

```csharp
[Test]
public async Task MyBusinessLogic_ShouldProcessPersons()
{
    // Arrange
    var mockClient = new Mock<IHugeGraphClient>();
    var expectedResponse = new CypherResponse<Person>
    {
        Status = new Status { Code = 200 },
        Result = new Result<Person> 
        { 
            Data = new List<Person> { new Person { Name = "John", Age = 30 } }
        }
    };
    
    mockClient.Setup(x => x.ExecuteAsync<Person>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(expectedResponse);
    
    var service = new MyService(mockClient.Object);
    
    // Act
    var result = await service.GetPersonsAsync();
    
    // Assert
    Assert.Single(result);
    Assert.Equal("John", result.First().Name);
}
```

## Performance Considerations

- **Query Length**: The client automatically uses HTTP GET for short queries and POST for longer ones (threshold: 2000 characters)
- **Connection Pooling**: Use `IHttpClientFactory` or dependency injection for proper HttpClient lifecycle management
- **Timeouts**: Configure appropriate timeouts based on your query complexity
- **Telemetry Overhead**: Tracing can be disabled via configuration if needed

## Examples

See the [sample application](./samples/HugeGraph.Net.Sample/) for complete examples including:
- Dependency injection setup
- Strongly typed queries
- Dynamic result handling
- Error handling patterns
- Logging configuration

## Requirements

- .NET 8.0 or later
- HugeGraph server with Cypher API endpoint

## Contributing

Contributions are welcome! Please feel free to submit issues, fork the repository, and create pull requests.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

- 📖 [HugeGraph Documentation](https://hugegraph.incubator.apache.org/)
- 🐛 [Report Issues](https://github.com/cisionmarkwalls/HugeGraph.NET/issues)
- 💬 [Discussions](https://github.com/cisionmarkwalls/HugeGraph.NET/discussions)
