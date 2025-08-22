using HugeGraph.Net.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HugeGraph.Net.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddHugeGraph_WithAction_RegistersServices()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddHugeGraph(options =>
            {
                options.BaseAddress = new Uri("http://test:8080");
                options.GraphName = "testgraph";
            });

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var client = serviceProvider.GetService<IHugeGraphClient>();
            var options = serviceProvider.GetService<IOptions<HugeGraphOptions>>();

            Assert.NotNull(client);
            Assert.NotNull(options);
            Assert.Equal("http://test:8080/", options.Value.BaseAddress.ToString());
            Assert.Equal("testgraph", options.Value.GraphName);
        }

        [Fact]
        public void AddHugeGraph_WithPreConfiguredOptions_RegistersServices()
        {
            // Arrange
            var services = new ServiceCollection();
            var hugeGraphOptions = new HugeGraphOptions
            {
                BaseAddress = new Uri("http://test:8080"),
                GraphName = "testgraph",
                Username = "user",
                Password = "pass"
            };

            // Act
            services.AddHugeGraph(hugeGraphOptions);

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var client = serviceProvider.GetService<IHugeGraphClient>();
            var options = serviceProvider.GetService<IOptions<HugeGraphOptions>>();

            Assert.NotNull(client);
            Assert.NotNull(options);
            Assert.Equal("http://test:8080/", options.Value.BaseAddress.ToString());
            Assert.Equal("testgraph", options.Value.GraphName);
            Assert.Equal("user", options.Value.Username);
            Assert.Equal("pass", options.Value.Password);
        }

        [Fact]
        public void AddHugeGraph_WithNullServices_ThrowsArgumentNullException()
        {
            // Arrange
            ServiceCollection services = null!;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                services.AddHugeGraph(options => { }));
        }

        [Fact]
        public void AddHugeGraph_WithNullConfigureOptions_ThrowsArgumentNullException()
        {
            // Arrange
            var services = new ServiceCollection();
            Action<HugeGraphOptions> configureOptions = null!;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                services.AddHugeGraph(configureOptions));
        }

        [Fact]
        public void AddHugeGraph_WithNullOptions_ThrowsArgumentNullException()
        {
            // Arrange
            var services = new ServiceCollection();
            HugeGraphOptions options = null!;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                services.AddHugeGraph(options));
        }
    }
}