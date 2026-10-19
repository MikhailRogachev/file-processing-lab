namespace domain.Contracts.Enums;

public enum State
{
    Unknown,
    Created,
    Validated,
    Running,
    Completed,
    Failed,
    Retried,
    Paused,
}
