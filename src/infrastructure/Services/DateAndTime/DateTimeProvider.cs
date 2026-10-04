using domain.Interfaces.DateAndTime;

namespace infrastructure.Services.DateAndTime;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime Now => DateTime.Now;

    public DateTimeOffset NowOffset => DateTimeOffset.Now;

    public DateTime UtcNow => DateTime.UtcNow;

    public DateOnly UtcToday => DateOnly.FromDateTime(DateTime.UtcNow);
}
