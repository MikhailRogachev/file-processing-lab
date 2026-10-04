namespace domain.Interfaces.DateAndTime;

public interface IDateTimeProvider
{
    DateTime Now { get; }
    DateTimeOffset NowOffset { get; }
    DateTime UtcNow { get; }
    DateOnly UtcToday { get; }
}
