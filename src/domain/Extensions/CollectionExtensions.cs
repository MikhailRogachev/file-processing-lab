namespace domain.Extensions;

public static class CollectionExtensions
{
    public static bool IsAny<T>(this T collection) where T : IEnumerable<object>
    {
        return collection != null && collection.Any();
    }

    public static int IndexOfSequence(this byte[] buffer, byte[] pattern)
    {
        int limit = buffer.Length - pattern.Length;
        for (int i = 0; i <= limit; i++)
        {
            bool match = true;
            for (int j = 0; j < pattern.Length; j++)
            {
                if (buffer[i + j] != pattern[j])
                {
                    match = false;
                    break;
                }
            }
            if (match) return i;
        }
        return -1;
    }
}
