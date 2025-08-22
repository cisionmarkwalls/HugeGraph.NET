using System.Threading;
using System.Threading.Tasks;

namespace HugeGraph.Net
{
    /// <summary>
    /// Defines the operations for executing Cypher queries against a HugeGraph server.
    /// Implementations of this interface should be thread‑safe and suitable for dependency injection.
    /// </summary>
    public interface IHugeGraphClient
    {
        /// <summary>
        /// Executes a Cypher query against the specified graph and deserializes each row into <typeparamref name="TResult"/>.
        /// </summary>
        /// <typeparam name="TResult">The type to deserialize each result row to.</typeparam>
        /// <param name="graph">The name of the graph.</param>
        /// <param name="cypher">The Cypher statement to execute.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that produces a <see cref="Models.CypherResponse{TResult}"/>.</returns>
        Task<Models.CypherResponse<TResult>> ExecuteAsync<TResult>(string graph, string cypher, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes a Cypher query against the specified graph and returns a dynamic result.
        /// </summary>
        /// <param name="graph">The name of the graph.</param>
        /// <param name="cypher">The Cypher statement to execute.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that produces a <see cref="Models.CypherResponse{System.Object}"/> with dynamic result rows.</returns>
        Task<Models.CypherResponse<dynamic>> ExecuteAsync(string graph, string cypher, CancellationToken cancellationToken = default);
    }
}
