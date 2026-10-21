namespace aws_agent.Contracts.Constants;

public static class FieldNames
{
    public const string ROOT_ELEMENT_NAME = "Records";
    public const string BUCKET_ELEMENT_NAME = "bucket";
    public const string S3_ELEMENT_NAME = "s3";
    public const string OBJECT_ELEMENT_NAME = "object";
    public const string EVENTNAME_FIELD_NAME = "eventName";
    public const string NAME_FIELD_NAME = "name";
    public const string KEY_FIELD_NAME = "key";
    public const string SIZE_FIELD_NAME = "size";
    public const string ETAG_FIELD_NAME = "eTag";

    public const string AWS_META_IDENTIFIER_FIELD = "x-amz-meta-identifier";
    public const string AWS_META_PRIORITY_FIELD = "x-amz-meta-priority";

    public const string UpdatedEventName = "Updated";
    public const string DeletedEventName = "Deleted";
}


