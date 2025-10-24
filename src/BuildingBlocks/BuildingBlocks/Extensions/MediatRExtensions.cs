using System.Reflection;
using BuildingBlocks.Behaviors;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Extensions
{
    /// <summary>
    /// Extension methods for registering MediatR in microservices.
    /// Automatically discovers assemblies and adds pipeline behaviors.
    /// </summary>
    public static class MediatRExtensions
    {
        /// <summary>
        /// Registers MediatR handlers from all relevant assemblies.
        /// Also adds global pipeline behaviors like Validation and Logging.
        /// </summary>
        /// <param name="services">The IServiceCollection</param>
        /// <param name="markerType">A marker type from the Core layer of the service (e.g., CreateOrderHandler)</param>
        /// <param name="assemblyPrefix">Common assembly name prefix for discovery (e.g., Digimarket)</param>
        public static IServiceCollection AddConfiguredMediatR(
            this IServiceCollection services,
            Type markerType,
            string assemblyPrefix = "Digimarket")
        {
            // 1️⃣ Explicitly add the Core assembly (Basket.Core, Ordering.Core, etc.)
            var markerAssemblies = new[]
            {
                markerType.Assembly
            };

            // 2️⃣ Dynamically discover assemblies in the service's bin folder
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
            var discovered = Directory.GetFiles(path, "*.dll")
                                      .Select(Assembly.LoadFrom)
                                      .Where(a => a.GetName().Name!.StartsWith(assemblyPrefix))
                                      .ToArray();

            // 3️⃣ Merge both and register them all
            var allAssemblies = markerAssemblies.Concat(discovered).Distinct().ToArray();

            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblies(allAssemblies);
                cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
                cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            });

            return services;
        }
    }
}