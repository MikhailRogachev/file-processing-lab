namespace floci_management.Contracts;

public static class ConnectionSettings
{
    public const string LOCAL_AWS_ENDPOINT_URL = "http://127.0.0.1:4566";
    public const string LOCAL_AWS_ACCESS_KEY_ID = "test";
    public const string LOCAL_AWS_SECRET_ACCESS_KEY = "test";
    public const string LOCAL_AWS_DEFAULT_REGION = "us-east-1";

    public const string LOCAL_BUCKET_NAME = "file-processing-bucket";
    public const string LOCAL_FOLDER_NAME = "";
    public const string LOCAL_SQS_QUEUE_NAME = "s3-file-events-queue";

    public const string REMOTE_PROFILE_FILENAME = @"C:\Users\mikrog\.aws\appcredentials";
    public const string REMOTE_BUCKET_NAME = "dev-makalu-dev-playoutassets";
    public const string REMOTE_FOLDER_NAME = "Clips/Test-Example/Rpgachev/";
}
