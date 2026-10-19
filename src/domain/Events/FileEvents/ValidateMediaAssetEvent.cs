namespace domain.Events.FileEvents;

/// <summary>
/// 
/// </summary>
/// <param name="Id"></param>
/// <param name="ReferenceId"></param>
/// <param name="CreatedAt"></param>
/// <param name="FileName"></param>
public record ValidateMediaAssetEvent(Guid Id, Guid ReferenceId, DateTimeOffset CreatedAt, string FileName) : IEvent;