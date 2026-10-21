namespace domain.Dtos;

public record class AssetCreateContext(string identifier, string FullFileName, long Size, int Priority, string etag);
