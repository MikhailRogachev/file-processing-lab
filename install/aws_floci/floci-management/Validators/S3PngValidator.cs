namespace floci_management.Validators;

public class S3PngValidator(ConnectionTarget target) : S3BaseValidator(target)
{
    private static readonly byte[] PngSignature = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
    protected override int StartByte => 0;
    protected override int EndByte => 7;

    protected override async Task<bool> ValidateStreamAsync(Stream stream, CancellationToken cancellationToken)
    {
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms, cancellationToken);
        byte[] bytes = ms.ToArray();

        if (bytes.Length < PngSignature.Length) return false;

        // Verify that the first 8 bytes match the PNG signature exactly
        for (int i = 0; i < PngSignature.Length; i++)
        {
            if (bytes[i] != PngSignature[i])
            {
                return false;
            }
        }

        return true;
    }
}
