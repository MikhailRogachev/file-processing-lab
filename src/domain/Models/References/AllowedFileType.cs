namespace domain.Models.References;

public class AllowedFileType
{
    public int Id { get; set; }
    public string Extension { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public string Chain { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
}