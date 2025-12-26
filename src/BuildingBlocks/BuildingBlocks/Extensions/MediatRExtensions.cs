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
        /// به صورت جنرال و قابل استفاده در همه میکروسرویس‌ها و تست‌ها
        /// </summary>
        public static IServiceCollection AddConfiguredMediatR(this IServiceCollection services)
        {
            var entryAssembly = Assembly.GetExecutingAssembly();

            var apiAssembly = entryAssembly;
            var applicationAssembly = FindApplicationAssembly(entryAssembly)
                                      ?? throw new InvalidOperationException(
                                          "Application assembly not found. Make sure the entry assembly references the Application project.");

            var assemblies = new[] { apiAssembly, applicationAssembly }
                .Distinct()
                .ToArray();

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
            // لیست اسمبلی‌های احتمالی Application (از ارجاع‌ها)
            var referencedAssemblies = entryAssembly.GetReferencedAssemblies()
                .Where(a => a.Name?.Contains("Application", StringComparison.OrdinalIgnoreCase) == true)
                .ToList();

            // فیلتر کردن اسمبلی‌های تست (مثل *.IntegrationTests, *.UnitTests, *.Tests)
            var candidate = referencedAssemblies
                .FirstOrDefault(a => !a.Name?.EndsWith("Tests", StringComparison.OrdinalIgnoreCase) == true &&
                                     !a.Name?.EndsWith("IntegrationTests", StringComparison.OrdinalIgnoreCase) == true &&
                                     !a.Name?.Contains(".Tests.", StringComparison.OrdinalIgnoreCase) == true);

            if (candidate != null)
                return Assembly.Load(candidate);

            // فال‌بک: از اسمبلی‌های لود شده در AppDomain
            return AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a =>
                {
                    var name = a.GetName().Name;
                    return name?.Contains("Application", StringComparison.OrdinalIgnoreCase) == true &&
                           !name?.EndsWith("Tests", StringComparison.OrdinalIgnoreCase) == true &&
                           !name?.EndsWith("IntegrationTests", StringComparison.OrdinalIgnoreCase) == true &&
                           !name?.Contains(".Tests.", StringComparison.OrdinalIgnoreCase) == true;
                });
        }
    }
}