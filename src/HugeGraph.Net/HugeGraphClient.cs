using HugeGraph.Net.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Web;

namespace HugeGraph.Net
{
    /// <summary>
    /// A client for executing Cypher queries against a HugeGraph server.
    /// </summary>
    public class HugeGraphClient : IHugeGraphClient, IDisposable
    {
        private static readonly ActivitySource ActivitySource = new("HugeGraph.Net");
        
        private readonly HttpClient _httpClient;
        private readonly HugeGraphOptions _options;
        private readonly ILogger<IHugeGraphClient>? _logger;
        private readonly bool _ownsHttpClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="HugeGraphClient"/> class.
        /// </summary>
        /// <param name="httpClient">The HTTP client to use for requests.</param>
        /// <param name="options">Configuration options.</param>
        /// <param name="logger">Optional logger for debugging and error reporting.</param>
        public HugeGraphClient(HttpClient httpClient, IOptions<HugeGraphOptions> options, ILogger<IHugeGraphClient>? logger = null)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger;
            _ownsHttpClient = false;

            ConfigureHttpClient();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HugeGraphClient"/> class.
        /// </summary>
        /// <param name="options">Configuration options.</param>
        /// <param name="logger">Optional logger for debugging and error reporting.</param>
        public HugeGraphClient(HugeGraphOptions options, ILogger<IHugeGraphClient>? logger = null)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _logger = logger;
            _httpClient = new HttpClient();
            _ownsHttpClient = true;

            ConfigureHttpClient();
        }

        /// <inheritdoc />
        public async Task<CypherResponse<TResult>> ExecuteAsync<TResult>(string graph, string cypher, CancellationToken cancellationToken = default)
        {
            using var activity = ActivitySource.StartActivity("HugeGraph.ExecuteCypher");
            
            var graphName = !string.IsNullOrEmpty(graph) ? graph : _options.GraphName;
            if (string.IsNullOrEmpty(graphName))
            {
                throw new ArgumentException("Graph name must be provided either in the method call or in options.", nameof(graph));
            }

            activity?.SetTag("http.method", "GET/POST");
            activity?.SetTag("db.statement", cypher);
            
            _logger?.LogDebug("Executing Cypher query against graph '{Graph}': {Query}", graphName, cypher);

            try
            {
                var responseJson = await ExecuteCypherQuery(graphName, cypher, cancellationToken);
                var response = JsonSerializer.Deserialize<CypherResponse<TResult>>(responseJson);
                
                if (response == null)
                {
                    throw new HugeGraphException("Failed to deserialize response from HugeGraph API");
                }

                return response;
            }
            catch (HttpRequestException ex)
            {
                _logger?.LogError(ex, "HTTP error executing Cypher query against graph '{Graph}'", graphName);
                throw new HugeGraphException($"HTTP error executing Cypher query: {ex.Message}", ex);
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                _logger?.LogError(ex, "Timeout executing Cypher query against graph '{Graph}'", graphName);
                throw new HugeGraphException($"Timeout executing Cypher query: {ex.Message}", ex);
            }
            catch (JsonException ex)
            {
                _logger?.LogError(ex, "Failed to deserialize response from HugeGraph API");
                throw new HugeGraphException($"Failed to deserialize API response: {ex.Message}", ex);
            }
        }

        /// <inheritdoc />
        public async Task<CypherResponse<dynamic>> ExecuteAsync(string graph, string cypher, CancellationToken cancellationToken = default)
        {
            return await ExecuteAsync<dynamic>(graph, cypher, cancellationToken);
        }

        private void ConfigureHttpClient()
        {
            _httpClient.BaseAddress = _options.BaseAddress;
            _httpClient.Timeout = _options.RequestTimeout;

            // Add authentication headers
            if (!string.IsNullOrEmpty(_options.BearerToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.BearerToken);
            }
            else if (!string.IsNullOrEmpty(_options.Username) && !string.IsNullOrEmpty(_options.Password))
            {
                var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_options.Username}:{_options.Password}"));
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
            }

            // Add additional headers
            foreach (var header in _options.AdditionalHeaders)
            {
                _httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
        }

        private async Task<string> ExecuteCypherQuery(string graph, string cypher, CancellationToken cancellationToken)
        {
            // Determine whether to use GET or POST based on query length
            var usePost = cypher.Length > 2000; // Reasonable URL length limit
            
            HttpResponseMessage response;
            
            if (usePost)
            {
                var uri = $"/graphs/{Uri.EscapeDataString(graph)}/cypher";
                var content = new StringContent(cypher, Encoding.UTF8, "application/json");
                response = await _httpClient.PostAsync(uri, content, cancellationToken);
            }
            else
            {
                var encodedCypher = HttpUtility.UrlEncode(cypher);
                var uri = $"/graphs/{Uri.EscapeDataString(graph)}/cypher?cypher={encodedCypher}";
                response = await _httpClient.GetAsync(uri, cancellationToken);
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new HugeGraphException(
                    $"HugeGraph API returned {response.StatusCode}: {errorContent}",
                    (int)response.StatusCode);
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            return responseContent;
        }

        /// <summary>
        /// Disposes the client and its resources.
        /// </summary>
        public void Dispose()
        {
            if (_ownsHttpClient)
            {
                _httpClient?.Dispose();
            }
        }
    }
}