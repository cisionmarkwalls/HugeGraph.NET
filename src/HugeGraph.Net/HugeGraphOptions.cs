namespace HugeGraph.Net
{
    /// <summary>
    /// Configuration options for the HugeGraph client.
    /// </summary>
    public class HugeGraphOptions
    {
        /// <summary>
        /// The base address of the HugeGraph server.
        /// </summary>
        public Uri BaseAddress { get; set; } = new Uri("http://localhost:8080");

        /// <summary>
        /// The default graph name to use for queries.
        /// </summary>
        public string GraphName { get; set; } = string.Empty;

        /// <summary>
        /// Username for basic authentication.
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// Password for basic authentication.
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// Bearer token for authentication.
        /// </summary>
        public string? BearerToken { get; set; }

        /// <summary>
        /// Request timeout for HTTP operations.
        /// </summary>
        public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Additional headers to include in requests.
        /// </summary>
        public Dictionary<string, string> AdditionalHeaders { get; set; } = new Dictionary<string, string>();
    }
}