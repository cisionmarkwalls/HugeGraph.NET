using HugeGraph.Net.Models;
using System.Text.Json;

namespace HugeGraph.Net.Tests
{
    public class ModelTests
    {
        [Fact]
        public void Status_SerializationRoundTrip_Succeeds()
        {
            // Arrange
            var status = new Status
            {
                Code = 200,
                Message = "OK",
                Attributes = new Dictionary<string, object>
                {
                    { "execution_time", 150 },
                    { "query_type", "read" }
                }
            };

            // Act
            var json = JsonSerializer.Serialize(status);
            var deserialized = JsonSerializer.Deserialize<Status>(json);

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal(200, deserialized.Code);
            Assert.Equal("OK", deserialized.Message);
            Assert.NotNull(deserialized.Attributes);
            Assert.Equal(2, deserialized.Attributes.Count);
        }

        [Fact]
        public void Result_SerializationRoundTrip_Succeeds()
        {
            // Arrange
            var result = new Result<TestNode>
            {
                Data = new List<TestNode>
                {
                    new TestNode { Id = 1, Name = "Node1" },
                    new TestNode { Id = 2, Name = "Node2" }
                },
                Meta = new Dictionary<string, object>
                {
                    { "count", 2 },
                    { "has_more", false }
                }
            };

            // Act
            var json = JsonSerializer.Serialize(result);
            var deserialized = JsonSerializer.Deserialize<Result<TestNode>>(json);

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal(2, deserialized.Data.Count);
            Assert.Equal("Node1", deserialized.Data[0].Name);
            Assert.Equal("Node2", deserialized.Data[1].Name);
            Assert.Equal(2, deserialized.Meta.Count);
        }

        [Fact]
        public void CypherResponse_SerializationRoundTrip_Succeeds()
        {
            // Arrange
            var response = new CypherResponse<TestNode>
            {
                RequestId = "req-123",
                Status = new Status { Code = 200, Message = "OK" },
                Result = new Result<TestNode>
                {
                    Data = new List<TestNode> { new TestNode { Id = 1, Name = "Test" } },
                    Meta = new Dictionary<string, object> { { "count", 1 } }
                }
            };

            // Act
            var json = JsonSerializer.Serialize(response);
            var deserialized = JsonSerializer.Deserialize<CypherResponse<TestNode>>(json);

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal("req-123", deserialized.RequestId);
            Assert.Equal(200, deserialized.Status.Code);
            Assert.Equal("OK", deserialized.Status.Message);
            Assert.Single(deserialized.Result.Data);
            Assert.Equal(1, deserialized.Result.Data[0].Id);
            Assert.Equal("Test", deserialized.Result.Data[0].Name);
        }

        private class TestNode
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
        }
    }
}