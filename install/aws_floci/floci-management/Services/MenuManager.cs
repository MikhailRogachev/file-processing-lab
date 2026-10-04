namespace floci_management.Services;

public class MenuManager
{
    private readonly IList<AppManagerItem> _items = new List<AppManagerItem>
    {
        new AppManagerItem("1", "List all S3 buckets"),
        new AppManagerItem("2", "Create a new S3 bucket"),
        new AppManagerItem("4", "List all SQS Queues"),
        new AppManagerItem("5", "Upload File to S3 bucket"),
        new AppManagerItem("6", "Delete File from S3 bucket"),
        new AppManagerItem("Q", "Quit the application")
    };

    public IList<AppManagerItem> MainMenu => _items;

    public bool IsKeyValid(string key)
    {
        return _items.Any(p => p.Key == key);
    }
}
