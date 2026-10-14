using Amazon;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.S3;

namespace aws_file_validation.Services;

public class AwsServiceBase
{
    private readonly string my_credentials = @"C:\Users\mikrog\.aws\appcredentials";

    protected string Backetname => "dev-makalu-dev-playoutassets";
    protected string Folder => "Clips/Test-Example/Rpgachev/";

    protected AWSCredentials GetCredentials()
    {
        var profile = new CredentialProfileStoreChain(my_credentials);

        if (!profile.TryGetAWSCredentials("default", out AWSCredentials credentials))
        {
            throw new AmazonAccountIdException("Wrong credentials");
        }

        return credentials;
    }

    protected IAmazonS3 Client()
    {
        return new AmazonS3Client(GetCredentials(), RegionEndpoint.EUCentral1);
    }
}
