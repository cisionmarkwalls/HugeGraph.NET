namespace HugeGraph.Net.Tests
{
    public class HugeGraphExceptionTests
    {
        [Fact]
        public void Constructor_WithMessage_SetsMessage()
        {
            // Arrange
            var message = "Test error message";

            // Act
            var exception = new HugeGraphException(message);

            // Assert
            Assert.Equal(message, exception.Message);
            Assert.Null(exception.StatusCode);
            Assert.Null(exception.HugeGraphStatusCode);
        }

        [Fact]
        public void Constructor_WithMessageAndInnerException_SetsProperties()
        {
            // Arrange
            var message = "Test error message";
            var innerException = new InvalidOperationException("Inner error");

            // Act
            var exception = new HugeGraphException(message, innerException);

            // Assert
            Assert.Equal(message, exception.Message);
            Assert.Equal(innerException, exception.InnerException);
            Assert.Null(exception.StatusCode);
            Assert.Null(exception.HugeGraphStatusCode);
        }

        [Fact]
        public void Constructor_WithMessageAndStatusCode_SetsProperties()
        {
            // Arrange
            var message = "Test error message";
            var statusCode = 500;

            // Act
            var exception = new HugeGraphException(message, statusCode);

            // Assert
            Assert.Equal(message, exception.Message);
            Assert.Equal(statusCode, exception.StatusCode);
            Assert.Null(exception.HugeGraphStatusCode);
        }

        [Fact]
        public void Constructor_WithAllParameters_SetsAllProperties()
        {
            // Arrange
            var message = "Test error message";
            var statusCode = 500;
            var hugeGraphStatusCode = 40001;

            // Act
            var exception = new HugeGraphException(message, statusCode, hugeGraphStatusCode);

            // Assert
            Assert.Equal(message, exception.Message);
            Assert.Equal(statusCode, exception.StatusCode);
            Assert.Equal(hugeGraphStatusCode, exception.HugeGraphStatusCode);
        }
    }
}