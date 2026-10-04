namespace floci_management.Contracts;

public static class Settings
{
    public const string AWS_ENDPOINT_URL = "http://127.0.0.1:4566";
    public const string AWS_ACCESS_KEY_ID = "test";
    public const string AWS_SECRET_ACCESS_KEY = "test";
    public const string AWS_DEFAULT_REGION = "us-east-1";

    public const string BUCKET_NAME = "file-processing-bucket";
    public const string SQS_QUEUE_NAME = "s3-file-events-queue";

    public const string QuitKey = "Q";
}
