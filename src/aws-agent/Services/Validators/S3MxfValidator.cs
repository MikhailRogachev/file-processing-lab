using domain.Extensions;

namespace aws_agent.Services.Validators;

public class S3MxfValidator(
    ILogger<S3MxfValidator> logger,
    IAwsClientConnectionFactory awsClientFactory,
    IOptions<AwsOptions> options
    ) : S3BaseValidator(logger, awsClientFactory, options)
{
    public override string Name => "MxfFileProbe";
    protected override int StartByte => 0;
    protected override int EndByte => 65535;

    // SMPTE ST 377-1 Partition Pack Key (11 bytes)
    private static readonly byte[] MxfSignature = { 0x06, 0x0E, 0x2B, 0x34, 0x02, 0x05, 0x01, 0x01, 0x0D, 0x01, 0x02 };

    protected override async Task<bool> ValidateStreamAsync(Stream stream, CancellationToken cancellationToken)
    {
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms, cancellationToken);
        byte[] bytes = ms.ToArray();

        if (bytes.Length < MxfSignature.Length) return false;

        return bytes.IndexOfSequence(MxfSignature) >= 0;
    }
}
