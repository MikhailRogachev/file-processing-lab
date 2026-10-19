using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;

namespace floci_management.Validators;

public abstract class S3BaseValidator
{
    private readonly ConnectionTarget _target;
    protected abstract int StartByte { get; }
    protected abstract int EndByte { get; }
    public string Bucket => _target == ConnectionTarget.LOCAL ? ConnectionSettings.LOCAL_BUCKET_NAME : ConnectionSettings.REMOTE_BUCKET_NAME;
    public bool IsValid { get; private set; }

    public S3BaseValidator(ConnectionTarget target)
    {
        _target = target;
    }

    #region connection region

    private AmazonS3Config S3Config => new AmazonS3Config
    {
        ServiceURL = ConnectionSettings.LOCAL_AWS_ENDPOINT_URL,
        AuthenticationRegion = ConnectionSettings.LOCAL_AWS_DEFAULT_REGION,
        ForcePathStyle = true
    };

    private AWSCredentials GetCredentials()
    {
        var profile = new CredentialProfileStoreChain(ConnectionSettings.REMOTE_PROFILE_FILENAME);

        if (!profile.TryGetAWSCredentials("default", out AWSCredentials credentials))
        {
            throw new AmazonAccountIdException("Wrong credentials");
        }

        return credentials;
    }

    protected AmazonS3Client Client()
    {
        if (_target == ConnectionTarget.LOCAL)
        {
            return new AmazonS3Client(ConnectionSettings.LOCAL_AWS_ACCESS_KEY_ID, ConnectionSettings.LOCAL_AWS_SECRET_ACCESS_KEY, S3Config);
        }
        else if (_target == ConnectionTarget.REMOTE)
        {
            return new AmazonS3Client(GetCredentials(), RegionEndpoint.EUCentral1);
        }
        else
        {
            throw new ArgumentException("The Connection wasn't established");
        }
    }

    #endregion

    public GetObjectRequest GetObjectRequest(FileProbe context, int firstByte, int lastByte)
    {
        return new GetObjectRequest
        {
            BucketName = context.S3Bucket,
            Key = context.S3Key,
            ByteRange = new ByteRange(firstByte, lastByte)
        };
    }

    public async Task<S3Payload<string>> ValidateAsync(FileProbe context, CancellationToken cancellationToken)
    {
        IsValid = false;

        try
        {
            var request = new GetObjectRequest
            {
                BucketName = context.S3Bucket,
                Key = context.S3Key,
                ByteRange = new ByteRange(StartByte, EndByte)
            };

            using var response = await Client().GetObjectAsync(request, cancellationToken);

            // Delegate specific stream parsing to the child class
            IsValid = await ValidateStreamAsync(response.ResponseStream, cancellationToken);

            if (!IsValid)
            {
                return new S3Payload<string>
                {
                    IsSuccess = false,
                    Message = string.Format("The file {0} has an invalid format", context.S3Key),
                    Context = string.Format("The file {0} has an invalid format", context.S3Key)
                };
            }

            return new S3Payload<string>
            {
                IsSuccess = true,
                Context = string.Format("The file {0} has a valid format", context.S3Key),
                Message = string.Format("The file {0} has a valid format", context.S3Key)
            };
        }
        catch (Exception ex)
        {
            return new S3Payload<string>
            {
                IsSuccess = false,
                Context = string.Format("The file {0} has an invalid format", context.S3Key),
                Message = ex.Message
            };
        }
    }

    protected abstract Task<bool> ValidateStreamAsync(Stream stream, CancellationToken cancellationToken);
}
