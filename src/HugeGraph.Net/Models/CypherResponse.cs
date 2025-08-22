using System.Text.Json.Serialization;

namespace HugeGraph.Net.Models
{
    /// <summary>
    /// Represents a complete response from the HugeGraph Cypher API.
    /// </summary>
    /// <typeparam name="T">The type of data returned in the result rows.</typeparam>
    public class CypherResponse<T>
    {
        /// <summary>
        /// Unique identifier for the request.
        /// </summary>
        [JsonPropertyName("requestId")]
        public string RequestId { get; set; } = string.Empty;

        /// <summary>
        /// Status information about the response.
        /// </summary>
        [JsonPropertyName("status")]
        public Status Status { get; set; } = new Status();

        /// <summary>
        /// The query result data and metadata.
        /// </summary>
        [JsonPropertyName("result")]
        public Result<T> Result { get; set; } = new Result<T>();
    }
}