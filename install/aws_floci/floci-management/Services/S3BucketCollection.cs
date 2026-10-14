namespace floci_management.Services;

public class S3BucketCollection(ConnectionTarget target) : AwsBaseService<IList<Bucket>>(target)
{
    protected override async Task<IList<Bucket>> GetRequestResultAsync(S3RequestContext context, CancellationToken cancellationToken)
    {
        var response = await Client().ListBucketsAsync(cancellationToken);

        if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
        {
            throw new InvalidOperationException($"Failed to list S3 buckets. HTTP Status Code: {response.HttpStatusCode}");
        }

        if (response.Buckets == null || response.Buckets.Count == 0)
        {
            throw new InvalidOperationException("No S3 buckets found.");
        }

        return response.Buckets.Select(p => new Bucket(p.BucketArn, p.BucketName, p.BucketRegion, p.CreationDate)).ToList();
    }
}
