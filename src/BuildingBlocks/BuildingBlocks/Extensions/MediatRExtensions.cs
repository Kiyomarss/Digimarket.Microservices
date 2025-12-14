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
        /// بدون نیاز به هیچ خط دستی در Startup
        /// </summary>
        public static IServiceCollection AddConfiguredMediatR(this IServiceCollection services)
        {
            var entryAssembly = Assembly.GetExecutingAssembly(); // Ordering.Api

            // 1. اسمبلی Api
            var apiAssembly = entryAssembly;

            // 2. پیدا کردن اسمبلی Application
            string? applicationAssemblyName = entryAssembly
                .GetReferencedAssemblies()
                .FirstOrDefault(a => a.Name?.Contains("Application", StringComparison.OrdinalIgnoreCase) == true)
                ?.Name;

            Assembly? applicationAssembly = null;

            if (!string.IsNullOrEmpty(applicationAssemblyName))
            {
                applicationAssembly = Assembly.Load(applicationAssemblyName);
            }

            if (applicationAssembly == null)
            {
                applicationAssembly = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name?.Contains("Application", StringComparison.OrdinalIgnoreCase) == true);
            }

            var assemblies = new List<Assembly> { apiAssembly };

            if (applicationAssembly != null && applicationAssembly != apiAssembly)
            {
                assemblies.Add(applicationAssembly);
            }

            // ثبت MediatR از همه اسمبلی‌های پیدا شده
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblies(assemblies.ToArray());

                cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
                cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            });

            // ثبت خودکار Validatorها از لایه Application (یا همه اسمبلی‌ها)
            if (applicationAssembly != null)
            {
                services.AddValidatorsFromAssembly(applicationAssembly);
            }
            else
            {
                // Fallback: از Api هم جستجو کن (اگر Validator در Api باشد)
                services.AddValidatorsFromAssembly(apiAssembly);
            }

            return services;
        }
    }
}