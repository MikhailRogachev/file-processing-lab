using System.Text;

namespace floci_management.Services;

public class S3MetaData(ConnectionTarget target) : AwsBaseService<string>(target)
{
    protected override async Task<string> GetRequestResultAsync(S3RequestContext context, CancellationToken cancellationToken)
    {
        var metadataResponse = await Client().GetObjectMetadataAsync(context.BucketName, context.FileName);
        var identifier = metadataResponse.Metadata[ConnectionSettings.AWS_META_IDENTIFIER_FIELD];
        var priority = metadataResponse.Metadata[ConnectionSettings.AWS_META_PRIORITY_FIELD];

        var sb = new StringBuilder();
        sb.AppendLine($"{ConnectionSettings.AWS_META_IDENTIFIER_FIELD} = {identifier}");
        sb.AppendLine($"{ConnectionSettings.AWS_META_PRIORITY_FIELD} = {priority}");

        return sb.ToString();
    }
}
