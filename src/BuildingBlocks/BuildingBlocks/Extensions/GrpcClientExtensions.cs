using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Extensions
{
    /// <summary>
    /// Extension methods for registering gRPC clients in a consistent way across microservices.
    /// </summary>
    public static class GrpcClientExtensions
    {
        /// <summary>
        /// Adds a gRPC client with standard SSL bypass and configuration binding.
        /// </summary>
        /// <typeparam name="TClient">The generated gRPC client type (e.g. OrderProtoService.OrderProtoServiceClient)</typeparam>
        /// <param name="services">The IServiceCollection</param>
        /// <param name="configuration">The IConfiguration instance</param>
        /// <param name="configKey">The key in configuration (e.g. "GrpcSettings:OrderUrl")</param>
        /// <returns></returns>
        public static IServiceCollection AddGrpcClientWithConfig<TClient>(
            this IServiceCollection services,
            IConfiguration configuration,
            string configKey)
            where TClient : class
        {
            var url = configuration[configKey];

            if (string.IsNullOrWhiteSpace(url))
                throw new InvalidOperationException($"Missing configuration for gRPC client URL at '{configKey}'");

            services.AddGrpcClient<TClient>(options => { options.Address = new Uri(url); })
                    .ConfigurePrimaryHttpMessageHandler(() =>
                    {
                        // 🔐 Allows development certificates (self-signed)
                        var handler = new HttpClientHandler
                        {
                            ServerCertificateCustomValidationCallback =
                                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                        };

                        return handler;
                    });

            return services;
        }
    }
}
