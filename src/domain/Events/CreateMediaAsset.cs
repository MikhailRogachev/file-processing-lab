namespace domain.Events;

public class CreateMediaAsset : BaseEvent
{
    public string Filename { get; set; } = string.Empty;

    public CreateMediaAsset(string filename)
    {
        this.Filename = filename;
    }
}
