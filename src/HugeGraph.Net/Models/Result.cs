using System.Text.Json.Serialization;

namespace HugeGraph.Net.Models
{
    /// <summary>
    /// Represents the result portion of a HugeGraph Cypher API response.
    /// </summary>
    /// <typeparam name="T">The type of data returned in each row.</typeparam>
    public class Result<T>
    {
        /// <summary>
        /// The data rows returned by the query.
        /// </summary>
        [JsonPropertyName("data")]
        public List<T> Data { get; set; } = new List<T>();

        /// <summary>
        /// Metadata about the query result.
        /// </summary>
        [JsonPropertyName("meta")]
        public Dictionary<string, object> Meta { get; set; } = new Dictionary<string, object>();
    }
}