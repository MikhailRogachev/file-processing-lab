namespace aws_agent.Extensions;

public static class AwsMessageExtension
{
    /// <summary>
    /// Deserializes and maps an SQS <see cref="Message"/> payload containing an AWS S3 event notification 
    /// into a strongly typed <see cref="QueueMessageConsumed"/> domain model.
    /// </summary>
    /// <param name="message">The incoming AWS SQS <see cref="Message"/> containing the raw S3 event JSON string in its body.</param>
    /// <returns>
    /// A populated <see cref="QueueMessageConsumed"/> instance containing extracted message metadata, S3 bucket details, 
    /// object key, object size, and normalized event type.
    /// </returns>
    /// <exception cref="JsonException">
    /// Thrown when the <see cref="Message.Body"/> string is not a valid JSON document.
    /// </exception>
    /// <exception cref="InvalidCastException">
    /// Thrown when any mandatory S3 event structural property (e.g., records array, bucket node, or object node) 
    /// is missing from the JSON payload.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the file size property is present but is not formatted as a valid numeric JSON token.
    /// </exception>
    public static QueueMessageConsumed ConvertToMessageConsumed(this Message message)
    {
        using JsonDocument doc = JsonDocument.Parse(message.Body);
        var record = doc.GetRootElement();
        var s3 = record.GetElement(FieldNames.S3_ELEMENT_NAME);
        var obj = s3.GetElement(FieldNames.OBJECT_ELEMENT_NAME);
        var bucket = s3.GetElement(FieldNames.BUCKET_ELEMENT_NAME);

        // get values
        var eventName = record.GetEventName();
        var bucketName = bucket.GetElementStringValue(FieldNames.NAME_FIELD_NAME);
        var key = obj.GetElementStringValue(FieldNames.KEY_FIELD_NAME);
        var size = obj.GetElementLongValue(FieldNames.SIZE_FIELD_NAME);

        return new QueueMessageConsumed(
            message.MessageId,
            message.ReceiptHandle,
            eventName,
            bucketName,
            key,
            size);
    }

    /// <summary>
    /// Extracts the primary record element from a JSON document payload.
    /// </summary>
    /// <param name="document">The <see cref="JsonDocument"/> instance containing the event payload.</param>
    /// <returns>
    /// The first <see cref="JsonElement"/> located within the array property defined by <see cref="FieldNames.ROOT_ELEMENT_NAME"/>.
    /// </returns>
    /// <exception cref="InvalidCastException">
    /// Thrown when the specified <see cref="FieldNames.ROOT_ELEMENT_NAME"/> property is missing from the JSON document's root structure.
    /// </exception>
    /// <exception cref="IndexOutOfRangeException">
    /// Thrown when the root property array exists but contains no elements.
    /// </exception>
    public static JsonElement GetRootElement(this JsonDocument document)
    {
        var rootElement = document.RootElement;

        if (!rootElement.TryGetProperty(FieldNames.ROOT_ELEMENT_NAME, out JsonElement records))
        {
            throw new InvalidCastException($"The Message is received doesn't contain {FieldNames.ROOT_ELEMENT_NAME} element");
        }

        return records[0];
    }

    /// <summary>
    /// Retrieves a required child <see cref="JsonElement"/> property from the target JSON element.
    /// </summary>
    /// <param name="jsonSource">The source <see cref="JsonElement"/> instance to inspect.</param>
    /// <param name="elementName">The case-sensitive name of the JSON property to extract.</param>
    /// <returns>
    /// The <see cref="JsonElement"/> representing the property value associated with <paramref name="elementName"/>.
    /// </returns>
    /// <exception cref="InvalidCastException">
    /// Thrown when the specified <paramref name="elementName"/> property does not exist within <paramref name="jsonSource"/>.
    /// </exception>
    public static JsonElement GetElement(this JsonElement jsonSource, string elementName)
    {
        if (!jsonSource.TryGetProperty(elementName, out JsonElement value))
        {
            throw new InvalidCastException($"The Message is received doesn't contain {elementName} element");
        }

        return value;
    }

    /// <summary>
    /// Extracts and translates the event name from an S3 record JSON element into a standardized domain event status string.
    /// </summary>
    /// <param name="jsonSource">The <see cref="JsonElement"/> representing an individual record payload.</param>
    /// <returns>
    /// A <see cref="string"/> representing the mapped event status:
    /// <list type="bullet">
    ///   <item><description><c>"Updated"</c> if the event is <c>"ObjectCreated:Put"</c>.</description></item>
    ///   <item><description><c>"Unknown"</c> if the event property is unmapped, unrecognized, or missing.</description></item>
    /// </list>
    /// </returns>
    public static string GetEventName(this JsonElement jsonSource)
    {
        if (jsonSource.TryGetProperty(FieldNames.EVENTNAME_FIELD_NAME, out JsonElement value))
        {
            switch (value.ToString())
            {
                case "ObjectCreated:Put":
                    return "Updated";
                default:
                    return "Unknown";
            }
        }

        return "Unknown";
    }

    /// <summary>
    /// Extracts the string representation of a specified child property from a <see cref="JsonElement"/>.
    /// </summary>
    /// <param name="jsonSource">The target <see cref="JsonElement"/> instance to inspect.</param>
    /// <param name="fieldName">The case-sensitive name of the property to extract.</param>
    /// <returns>
    /// A <see cref="string"/> containing the raw value of the specified property.
    /// </returns>
    /// <exception cref="InvalidCastException">
    /// Thrown when the specified <paramref name="fieldName"/> property is missing from <paramref name="jsonSource"/>.
    /// </exception>
    public static string GetElementStringValue(this JsonElement jsonSource, string fieldName)
    {
        var value = jsonSource.GetElement(fieldName);
        return value.ToString();
    }

    /// <summary>
    /// Extracts a required child property from a <see cref="JsonElement"/> and converts its value to a 64-bit signed integer.
    /// </summary>
    /// <param name="jsonSource">The target <see cref="JsonElement"/> instance to inspect.</param>
    /// <param name="fieldName">The case-sensitive name of the property to extract.</param>
    /// <returns>
    /// A <see cref="long"/> representing the numeric value of the specified property.
    /// </returns>
    /// <exception cref="InvalidCastException">
    /// Thrown by <c>GetElement</c> when the specified <paramref name="fieldName"/> property is missing from <paramref name="jsonSource"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the target JSON token type is not a number.
    /// </exception>
    /// <exception cref="FormatException">
    /// Thrown when the property value cannot be represented as a 64-bit signed integer.
    /// </exception>
    public static long GetElementLongValue(this JsonElement jsonSource, string fieldName)
    {
        var value = jsonSource.GetElement(fieldName);
        return value.GetInt64();
    }
}
