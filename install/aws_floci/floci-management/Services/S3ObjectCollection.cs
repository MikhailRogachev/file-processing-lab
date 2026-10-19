namespace floci_management.Services;

public class S3ObjectCollection(ConnectionTarget target) : AwsBaseService<IList<FileItem>>(target)
{

    protected override async Task<IList<FileItem>> GetRequestResultAsync(S3RequestContext context, CancellationToken cancellationToken)
    {
        var list = new List<FileItem>();
        var index = 1;
        var request = new ListObjectsV2Request
        {
            BucketName = context.BucketName,
            Prefix = context.Path
        };

        var response = await Client().ListObjectsV2Async(request, cancellationToken);

        if (response == null || response.S3Objects == null || !response.S3Objects.Any())
        {
            throw new InvalidOperationException($"No any files found in the Bucket {context.BucketName}, folder {context.Path}");
        }

        foreach (S3Object obj in response.S3Objects)
        {
            list.Add(new FileItem(index, obj.Key, obj.Size));
            index++;
        }

        return list;
    }

}
