namespace infrastructure.Services.Messaging;

public class MessageDeserializer : IRabbitMqMessageDeserializer
{
    private readonly Dictionary<string, Type> _resolvedTypeCache;

    public MessageDeserializer()
    {
        var targetInterfaceType = typeof(IEvent);
        var targetAssembly = targetInterfaceType.Assembly;

        _resolvedTypeCache = targetAssembly
            .GetTypes()
            .Where(type =>
                type.IsClass &&
                !type.IsAbstract &&
                targetInterfaceType.IsAssignableFrom(type) &&
                !string.IsNullOrWhiteSpace(type.FullName))
            .ToDictionary(type => type.FullName!, type => type);
    }

    public object? Deserialize(string typeFullName, string jsonMessage)
    {
        var targetType = ResolveType(typeFullName);

        if (targetType == null)
        {
            throw new InvalidOperationException($"Type '{typeFullName}' was not found in the application domain or does not implement IEvent.");
        }

        return JsonSerializer.Deserialize(jsonMessage, targetType);
    }

    public Type? ResolveType(string typeFullName)
    {
        _resolvedTypeCache.TryGetValue(typeFullName, out var targetType);
        return targetType;
    }
}
