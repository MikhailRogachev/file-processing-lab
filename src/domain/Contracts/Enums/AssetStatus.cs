namespace domain.Contracts.Enums;

public enum AssetStatus
{
    Unknown,
    Created,
    Validated,
    ValidationFailed,
    Running,
    Completed,
    Failed,
    Retried,
    Paused,
}
