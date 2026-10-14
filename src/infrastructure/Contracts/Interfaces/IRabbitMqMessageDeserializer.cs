namespace infrastructure.Contracts.Interfaces;

public interface IRabbitMqMessageDeserializer
{
    /// <summary>
    /// Deserializes a JSON string into an object based on the provided class full name.
    /// </summary>
    object? Deserialize(string typeFullName, string jsonMessage);

    /// <summary>
    /// Resolves the C# Type associated with the full name string.
    /// </summary>
    Type? ResolveType(string typeFullName);
}
