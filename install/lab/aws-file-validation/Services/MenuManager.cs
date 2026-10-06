namespace aws_file_validation.Services;

public class MenuManager
{
    private readonly IList<AppManagerItem> _items = new List<AppManagerItem>
    {
        new AppManagerItem("1", "List all file"),
        new AppManagerItem("2", "Get FileExtension"),
        new AppManagerItem("3", "Get file header"),
        new AppManagerItem("Q", "Quit the application")
    };

    public IList<AppManagerItem> MainMenu => _items;

    public bool IsKeyValid(string key)
    {
        return _items.Any(p => p.Key == key);
    }
}


public record AppManagerItem(string Key, string MenuDescription);

public static class Settings
{
    public const string QuitKey = "Q";
}
