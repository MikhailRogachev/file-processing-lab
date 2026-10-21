namespace aws_agent.Extensions;

public static class AwsMessageExtension
{

    public static MessageConsumed ConvertToMessageConsumed(this Message message)
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
        var ingestContext = GetFileIngestContext(key);
        var size = obj.GetElementLongValue(FieldNames.SIZE_FIELD_NAME);
        var etag = obj.GetElementStringValue(FieldNames.ETAG_FIELD_NAME);

        return new MessageConsumed(
            message.MessageId,
            message.ReceiptHandle,
            eventName,
            bucketName,
            ingestContext.FileName,
            size,
            ingestContext.Priority,
            ingestContext.PackageIdentifier,
            etag);
    }


    public static JsonElement GetRootElement(this JsonDocument document)
    {
        var rootElement = document.RootElement;

        if (!rootElement.TryGetProperty(FieldNames.ROOT_ELEMENT_NAME, out JsonElement records))
        {
            throw new InvalidCastException($"The Message is received doesn't contain {FieldNames.ROOT_ELEMENT_NAME} element");
        }

        return records[0];
    }


    public static JsonElement GetElement(this JsonElement jsonSource, string elementName)
    {
        if (!jsonSource.TryGetProperty(elementName, out JsonElement value))
        {
            throw new InvalidCastException($"The Message is received doesn't contain {elementName} element");
        }

        return value;
    }


    public static string GetEventName(this JsonElement jsonSource)
    {
        if (jsonSource.TryGetProperty(FieldNames.EVENTNAME_FIELD_NAME, out JsonElement value))
        {
            switch (value.ToString())
            {
                case "ObjectCreated:Put":
                    return FieldNames.UpdatedEventName;
                default:
                    return "Unknown";
            }
        }

        return "Unknown";
    }


    public static string GetElementStringValue(this JsonElement jsonSource, string fieldName)
    {
        var value = jsonSource.GetElement(fieldName);
        return value.ToString();
    }


    public static long GetElementLongValue(this JsonElement jsonSource, string fieldName)
    {
        var value = jsonSource.GetElement(fieldName);
        return value.GetInt64();
    }

    public static FileIngestContext GetFileIngestContext(string src)
    {
        return JsonSerializer.Deserialize<FileIngestContext>(src)!;
    }
}
