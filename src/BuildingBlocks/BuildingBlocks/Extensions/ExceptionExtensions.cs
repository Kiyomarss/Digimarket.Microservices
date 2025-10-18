using BuildingBlocks.Exceptions.Handler;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Extensions;

public static class ExceptionExtensions
{
    public static IServiceCollection AddGlobalExceptionHandler(this IServiceCollection services)
    {
        services.AddExceptionHandler<CustomExceptionHandler>();
        return services;
    }
}