namespace aws_agent.Services.Validators;

public class S3SrtValidator(
    ILogger<S3SrtValidator> logger,
    IAwsClientConnectionFactory awsClientFactory,
    IOptions<AwsOptions> options
    ) : S3BaseValidator(logger, awsClientFactory, options)
{
    public override string Name => "SrtFileProbe";
    protected override int StartByte => 0;
    protected override int EndByte => 2048;

    protected override async Task<bool> ValidateStreamAsync(Stream stream, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(stream);

        string? line1 = await reader.ReadLineAsync(cancellationToken);
        string? line2 = await reader.ReadLineAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(line1) || string.IsNullOrWhiteSpace(line2))
            return false;

        bool hasSequenceNumber = int.TryParse(line1.Trim(), out int seq) && seq == 1;
        bool hasTimestamp = line2.Contains("-->");

        return hasSequenceNumber && hasTimestamp;
    }
}
