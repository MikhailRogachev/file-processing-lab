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





    ////http://my-bucket.s3-website-us-east-1.localhost:4566/
    //public async Task<Bucket> CreateS3BucketAsync(string bucketname, CancellationToken cancellationToken)
    //{
    //    try
    //    {
    //        using var s3Client = S3Client();

    //        var request = new PutBucketRequest
    //        {
    //            BucketName = bucketname,
    //            BucketRegionName = ConnectionSettings.LOCAL_AWS_DEFAULT_REGION
    //        };

    //        await s3Client.PutBucketAsync(request, cancellationToken);

    //        var list = await ListS3BucketsAsync(cancellationToken);

    //        if (!list.Any() || !list.Any(p => p.BucketName == bucketname))
    //        {
    //            throw new FlociOperationException($"The bucket {bucketname} is not created.");
    //        }

    //        return list.First(p => p.BucketName == bucketname);
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine($"Error creating S3 bucket: {ex.Message}");
    //        throw;
    //    }
    //}

    //public async Task<bool> UploadFileAsync(string filename, CancellationToken cancellationToken)
    //{
    //    try
    //    {
    //        var request = new PutObjectRequest
    //        {
    //            BucketName = ConnectionSettings.LOCAL_BUCKET_NAME,
    //            Key = filename,
    //            FilePath = filename,
    //        };

    //        using var s3Client = S3Client();
    //        var response = await s3Client.PutObjectAsync(request, cancellationToken);

    //        return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
    //    }
    //    catch (AmazonS3Exception ex)
    //    {
    //        Console.WriteLine("Error uploading sample: {0}", ex.Message);
    //        throw;
    //    }
    //}

    //public async Task<List<FileItem>> GetFilesCollectionAsync(CancellationToken cancellationToken)
    //{
    //    var list = new List<FileItem>();
    //    var index = 1;
    //    using var client = S3Client();
    //    var request = new ListObjectsV2Request
    //    {
    //        BucketName = ConnectionSettings.LOCAL_BUCKET_NAME,
    //        Prefix = ""
    //    };

    //    var response = await client.ListObjectsV2Async(request, cancellationToken);

    //    if (response == null || response.S3Objects == null || !response.S3Objects.Any())
    //    {
    //        return list;
    //    }

    //    foreach (S3Object obj in response.S3Objects)
    //    {
    //        list.Add(new FileItem(index, obj.Key, obj.Size));
    //        index++;
    //    }

    //    return list;
    //}
    //protected override async Task<S3Payload<IList<FileItem>> GetRequestResultAsync<FileItem>(S3RequestContext context, CancellationToken cancellationToken)
    //{



}
