using System.Text;

namespace floci_management.Services;

public class FileMenuBuilder
{
    public IList<Sample> Samples { get; set; } = new List<Sample>();

    public bool IsEmpty => Samples == null || !Samples.Any();

    public override string ToString()
    {
        if (Samples == null || !Samples.Any())
        {
            return "No any files in the source folder found";
        }
        else
        {
            var sb = new StringBuilder();
            sb.AppendLine("Select file from the list:");

            foreach (var sample in Samples)
            {
                sb.AppendLine(string.Format("|{0,-3}| {1}", sample.Index, sample.FileName));
            }

            return sb.ToString();
        }
    }
}
