using HugeGraph.Net;
using HugeGraph.Net.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;

namespace HugeGraph.Net.Tests
{
    public class HugeGraphClientTests
    {
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly Mock<ILogger<IHugeGraphClient>> _mockLogger;
        private readonly HugeGraphOptions _options;
        private readonly HttpClient _httpClient;

        public HugeGraphClientTests()
        {
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _mockLogger = new Mock<ILogger<IHugeGraphClient>>();
            _options = new HugeGraphOptions
            {
                BaseAddress = new Uri("http://localhost:8080"),
                GraphName = "testgraph",
                RequestTimeout = TimeSpan.FromSeconds(30)
            };
            
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object)
            {
                BaseAddress = _options.BaseAddress
            };
        }

        [Fact]
        public async Task ExecuteAsync_WithValidQuery_ReturnsExpectedResponse()
        {
            // Arrange
            var expectedResponse = new CypherResponse<TestNode>
            {
                RequestId = "test-request-id",
                Status = new Status { Code = 200, Message = "OK" },
                Result = new Result<TestNode>
                {
                    Data = new List<TestNode> { new TestNode { Id = 1, Name = "Test" } },
                    Meta = new Dictionary<string, object> { { "count", 1 } }
                }
            };

            var responseJson = JsonSerializer.Serialize(expectedResponse);
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson)
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponseMessage);

            var client = new HugeGraphClient(_httpClient, Options.Create(_options), _mockLogger.Object);

            // Act
            var result = await client.ExecuteAsync<TestNode>("testgraph", "MATCH (n) RETURN n");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("test-request-id", result.RequestId);
            Assert.Equal(200, result.Status.Code);
            Assert.Equal("OK", result.Status.Message);
            Assert.Single(result.Result.Data);
            Assert.Equal(1, result.Result.Data[0].Id);
            Assert.Equal("Test", result.Result.Data[0].Name);
        }

        [Fact]
        public async Task ExecuteAsync_WithHttpError_ThrowsHugeGraphException()
        {
            // Arrange
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("Internal Server Error")
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponseMessage);

            var client = new HugeGraphClient(_httpClient, Options.Create(_options), _mockLogger.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<HugeGraphException>(
                () => client.ExecuteAsync<TestNode>("testgraph", "MATCH (n) RETURN n"));
            
            Assert.Contains("InternalServerError", exception.Message);
            Assert.Equal(500, exception.StatusCode);
        }

        [Fact]
        public async Task ExecuteAsync_WithEmptyGraphParameter_UsesDefaultGraph()
        {
            // Arrange
            var expectedResponse = new CypherResponse<TestNode>
            {
                RequestId = "test-request-id",
                Status = new Status { Code = 200, Message = "OK" },
                Result = new Result<TestNode> { Data = new List<TestNode>(), Meta = new Dictionary<string, object>() }
            };

            var responseJson = JsonSerializer.Serialize(expectedResponse);
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson)
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().Contains("testgraph")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponseMessage);

            var client = new HugeGraphClient(_httpClient, Options.Create(_options), _mockLogger.Object);

            // Act
            var result = await client.ExecuteAsync<TestNode>("", "MATCH (n) RETURN n");

            // Assert
            Assert.NotNull(result);
            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().Contains("testgraph")),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task ExecuteAsync_WithLongQuery_UsesPostMethod()
        {
            // Arrange
            var longQuery = new string('A', 3000); // Create a long query string
            var expectedResponse = new CypherResponse<TestNode>
            {
                RequestId = "test-request-id",
                Status = new Status { Code = 200, Message = "OK" },
                Result = new Result<TestNode> { Data = new List<TestNode>(), Meta = new Dictionary<string, object>() }
            };

            var responseJson = JsonSerializer.Serialize(expectedResponse);
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson)
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponseMessage);

            var client = new HugeGraphClient(_httpClient, Options.Create(_options), _mockLogger.Object);

            // Act
            var result = await client.ExecuteAsync<TestNode>("testgraph", longQuery);

            // Assert
            Assert.NotNull(result);
            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task ExecuteAsync_WithShortQuery_UsesGetMethod()
        {
            // Arrange
            var shortQuery = "MATCH (n) RETURN n";
            var expectedResponse = new CypherResponse<TestNode>
            {
                RequestId = "test-request-id",
                Status = new Status { Code = 200, Message = "OK" },
                Result = new Result<TestNode> { Data = new List<TestNode>(), Meta = new Dictionary<string, object>() }
            };

            var responseJson = JsonSerializer.Serialize(expectedResponse);
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson)
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponseMessage);

            var client = new HugeGraphClient(_httpClient, Options.Create(_options), _mockLogger.Object);

            // Act
            var result = await client.ExecuteAsync<TestNode>("testgraph", shortQuery);

            // Assert
            Assert.NotNull(result);
            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task ExecuteAsync_DynamicOverload_ReturnsExpectedResponse()
        {
            // Arrange
            var expectedResponse = new CypherResponse<dynamic>
            {
                RequestId = "test-request-id",
                Status = new Status { Code = 200, Message = "OK" },
                Result = new Result<dynamic>
                {
                    Data = new List<dynamic> { new { id = 1, name = "Test" } },
                    Meta = new Dictionary<string, object> { { "count", 1 } }
                }
            };

            var responseJson = JsonSerializer.Serialize(expectedResponse);
            var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson)
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponseMessage);

            var client = new HugeGraphClient(_httpClient, Options.Create(_options), _mockLogger.Object);

            // Act
            var result = await client.ExecuteAsync("testgraph", "MATCH (n) RETURN n");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("test-request-id", result.RequestId);
            Assert.Equal(200, result.Status.Code);
        }

        private class TestNode
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
        }
    }
}