namespace floci_management.Services;

public class AwsS3Service : AwsService
{
    public async Task<List<Bucket>> ListS3BucketsAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var s3Client = S3Client();
            var listBucketsResponse = await s3Client.ListBucketsAsync(cancellationToken);

            if (listBucketsResponse.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                Console.WriteLine("Failed to list S3 buckets. HTTP Status Code: {0}", listBucketsResponse.HttpStatusCode);
                return new List<Bucket>();
            }

            if (listBucketsResponse.Buckets == null || listBucketsResponse.Buckets.Count == 0)
            {
                Console.WriteLine("No S3 buckets found.");
                return new List<Bucket>();
            }

            return listBucketsResponse.Buckets.Select(p => new Bucket(p.BucketArn, p.BucketName, p.BucketRegion, p.CreationDate)).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error listing S3 buckets: {ex.Message}");
            throw;
        }
    }

    //http://my-bucket.s3-website-us-east-1.localhost:4566/
    public async Task<Bucket> CreateS3BucketAsync(string bucketname, CancellationToken cancellationToken)
    {
        try
        {
            using var s3Client = S3Client();

            var request = new PutBucketRequest
            {
                BucketName = bucketname,
                BucketRegionName = Settings.AWS_DEFAULT_REGION
            };

            await s3Client.PutBucketAsync(request, cancellationToken);

            var list = await ListS3BucketsAsync(cancellationToken);

            if (!list.Any() || !list.Any(p => p.BucketName == bucketname))
            {
                throw new FlociOperationException($"The bucket {bucketname} is not created.");
            }

            return list.First(p => p.BucketName == bucketname);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating S3 bucket: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> UploadFileAsync(
        string filename,
        CancellationToken cancellationToken)
    {
        try
        {
            var request = new PutObjectRequest
            {
                BucketName = Settings.BUCKET_NAME,
                Key = filename,
                FilePath = filename,
            };

            using var s3Client = S3Client();
            var response = await s3Client.PutObjectAsync(request, cancellationToken);

            return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
        }
        catch (AmazonS3Exception ex)
        {
            Console.WriteLine("Error uploading sample: {0}", ex.Message);
            return false;
        }
    }
}
