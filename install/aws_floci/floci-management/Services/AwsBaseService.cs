using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;

namespace floci_management.Services;

public abstract class AwsBaseService<T> where T : class
{
    private readonly ConnectionTarget _target;

    public string Bucket => _target == ConnectionTarget.LOCAL ? ConnectionSettings.LOCAL_BUCKET_NAME : ConnectionSettings.REMOTE_BUCKET_NAME;

    public string FolderName => _target == ConnectionTarget.LOCAL ? ConnectionSettings.LOCAL_FOLDER_NAME : ConnectionSettings.REMOTE_FOLDER_NAME;

    public AwsBaseService(ConnectionTarget connectionTarget)
    {
        _target = connectionTarget;
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

    private AWSCredentials GetCredentials()
    {
        var profile = new CredentialProfileStoreChain(ConnectionSettings.REMOTE_PROFILE_FILENAME);

        if (!profile.TryGetAWSCredentials("default", out AWSCredentials credentials))
        {
            throw new AmazonAccountIdException("Wrong credentials");
        }

        return credentials;
    }

    private AmazonS3Config S3Config => new AmazonS3Config
    {
        ServiceURL = ConnectionSettings.LOCAL_AWS_ENDPOINT_URL,
        AuthenticationRegion = ConnectionSettings.LOCAL_AWS_DEFAULT_REGION,
        ForcePathStyle = true
    };

    public async Task<S3Payload<T>> RunRequestAsync(S3RequestContext context, CancellationToken cancellationToken)
    {
        try
        {
            var response = await GetRequestResultAsync(context, cancellationToken);

            return new S3Payload<T>
            {
                IsSuccess = true,
                Message = string.Empty,
                Context = response
            };
        }
        catch (Exception ex)
        {
            return new S3Payload<T>
            {
                IsSuccess = false,
                Message = ex.Message
            };
        }
    }

    protected abstract Task<T> GetRequestResultAsync(S3RequestContext context, CancellationToken cancellationToken);
}