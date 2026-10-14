using System.Text;

namespace floci_management.Services;

public class MenuManager
{
    private readonly IList<AppManagerItem> _items = new List<AppManagerItem>
    {
        new AppManagerItem("1", "List all S3 buckets"),
        new AppManagerItem("2", "Get the list of files in the bucket."),
        new AppManagerItem("3", "Upload File to S3 bucket"),
        new AppManagerItem("5", "Validate File"),
        new AppManagerItem("Q", "Quit the application")
    };

    private string _samplesRootDirectory = string.Empty;

    public IList<AppManagerItem> MainMenu => _items;

    public bool IsKeyValid(string key)
    {
        return _items.Any(p => p.Key == key);
    }

    public string FileCollectionMenu()
    {
        var bld = new StringBuilder();
        bld.AppendLine("Files Collection to select:");
        bld.AppendLine("sample001.mp4");
        bld.AppendLine("sample003.de.stl");
        bld.AppendLine("sample004.fr.srt");
        bld.AppendLine("sample005.mxf");
        return bld.ToString();
    }

    public string GetSamplesRootDirectory()
    {
        if (!string.IsNullOrWhiteSpace(_samplesRootDirectory))
            return _samplesRootDirectory;

        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        _samplesRootDirectory = GetParent(directory);
        return _samplesRootDirectory;
    }




    public IList<Sample> GetTestDataFilesMenu()
    {

        return new List<Sample>();



    }

    private string GetParent(DirectoryInfo di)
    {
        if (di == null || di.Parent == null)
            throw new DirectoryNotFoundException();

        var value = di.GetDirectories().FirstOrDefault(p => p.Name == KeySettings.SAMPLE_ROOT_DIRECTORY_NAME);
        if (value != null)
            return value.FullName;

        return GetParent(di.Parent);
    }
}
