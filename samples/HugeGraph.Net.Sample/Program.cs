using HugeGraph.Net;
using HugeGraph.Net.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HugeGraph.Net.Sample;

class Program
{
    static async Task Main(string[] args)
    {
        // Create a host builder to demonstrate dependency injection usage
        var hostBuilder = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                // Configure HugeGraph client with dependency injection
                services.AddHugeGraph(options =>
                {
                    options.BaseAddress = new Uri("http://localhost:8080");
                    options.GraphName = "hugegraph";
                    options.Username = "admin"; // Optional basic auth
                    options.Password = "admin"; // Optional basic auth
                    options.RequestTimeout = TimeSpan.FromSeconds(30);
                });
            })
            .ConfigureLogging(logging =>
            {
                logging.SetMinimumLevel(LogLevel.Debug);
                logging.AddConsole();
            });

        using var host = hostBuilder.Build();
        
        // Start the host
        await host.StartAsync();

        // Get the HugeGraph client from DI container
        var serviceProvider = host.Services;
        var hugeGraphClient = serviceProvider.GetRequiredService<IHugeGraphClient>();
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

        logger.LogInformation("Starting HugeGraph.Net Sample Application");

        try
        {
            // Example 1: Simple query with strongly typed results
            logger.LogInformation("Example 1: Executing a simple Cypher query...");
            
            var personQuery = "g.V().hasLabel('person').limit(5)";
            var personResult = await hugeGraphClient.ExecuteAsync<Person>("hugegraph", personQuery);
            
            logger.LogInformation("Query executed successfully. RequestId: {RequestId}", personResult.RequestId);
            logger.LogInformation("Status: {Code} - {Message}", personResult.Status.Code, personResult.Status.Message);
            logger.LogInformation("Found {Count} persons", personResult.Result.Data.Count);
            
            foreach (var person in personResult.Result.Data.Take(3))
            {
                logger.LogInformation("Person: {Name}, Age: {Age}", person.Name, person.Age);
            }

            // Example 2: Query with dynamic results
            logger.LogInformation("\nExample 2: Executing a query with dynamic results...");
            
            var countQuery = "g.V().count()";
            var countResult = await hugeGraphClient.ExecuteAsync("hugegraph", countQuery);
            
            logger.LogInformation("Total vertices count query executed");
            logger.LogInformation("Status: {Code} - {Message}", countResult.Status.Code, countResult.Status.Message);
            
            // Example 3: Creating a client manually (without DI)
            logger.LogInformation("\nExample 3: Creating client manually...");
            
            var options = new HugeGraphOptions
            {
                BaseAddress = new Uri("http://localhost:8080"),
                GraphName = "hugegraph",
                RequestTimeout = TimeSpan.FromSeconds(15)
            };
            
            using var httpClient = new HttpClient();
            using var manualClient = new HugeGraphClient(httpClient, Microsoft.Extensions.Options.Options.Create(options));
            
            var simpleQuery = "g.V().hasLabel('software').limit(3)";
            var softwareResult = await manualClient.ExecuteAsync<Software>("hugegraph", simpleQuery);
            
            logger.LogInformation("Manual client query executed successfully");
            logger.LogInformation("Found {Count} software items", softwareResult.Result.Data.Count);
            
            foreach (var software in softwareResult.Result.Data)
            {
                logger.LogInformation("Software: {Name}, Language: {Language}", software.Name, software.Language);
            }

            // Example 4: Error handling
            logger.LogInformation("\nExample 4: Demonstrating error handling...");
            
            try
            {
                var invalidQuery = "INVALID CYPHER SYNTAX";
                await hugeGraphClient.ExecuteAsync<object>("hugegraph", invalidQuery);
            }
            catch (HugeGraphException ex)
            {
                logger.LogWarning("Expected HugeGraphException caught: {Message}", ex.Message);
                if (ex.StatusCode.HasValue)
                {
                    logger.LogWarning("HTTP Status Code: {StatusCode}", ex.StatusCode.Value);
                }
            }
            
            logger.LogInformation("\nSample application completed successfully!");
        }
        catch (HugeGraphException ex)
        {
            logger.LogError(ex, "HugeGraph error occurred: {Message}", ex.Message);
            if (ex.StatusCode.HasValue)
            {
                logger.LogError("HTTP Status Code: {StatusCode}", ex.StatusCode.Value);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error occurred: {Message}", ex.Message);
        }
        finally
        {
            await host.StopAsync();
        }
    }
}

// Example POCOs for strongly typed results
public class Person
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public string City { get; set; } = string.Empty;
}

public class Software
{
    public string Name { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
}
