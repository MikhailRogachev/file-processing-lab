namespace domain.Contracts.Enums;

public enum OutboxMessageStatus
{
    Created,
    Queued,
    Running,
    Completed,
    Failed
}
