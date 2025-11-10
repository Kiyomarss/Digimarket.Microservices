using BuildingBlocks.Behaviors;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Extensions
{
    public static class MediatRExtensions
    {
        /// <summary>
        /// Registers MediatR handlers from one or multiple assemblies.
        /// Usage:
        /// services.AddConfiguredMediatR(typeof(AppMarker), typeof(DomainMarker));
        /// </summary>
        public static IServiceCollection AddConfiguredMediatR(
            this IServiceCollection services,
            params Type[] markerTypes)
        {
            if (markerTypes == null || markerTypes.Length == 0)
                throw new ArgumentException("You must pass at least one marker type to AddConfiguredMediatR.");

            // ✅ استخراج همه اسمبلی‌ها
            var assemblies = markerTypes
                             .Select(t => t.Assembly)
                             .Distinct()
                             .ToArray();

            services.AddMediatR(cfg =>
            {
                // ✅ ثبت همه اسمبلی‌ها
                cfg.RegisterServicesFromAssemblies(assemblies);

                // ✅ اضافه کردن pipeline behaviors
                cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
                cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            });

            return services;
        }
    }
}