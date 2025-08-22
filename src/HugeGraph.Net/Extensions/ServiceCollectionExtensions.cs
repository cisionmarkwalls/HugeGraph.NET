using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HugeGraph.Net.Extensions
{
    /// <summary>
    /// Extension methods for configuring HugeGraph services in dependency injection.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds HugeGraph client services to the service collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configureOptions">Action to configure HugeGraph options.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddHugeGraph(this IServiceCollection services, Action<HugeGraphOptions> configureOptions)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (configureOptions == null) throw new ArgumentNullException(nameof(configureOptions));

            services.Configure(configureOptions);
            
            services.AddHttpClient<IHugeGraphClient, HugeGraphClient>((serviceProvider, httpClient) =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<HugeGraphOptions>>().Value;
                httpClient.BaseAddress = options.BaseAddress;
                httpClient.Timeout = options.RequestTimeout;
            });

            return services;
        }

        /// <summary>
        /// Adds HugeGraph client services to the service collection with pre-configured options.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="options">Pre-configured HugeGraph options.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddHugeGraph(this IServiceCollection services, HugeGraphOptions options)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (options == null) throw new ArgumentNullException(nameof(options));

            return services.AddHugeGraph(opt =>
            {
                opt.BaseAddress = options.BaseAddress;
                opt.GraphName = options.GraphName;
                opt.Username = options.Username;
                opt.Password = options.Password;
                opt.BearerToken = options.BearerToken;
                opt.RequestTimeout = options.RequestTimeout;
                opt.AdditionalHeaders = options.AdditionalHeaders;
            });
        }
    }
}