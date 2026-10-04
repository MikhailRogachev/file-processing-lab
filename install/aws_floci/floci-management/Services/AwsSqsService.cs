

namespace floci_management.Services;

public class AwsSqsService : AwsService
{
    public async Task<List<string>> QueueListAsync(CancellationToken cancellationToken)
    {
        using var client = SqsClient();
        var response = await client.ListQueuesAsync(new ListQueuesRequest());

        if (response == null || response.HttpStatusCode != System.Net.HttpStatusCode.OK)
        {
            throw new FlociOperationException("The error during queues list request.");
        }

        if (!response.QueueUrls.Any())
        {
            return new List<string>();
        }

        return response.QueueUrls;
    }
}
