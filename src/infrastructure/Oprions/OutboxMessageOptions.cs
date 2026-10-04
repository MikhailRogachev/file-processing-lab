namespace infrastructure.Oprions;

public class OutboxMessageOptions
{
    /// <summary>
    /// Get; Set; outbox message request interval (mSec)
    /// </summary>
    public int ScanIntervalMs { get; set; }

    public int ExceptionsAllowedBeforeBreak { get; set; }

    public int DurationOfBreakSec { get; set; }
}