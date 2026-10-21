namespace domain.Dtos;

public record class AssetCreateContext(string identifier, string FileName, long Size, int Priority, string etag);
