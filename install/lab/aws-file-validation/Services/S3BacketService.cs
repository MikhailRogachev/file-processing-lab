using Amazon.S3.Model;
using System.Text;

namespace aws_file_validation.Services;

public class S3BacketService : AwsServiceBase
{
    public async Task<List<FileItem>> GetFilesCollectionAsync(CancellationToken cancellationToken)
    {
        var list = new List<FileItem>();
        var index = 1;
        using var client = Client();
        var request = new ListObjectsV2Request
        {
            BucketName = Backetname,
            Prefix = Folder
        };

        var response = await client.ListObjectsV2Async(request, cancellationToken);

        foreach (S3Object obj in response.S3Objects)
        {
            list.Add(new FileItem(index, obj.Key, obj.Size));
            index++;
        }

        return list;
    }

    public string GetFileExtension(string filename)
    {
        return Path.GetExtension(filename);
    }






























    public async Task<string> GetMetadataAsync(string fileName, CancellationToken cancellationToken)
    {
        try
        {
            var request = new GetObjectMetadataRequest
            {
                BucketName = Backetname,
                Key = fileName
            };

            using var client = Client();
            GetObjectMetadataResponse response = await client.GetObjectMetadataAsync(request);

            if (response == null)
            {
                return $"Error occured during Metadata request. The response is null";
            }

            if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                return $"Error occured during Metadata request. The status code {response.HttpStatusCode}";
            }

            var msg = new StringBuilder();
            msg.AppendLine($"Content-Type: {response.Headers.ContentType}");
            msg.AppendLine($"Content-Length: {response.Headers.ContentLength} bytes");
            msg.AppendLine($"Last-Modified: {response.LastModified}");
            msg.AppendLine($"ETag: {response.ETag}");

            return msg.ToString();
        }
        catch (Exception ex)
        {
            return $"Error occured during Metadata request {ex.Message}";
        }
    }
}



