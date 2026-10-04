namespace domain.Extensions;

public static class FileNameExtensions
{
    public static string CreateAssetIdentificator(this string fileName)
    {
        var index = fileName.LastIndexOf('.');
        return fileName.Substring(0, index);
    }
}
