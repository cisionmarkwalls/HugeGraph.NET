using System.Text.Json.Serialization;

namespace HugeGraph.Net.Models
{
    /// <summary>
    /// Represents the status portion of a HugeGraph Cypher API response.
    /// </summary>
    public class Status
    {
        /// <summary>
        /// The status code of the response.
        /// </summary>
        [JsonPropertyName("code")]
        public int Code { get; set; }

        /// <summary>
        /// The status message.
        /// </summary>
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Optional attributes providing additional status information.
        /// </summary>
        [JsonPropertyName("attributes")]
        public Dictionary<string, object>? Attributes { get; set; }
    }
}