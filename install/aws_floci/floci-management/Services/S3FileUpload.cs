using System.Net;
using System.Text;

namespace floci_management.Services;

public class S3FileUpload(ConnectionTarget target) : AwsBaseService<string>(target)
{
    protected override async Task<string> GetRequestResultAsync(S3RequestContext context, CancellationToken cancellationToken)
    {
        using var fileStream = File.OpenRead(Path.Combine(context.Path, context.FileName));

        var request = new PutObjectRequest
        {
            BucketName = context.BucketName,
            Key = context.FileName, //Path.Combine(context.Path, context.FileName),
            InputStream = fileStream,
            ChecksumAlgorithm = ChecksumAlgorithm.SHA256,
        };

        request.Metadata.Add(ConnectionSettings.AWS_META_IDENTIFIER_FIELD, context.PackageIdentifier);
        request.Metadata.Add(ConnectionSettings.AWS_META_PRIORITY_FIELD, context.Priority.ToString());

        var response = await Client().PutObjectAsync(request, cancellationToken);

        if (response != null && response.HttpStatusCode != HttpStatusCode.OK)
        {
            throw new InvalidOperationException(string.Format("The file {0} is not uploaded. Status code received {1}", context.FileName, response.HttpStatusCode));
        }
        else if (response != null && response.HttpStatusCode == HttpStatusCode.OK)
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Format("The file {0} is successfully uploaded", context.FileName));
            sb.AppendLine(string.Format("ContentLength            - {0}", response.ContentLength));
            sb.AppendLine(string.Format("VersionId                - {0}", response.VersionId));
            sb.AppendLine(string.Format("Size                     - {0}", response.Size));
            sb.AppendLine(string.Format("ETag                     - {0}", response.ETag));
            sb.AppendLine(string.Format("CheckSum                 - {0}", response.ChecksumSHA256));
            sb.AppendLine(string.Format("ChecksumValidationStatus - {0}", response.ResponseMetadata.ChecksumValidationStatus));
            sb.AppendLine(string.Format("ChecksumAlgorithm        - {0}", response.ResponseMetadata.ChecksumAlgorithm));

            foreach (var key in response.ResponseMetadata.Metadata.Keys)
            {
                sb.AppendLine(string.Format("             {0} - {1}", key, response.ResponseMetadata.Metadata[key]));
            }
            return sb.ToString();
        }
        else
        {
            throw new InvalidOperationException("Invalid service response received");
        }
    }
}