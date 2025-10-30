namespace BuildingBlocks.Common.Extensions;

public static class GuidExtensions
{
    /// <summary>
    /// Converts a collection of string IDs to a collection of valid Guids.
    /// Invalid Guids are ignored.
    /// </summary>
    public static IEnumerable<Guid> ToValidGuids(this IEnumerable<string>? ids)
    {
        if (ids is null)
            return [];

        return ids
               .Select(id => Guid.TryParse(id, out var guid) ? guid : Guid.Empty)
               .Where(g => g != Guid.Empty);
    }
}