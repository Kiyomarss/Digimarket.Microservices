namespace Shared;

public static class ServiceProviderExtensions
{
    public static bool IsServiceRegistered<T>(this IServiceProvider services)
    {
        try
        {
            var s = services.GetService(typeof(T));
            return s != null;
        }
        catch
        {
            return false;
        }
    }
}
