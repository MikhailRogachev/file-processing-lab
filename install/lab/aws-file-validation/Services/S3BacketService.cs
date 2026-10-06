using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.S3;
using Amazon.S3.Model;

namespace aws_file_validation.Services;

public class S3BacketService
{
    private readonly string my_credentials = @"C:\Users\mikrog\.aws\appcredentials";
    private readonly string backetname = "dev-makalu-dev-playoutassets";
    private readonly string folder = "Clips/Test-Example/Rpgachev/";

    private readonly int headerByteToRead = 64;

    // Magic Byte Signatures
    private static readonly byte[] Mp4FtypMagic = "ftyp"u8.ToArray();
    private static readonly byte[] EbmlMagic = [0x1A, 0x45, 0xDF, 0xA3]; // MKV/WebM
    private static readonly byte[] Id3Magic = "ID3"u8.ToArray(); // MP3 ID3v2
    private static readonly byte[] WebVttMagic = "WEBVTT"u8.ToArray();
    private static readonly byte[] mxfHeaderMagic = [0x06, 0x0E, 0x2B, 0x34, 0x02, 0x05, 0x01, 0x01, 0x0D, 0x01, 0x02, 0x01, 0x01, 0x02, 0x10, 0x00];

    private AWSCredentials GetCredentials()
    {
        var profile = new CredentialProfileStoreChain(my_credentials);

        if (!profile.TryGetAWSCredentials("default", out AWSCredentials credentials))
        {
            throw new AmazonAccountIdException("Wrong credentials");
        }

        return credentials;
    }

    public async Task<List<FileItem>> GetFilesCollectionAsync(CancellationToken cancellationToken)
    {
        var list = new List<FileItem>();
        var index = 1;
        using var client = new AmazonS3Client(GetCredentials(), RegionEndpoint.EUCentral1);

        var request = new ListObjectsV2Request
        {
            BucketName = backetname,
            Prefix = folder
        };

        var response = await client.ListObjectsV2Async(request, cancellationToken);

        foreach (S3Object obj in response.S3Objects)
        {
            list.Add(new FileItem(index, obj.Key, obj.Size));
            index++;
        }

        return list;
    }

    public string GetFileExtension(string filename)
    {
        return Path.GetExtension(filename);
    }


    public async Task<string> GetFileHeaderAsync(string filename, CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(filename);
        var request = new GetObjectRequest
        {
            BucketName = backetname,
            Key = filename,
            ByteRange = new ByteRange(0, headerByteToRead - 1)
        };

        using var client = new AmazonS3Client(GetCredentials(), RegionEndpoint.EUCentral1);
        using var response = await client.GetObjectAsync(request, cancellationToken);
        using var ms = new MemoryStream();
        await response.ResponseStream.CopyToAsync(ms, cancellationToken);

        var (isMagicValid, detectedMime) = ValidateMagicBytes(extension, ms.ToArray());

        if (!isMagicValid)
        {
            return $"Invalid Mime for the file {filename}";
        }

        return $"etected Mime {detectedMime} for the file: {filename}";
    }

    private static (bool IsValid, string DetectedMime) ValidateMagicBytes(string extension, byte[] header)
    {
        // MP4 check: contains 'ftyp' starting at byte offset 4
        if (extension is ".mp4" or ".mov")
        {
            var isMp4 = header.Length >= 12 && header.AsSpan(4, 4).SequenceEqual(Mp4FtypMagic);
            return (isMp4, isMp4 ? "video/mp4" : "application/octet-stream");
        }

        // MKV / WebM EBML check
        if (extension is ".mkv" or ".webm")
        {
            var isEbml = header.Length >= 4 && header.AsSpan(0, 4).SequenceEqual(EbmlMagic);
            return (isEbml, isEbml ? "video/x-matroska" : "application/octet-stream");
        }

        // MP3 check (ID3v2 header tag)
        if (extension is ".mp3")
        {
            var isMp3 = header.Length >= 3 && header.AsSpan(0, 3).SequenceEqual(Id3Magic);
            return (isMp3, isMp3 ? "audio/mpeg" : "application/octet-stream");
        }

        // WebVTT check
        if (extension is ".vtt")
        {
            var isVtt = header.Length >= 6 && header.AsSpan(0, 6).SequenceEqual(WebVttMagic);
            return (isVtt, isVtt ? "text/vtt" : "text/plain");
        }

        // Fallback for custom allowed types
        return (true, "application/octet-stream");
    }

}

