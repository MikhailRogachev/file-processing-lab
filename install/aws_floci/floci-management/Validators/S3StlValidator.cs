using System.Text;

namespace floci_management.Validators;

public class S3StlValidator(ConnectionTarget target) : S3BaseValidator(target)
{
    protected override int StartByte => 0;
    protected override int EndByte => 1023;

    protected override async Task<bool> ValidateStreamAsync(Stream stream, CancellationToken cancellationToken)
    {
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms, cancellationToken);
        byte[] headerBytes = ms.ToArray();

        if (headerBytes.Length < 1024) return false;

        // 1. Validate Code Page Number (Bytes 0-2: Must be a 3-digit numeric code page)
        string cpn = Encoding.ASCII.GetString(headerBytes, 0, 3);
        if (!int.TryParse(cpn, out int cpnValue) || cpnValue < 100 || cpnValue > 999) return false;

        // 2. Validate Disk Format Code (Bytes 3-10: e.g. "STL25.01")
        string dfc = Encoding.ASCII.GetString(headerBytes, 3, 8);
        if (!dfc.StartsWith("STL", StringComparison.Ordinal)) return false;

        // 3. Validate Total Number of TTI Blocks (Bytes 238-242)
        string ttiCountStr = Encoding.ASCII.GetString(headerBytes, 238, 5);
        if (!int.TryParse(ttiCountStr.Trim(), out int ttiBlockCount) || ttiBlockCount < 0) return false;

        return true;
    }
}
