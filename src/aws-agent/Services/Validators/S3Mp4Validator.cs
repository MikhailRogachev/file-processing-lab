namespace aws_agent.Services.Validators;

public class S3Mp4Validator(
    ILogger<S3Mp4Validator> logger,
    IAwsClientConnectionFactory awsClientFactory
    ) : S3BaseValidator(logger, awsClientFactory)
{

    public override string Name => "Mp4FileProbe";
    protected override int StartByte => 0;
    protected override int EndByte => 11;

    protected override async Task<bool> ValidateStreamAsync(Stream stream, CancellationToken cancellationToken)
    {
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms, cancellationToken);
        byte[] bytes = ms.ToArray();

        return bytes.Length >= 8 &&
               bytes[4] == 'f' && bytes[5] == 't' && bytes[6] == 'y' && bytes[7] == 'p';

    }
}
