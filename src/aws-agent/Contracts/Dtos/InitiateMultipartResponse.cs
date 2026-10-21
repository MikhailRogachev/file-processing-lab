namespace aws_agent.Contracts.Dtos;

public record InitiateMultipartResponse(string ObjectKey, string UploadId, List<PartUrlDto> PresignedUrls, int TotalParts);
