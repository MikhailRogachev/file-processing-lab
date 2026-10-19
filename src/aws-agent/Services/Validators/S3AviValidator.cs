using System.Text;

namespace aws_agent.Services.Validators;

public class S3AviValidator(
    ILogger<S3AviValidator> logger,
    IAwsClientConnectionFactory awsClientFactory,
    IOptions<AwsOptions> options)
    : S3BaseValidator(logger, awsClientFactory, options)
{
    public override string Name => "AviFileProbe";
    protected override int StartByte => 0;
    protected override int EndByte => 11; // Fetch exactly the first 12 bytes

    protected override async Task<bool> ValidateStreamAsync(Stream stream, CancellationToken cancellationToken)
    {
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms, cancellationToken);
        byte[] bytes = ms.ToArray();

        if (bytes.Length < 12) return false;

        // Validate "RIFF" signature (Bytes 0-3)
        string riff = Encoding.ASCII.GetString(bytes, 0, 4);
        if (riff != "RIFF") return false;

        // Validate "AVI " signature (Bytes 8-11, note the trailing space)
        string avi = Encoding.ASCII.GetString(bytes, 8, 4);
        if (avi != "AVI ") return false;

        return true;
    }
}