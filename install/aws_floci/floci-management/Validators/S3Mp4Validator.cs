namespace floci_management.Validators;

public class S3Mp4Validator(ConnectionTarget target) : S3BaseValidator(target)
{
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
