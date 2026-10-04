namespace domain.Extensions;

public static class CollectionExtensions
{
    public static bool IsAny<T>(this T collection) where T : IEnumerable<object>
    {
        return collection != null && collection.Any();
    }
}
