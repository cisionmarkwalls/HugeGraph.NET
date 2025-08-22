# HugeGraph.Net Sample Application

This sample console application demonstrates how to use the HugeGraph.Net library in various scenarios.

## Features Demonstrated

1. **Dependency Injection Setup**: How to register HugeGraph client with the .NET hosting model
2. **Strongly Typed Queries**: Executing queries with custom POCO types
3. **Dynamic Results**: Working with dynamic query results
4. **Manual Client Creation**: Creating clients without dependency injection
5. **Error Handling**: Proper exception handling patterns
6. **Logging Integration**: Using structured logging with the client

## Prerequisites

- .NET 8.0 or later
- HugeGraph server running on `localhost:8080` (configurable)
- Sample data in the HugeGraph database (optional, for meaningful results)

## Running the Sample

1. Ensure your HugeGraph server is running
2. Update the connection settings in `Program.cs` if needed
3. Run the application:

```bash
dotnet run
```

## Configuration

The sample uses these default settings:
- **Base Address**: `http://localhost:8080`
- **Graph Name**: `hugegraph`
- **Username**: `admin`
- **Password**: `admin`

Modify these in the `Program.cs` file to match your HugeGraph setup.

## Expected Output

The application will demonstrate:
- Successful query execution with detailed logging
- Proper error handling for invalid queries
- Different ways to create and use the client
- Integration with the .NET hosting and logging infrastructure

## Sample Queries

The application includes sample queries for:
- Finding persons: `g.V().hasLabel('person').limit(5)`
- Counting vertices: `g.V().count()`
- Finding software: `g.V().hasLabel('software').limit(3)`

These queries assume standard HugeGraph sample data. Adjust as needed for your schema.

## Learning Points

After running this sample, you should understand:
- How to set up HugeGraph.Net with dependency injection
- How to execute both strongly typed and dynamic queries
- How to handle errors gracefully
- How to integrate logging and telemetry
- How to create clients manually when DI is not available