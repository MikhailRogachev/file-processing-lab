namespace domain.Events;

public class ValidateMediaAsset : BaseEvent
{
    public string Filename { get; set; } = string.Empty;

    public ValidateMediaAsset(string filename)
    {
        this.Filename = filename;
    }
}
