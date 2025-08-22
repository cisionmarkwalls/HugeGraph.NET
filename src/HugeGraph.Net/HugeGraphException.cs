namespace HugeGraph.Net
{
    /// <summary>
    /// Exception thrown when HugeGraph API operations fail.
    /// </summary>
    public class HugeGraphException : Exception
    {
        /// <summary>
        /// The HTTP status code associated with the error, if available.
        /// </summary>
        public int? StatusCode { get; }

        /// <summary>
        /// The HugeGraph status code from the API response, if available.
        /// </summary>
        public int? HugeGraphStatusCode { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HugeGraphException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        public HugeGraphException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HugeGraphException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public HugeGraphException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HugeGraphException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="statusCode">The HTTP status code.</param>
        public HugeGraphException(string message, int statusCode) : base(message)
        {
            StatusCode = statusCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HugeGraphException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="statusCode">The HTTP status code.</param>
        /// <param name="hugeGraphStatusCode">The HugeGraph status code.</param>
        public HugeGraphException(string message, int statusCode, int hugeGraphStatusCode) : base(message)
        {
            StatusCode = statusCode;
            HugeGraphStatusCode = hugeGraphStatusCode;
        }
    }
}