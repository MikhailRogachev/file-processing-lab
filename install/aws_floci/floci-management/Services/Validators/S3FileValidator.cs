namespace floci_management.Services.Validators;

public class S3FileValidator : AwsService
{
    private readonly string[] _videofiles = new string[] { ".mp4", ".mxf" };
    private readonly string[] _textfiles = new string[] { ".srt", ".stl" };

    public async Task<string> ValidateFileAsync(string filename, CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(filename);

        if (string.IsNullOrEmpty(extension))
        {
            throw new ArgumentNullException(string.Format("The extension for the file {} is nor found.", filename));
        }
        else if (_videofiles.Contains(extension))
        {
            return await ValidateVideoFileAsync(filename, extension, cancellationToken);
        }
        else if (_textfiles.Contains(extension))
        {
            return await ValidateTextFileAsync(filename, extension, cancellationToken);
        }
        else
        {
            return string.Format("It is impossible to recognize file type for the file {fn}", filename);
        }
    }

    private async Task<string> ValidateTextFileAsync(string filename, string extension, CancellationToken cancellationToken)
    {
        var request = GetS3ObjectRequest(filename, 0, 2048);
        using var response = await S3Client().GetObjectAsync(request, cancellationToken);

        using var reader = new StreamReader(response.ResponseStream); // Stream file content directly from S3

        // 2. Read the first few lines from S3 stream
        string? line1 = await reader.ReadLineAsync(cancellationToken); // Expected: "1"
        string? line2 = await reader.ReadLineAsync(cancellationToken); // Expected: "00:00:00,000 --> 00:00:03,003"[cite: 9]

        if (string.IsNullOrWhiteSpace(line1) || string.IsNullOrWhiteSpace(line2))
        {
            return string.Format("The file {0} didn't recognize", filename);
        }

        bool hasSequenceNumber = int.TryParse(line1.Trim(), out int seq) && seq == 1; //[cite: 9]
        bool hasTimestamp = line2.Contains("-->"); //[cite: 9]

        if (hasSequenceNumber && hasTimestamp)
        {
            return string.Format("The file {0} is a valid test file", filename);
        }

        return string.Format("The file {0} has an invalid format", filename);

    }

    private async Task<string> ValidateVideoFileAsync(string filename, string extension, CancellationToken cancellationToken)
    {
        var request = GetS3ObjectRequest(filename, 0, 11);

        using var response = await S3Client().GetObjectAsync(request, cancellationToken);
        using var ms = new MemoryStream();
        await response.ResponseStream.CopyToAsync(ms, cancellationToken);
        var bytes = ms.ToArray();

        if (extension == ".mp4" && GetMp4Magic(bytes))
        {
            return string.Format("The file {0} is MP4 file", filename);
        }


        return string.Format("The file {0} didn't recognize", filename);
    }

    private GetObjectRequest GetS3ObjectRequest(string filename, int firstByte, int lastByte)
    {
        return new GetObjectRequest
        {
            BucketName = Settings.BUCKET_NAME,
            Key = filename,
            ByteRange = new ByteRange(firstByte, lastByte)
        };
    }


    private static bool GetMp4Magic(byte[] bytes)
    {
        return bytes.Length >= 8 &&
                   bytes[4] == 'f' && bytes[5] == 't' && bytes[6] == 'y' && bytes[7] == 'p';
    }
}
