// فایل: BuildingBlocks/Extensions/MediatRExtensions.cs

using System.Reflection;
using BuildingBlocks.Behaviors;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Extensions
{
    public static class MediatRExtensions
    {
        /// <summary>
        /// ثبت کامل MediatR + FluentValidation از لایه‌های Api و Application
        /// </summary>
        public static IServiceCollection AddConfiguredMediatR(this IServiceCollection services)
        {
            var apiAssembly = Assembly.GetExecutingAssembly();

            // پیدا کردن Application assembly با اولویت بالاتر
            var applicationAssembly = FindApplicationAssembly(apiAssembly)
                                      ?? throw new InvalidOperationException(
                                                                             "Application assembly not found. Expected an assembly containing 'Application' in name. " +
                                                                             "Referenced assemblies: " +
                                                                             string.Join(", ", apiAssembly.GetReferencedAssemblies().Select(a => a.Name)));

            var assemblies = new[]
            {
                apiAssembly, applicationAssembly
            }.Distinct().ToArray();

            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblies(assemblies);
                cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
                cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            });

            services.AddValidatorsFromAssembly(applicationAssembly);

            return services;
        }

        private static Assembly? FindApplicationAssembly(Assembly entryAssembly)
        {
            // اولویت ۱: از ارجاع‌ها
            var refName = entryAssembly.GetReferencedAssemblies()
                                       .FirstOrDefault(a => a.Name?.Contains("Application", StringComparison.OrdinalIgnoreCase) == true);

            if (refName != null)
                return Assembly.Load(refName);

            // اولویت ۲: از اسمبلی‌های لود شده
            return AppDomain.CurrentDomain.GetAssemblies()
                            .FirstOrDefault(a => a.GetName().Name?.Contains("Application", StringComparison.OrdinalIgnoreCase) == true);
        }
    }
}