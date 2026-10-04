namespace domain.Models.References;

public record AllowedFileType(int Id, string Extension, string MimeType, bool IsActive, string Comment);