public record FileItem(int index, string filename, long? filesize);


/*
 
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;

namespace MediaProcessor.Infrastructure.Validation;

public interface IMediaFileValidator
{
    Task<ValidationResult> ValidateMediaFileAsync(string s3Bucket, string s3Key, CancellationToken cancellationToken);
}

public record ValidationResult(bool IsValid, string? ErrorCode, string? ErrorMessage, string? DetectedMimeType);

public class MediaFileValidator(
    ILogger<MediaFileValidator> logger,
    AppDbContext context,
    IAmazonS3 s3Client) : IMediaFileValidator
{
    // Magic Byte Signatures
    private static readonly byte[] Mp4FtypMagic = "ftyp"u8.ToArray();
    private static readonly byte[] EbmlMagic = [0x1A, 0x45, 0xDF, 0xA3]; // MKV/WebM
    private static readonly byte[] Id3Magic = "ID3"u8.ToArray(); // MP3 ID3v2
    private static readonly byte[] WebVttMagic = "WEBVTT"u8.ToArray();

    public async Task<ValidationResult> ValidateMediaFileAsync(string s3Bucket, string s3Key, CancellationToken cancellationToken)
    {
        logger.LogDebug("Starting multi-tier validation for S3 Object s3://{Bucket}/{Key}", s3Bucket, s3Key);

        try
        {
            // -------------------------------------------------------------------------
            // TIER 1: Extension & Allowed DB Check
            // -------------------------------------------------------------------------
            var fileExtension = Path.GetExtension(s3Key)?.ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(fileExtension))
            {
                logger.LogWarning("Media file has no extension: {Key}", s3Key);
                return new ValidationResult(false, "NO_EXTENSION", "File extension is missing.", null);
            }

            var allowedType = await context.AllowedFileTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Extension == fileExtension && p.IsActive, cancellationToken);

            if (allowedType == null)
            {
                logger.LogWarning("Media file type not allowed by database policy: {FileExtension}", fileExtension);
                return new ValidationResult(false, "EXTENSION_NOT_ALLOWED", $"Extension '{fileExtension}' is not permitted.", null);
            }

            // -------------------------------------------------------------------------
            // TIER 2: S3 Range Request - Magic Byte Inspection (Header Inspection)
            // -------------------------------------------------------------------------
            var headerBytes = await FetchS3HeaderBytesAsync(s3Bucket, s3Key, bytesToRead: 64, cancellationToken);
            if (headerBytes.Length < 12)
            {
                return new ValidationResult(false, "FILE_TOO_SMALL", "File header is under minimum size threshold.", null);
            }

            var (isMagicValid, detectedMime) = ValidateMagicBytes(fileExtension, headerBytes);
            if (!isMagicValid)
            {
                logger.LogWarning("Magic byte mismatch for s3://{Bucket}/{Key}. Claimed ext: {Ext}, Detected: {Mime}", s3Bucket, s3Key, fileExtension, detectedMime);
                return new ValidationResult(false, "SPOOFED_FILE_TYPE", $"File content does not match extension '{fileExtension}'. Detected: {detectedMime}", detectedMime);
            }

            // Subtitles & Audio skip deep ffprobe checks
            if (fileExtension is ".vtt" or ".srt")
            {
                return new ValidationResult(true, null, null, detectedMime);
            }

            // -------------------------------------------------------------------------
            // TIER 3: FFprobe Stream Inspection via Presigned S3 URL
            // -------------------------------------------------------------------------
            var (isStreamValid, streamError) = await ValidateStreamsWithFFprobeAsync(s3Bucket, s3Key, cancellationToken);
            if (!isStreamValid)
            {
                logger.LogWarning("FFprobe stream inspection failed for s3://{Bucket}/{Key}: {Error}", s3Bucket, s3Key, streamError);
                return new ValidationResult(false, "CORRUPT_OR_UNSUPPORTED_STREAM", streamError, detectedMime);
            }

            logger.LogInformation("Media file validated successfully: s3://{Bucket}/{Key}", s3Bucket, s3Key);
            return new ValidationResult(true, null, null, detectedMime);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while validating s3://{Bucket}/{Key}", s3Bucket, s3Key);
            throw;
        }
    }

    /// <summary>
    /// Fetches only the first N bytes from S3 using HTTP Range Requests (Zero-copy full download)
    /// </summary>
    private async Task<byte[]> FetchS3HeaderBytesAsync(string bucket, string key, int bytesToRead, CancellationToken cancellationToken)
    {
        var request = new GetObjectRequest
        {
            BucketName = bucket,
            Key = key,
            ByteRange = new ByteRange(0, bytesToRead - 1)
        };

        using var response = await s3Client.GetObjectAsync(request, cancellationToken);
        using var ms = new MemoryStream();
        await response.ResponseStream.CopyToAsync(ms, cancellationToken);
        return ms.ToArray();
    }

    /// <summary>
    /// Checks header bytes against known magic signatures
    /// </summary>
    private static (bool IsValid, string DetectedMime) ValidateMagicBytes(string extension, byte[] header)
    {
        // MP4 check: contains 'ftyp' starting at byte offset 4
        if (extension is ".mp4" or ".mov")
        {
            var isMp4 = header.Length >= 12 && header.AsSpan(4, 4).SequenceEqual(Mp4FtypMagic);
            return (isMp4, isMp4 ? "video/mp4" : "application/octet-stream");
        }

        // MKV / WebM EBML check
        if (extension is ".mkv" or ".webm")
        {
            var isEbml = header.Length >= 4 && header.AsSpan(0, 4).SequenceEqual(EbmlMagic);
            return (isEbml, isEbml ? "video/x-matroska" : "application/octet-stream");
        }

        // MP3 check (ID3v2 header tag)
        if (extension is ".mp3")
        {
            var isMp3 = header.Length >= 3 && header.AsSpan(0, 3).SequenceEqual(Id3Magic);
            return (isMp3, isMp3 ? "audio/mpeg" : "application/octet-stream");
        }

        // WebVTT check
        if (extension is ".vtt")
        {
            var isVtt = header.Length >= 6 && header.AsSpan(0, 6).SequenceEqual(WebVttMagic);
            return (isVtt, isVtt ? "text/vtt" : "text/plain");
        }

        // Fallback for custom allowed types
        return (true, "application/octet-stream");
    }

    /// <summary>
    /// Runs ffprobe against S3 presigned URL without downloading the 400 GB file locally
    /// </summary>
    private async Task<(bool IsValid, string? ErrorMessage)> ValidateStreamsWithFFprobeAsync(string bucket, string key, CancellationToken cancellationToken)
    {
        // Generate short-lived presigned URL for ffprobe streaming
        var urlRequest = new GetPreSignedUrlRequest
        {
            BucketName = bucket,
            Key = key,
            Expires = DateTime.UtcNow.AddMinutes(10)
        };
        var presignedUrl = await s3Client.GetPreSignedURLAsync(urlRequest);

        var startInfo = new ProcessStartInfo
        {
            FileName = "ffprobe",
            Arguments = $"-v quiet -print_format json -show_format -show_streams \"{presignedUrl}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);

        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0)
        {
            var err = await errorTask;
            return (false, $"FFprobe execution failed: {err}");
        }

        var jsonOutput = await outputTask;
        using var doc = JsonDocument.Parse(jsonOutput);

        var hasStreams = doc.RootElement.TryGetProperty("streams", out var streams) && streams.GetArrayLength() > 0;
        if (!hasStreams)
        {
            return (false, "Media file contains no readable audio or video streams.");
        }

        return (true, null);
    }
}




*/