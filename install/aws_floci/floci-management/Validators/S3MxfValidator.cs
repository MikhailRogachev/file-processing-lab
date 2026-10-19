namespace floci_management.Validators;

public class S3MxfValidator(ConnectionTarget target) : S3BaseValidator(target)
{
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

        return IndexOfSequence(bytes, MxfSignature) >= 0;
    }

    private static int IndexOfSequence(byte[] buffer, byte[] pattern)
    {
        int limit = buffer.Length - pattern.Length;
        for (int i = 0; i <= limit; i++)
        {
            bool match = true;
            for (int j = 0; j < pattern.Length; j++)
            {
                if (buffer[i + j] != pattern[j])
                {
                    match = false;
                    break;
                }
            }
            if (match) return i;
        }
        return -1;
    }
}
