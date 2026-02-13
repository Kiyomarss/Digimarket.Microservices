namespace Ordering.Api.Contracts;

public static class ApiEndpoints
{
    public static class Orders
    {
        public const string Base = "/Order";

        public const string GetCurrentUserOrders =
            $"{Base}/current";
    }
}
