namespace BuildingBlocks.Caching;

public static class CacheKey
{
    public static string Basket(Guid userId)
        => $"basket:{userId}";

    public static string BasketItem(Guid id)
        => $"basket:item:{id}";

    public static string Tag(string tag)
        => $"tag:{tag}";
}