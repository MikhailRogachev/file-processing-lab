using aws_agent.Extensions;

namespace aws_agent.Services;

public class SqsMessageConsumer(
    ILogger<SqsMessageConsumer> logger,
    IOptions<AwsOptions> options
    ) : IMessageConsumer<AmazonSQSClient>
{

    /// <summary>
    /// Asynchronously retrieves and parses a batch of pending messages from the configured SQS queue.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains an <see cref="IList{QueueMessageConsumed}"/> 
    /// of parsed messages. Returns an empty list if no messages were retrieved from the queue.
    /// </returns>
    /// <exception cref="Exception">
    /// Thrown when the SQS service returns a non-200 HTTP response status code.
    /// </exception>
    /// <exception cref="JsonException">
    /// Thrown downstream when a message body contains malformed JSON during conversion.
    /// </exception>
    /// <exception cref="InvalidCastException">
    /// Thrown downstream when an S3 event payload is missing required schema fields during conversion.
    /// </exception>
    public async Task<IList<QueueMessageConsumed>> ReceiveMessageAsync(CancellationToken cancellationToken)
    {
        // get sqs queue url
        var queueUrl = await GetQueueUrlAsync();
        var request = GetConsumeRequest(queueUrl);

        logger.LogDebug("Requesting messages from the SQS queue - {queue}", queueUrl);

        try
        {
            using var client = GetClient();
            var response = await client.ReceiveMessageAsync(request);

            if (response == null || response.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception($"Error during request messages. Status code - {response?.HttpStatusCode}");
            }

            if (response.Messages?.Any() == true)
            {
                var consumedList = new List<QueueMessageConsumed>();

                foreach (var message in response.Messages)
                {
                    consumedList.Add(message.ConvertToMessageConsumed());
                }

                return consumedList;
            }

            return new List<QueueMessageConsumed>();

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "SqsMessageConsumer (ReceiveMessageAsync) error: {msg}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Asynchronously removes a batch of consumed messages from the SQS queue using their receipt handles.
    /// </summary>
    /// <param name="messages">A collection of <see cref="QueueMessageConsumed"/> objects containing receipt handles of messages to delete.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous deletion operation.</returns>
    /// <exception cref="ReceiptHandleIsInvalidException">
    /// Thrown when a receipt handle is expired or malformed.
    /// </exception>
    /// <exception cref="AmazonSQSException">
    /// Thrown when an error occurs while communicating with the AWS SQS service during message deletion.
    /// </exception>
    public async Task RemoveMassagesAsync(IList<QueueMessageConsumed> messages, CancellationToken cancellationToken)
    {
        var queueUrl = await GetQueueUrlAsync();

        try
        {
            using var client = GetClient();

            foreach (var message in messages)
            {
                await client.DeleteMessageAsync(new DeleteMessageRequest
                {
                    QueueUrl = queueUrl,
                    ReceiptHandle = message.ReceiptHandle
                }, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "SqsMessageConsumer (RemoveMassagesAsync) error: {msg}", ex.Message);
            throw;
        }
    }


    #region private functions and methods

    /// <summary>
    /// Initializes and returns a configured <see cref="AmazonSQSClient"/> instance using current application options.
    /// </summary>
    /// <returns>
    /// A new instance of <see cref="AmazonSQSClient"/> configured with explicit credentials, custom service endpoint, 
    /// and authentication region.
    /// </returns>
    public AmazonSQSClient GetClient()
    {
        var value = options.Value;
        return new AmazonSQSClient(value.AccessKeyId, value.SecretAccessKey, new AmazonSQSConfig
        {
            ServiceURL = options.Value.EndPoint,
            AuthenticationRegion = options.Value.Region
        });
    }

    /// <summary>
    /// Asynchronously retrieves the canonical SQS queue URL for the configured queue name.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the resolved queue URL <see cref="string"/>.
    /// </returns>
    /// <exception cref="QueueDoesNotExistException">
    /// Thrown when the queue specified by <c>options.Value.SqsQueueName</c> does not exist on the endpoint.
    /// </exception>
    /// <exception cref="AmazonSQSException">
    /// Thrown when an error occurs while communicating with the AWS SQS service.
    /// </exception>
    private async Task<string> GetQueueUrlAsync()
    {
        try
        {
            using var client = GetClient();
            var response = await client.GetQueueUrlAsync(new GetQueueUrlRequest
            {
                QueueName = options.Value.SqsQueueName,
            });

            return response.QueueUrl;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "SqsMessageConsumer (GetQueueUrlAsync) error: {msg}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Constructs and initializes a <see cref="ReceiveMessageRequest"/> configured for single-message polling from SQS.
    /// </summary>
    /// <param name="queueUrl">The absolute URL of the target SQS queue.</param>
    /// <returns>
    /// A <see cref="ReceiveMessageRequest"/> instance populated with queue parameters, system attributes, and custom message attributes.
    /// </returns>
    private ReceiveMessageRequest GetConsumeRequest(string queueUrl)
    {
        return new ReceiveMessageRequest
        {
            MessageSystemAttributeNames = new List<string> { "SentTimestamp" },
            MaxNumberOfMessages = 1,
            MessageAttributeNames = new List<string> { "All" },
            QueueUrl = queueUrl,
            VisibilityTimeout = 0,
            WaitTimeSeconds = 0,
        };
    }

    #endregion
}


/*
{
    "Records": [
        {
        "eventVersion": "2.1",
        "eventSource": "aws:s3",
        "awsRegion": "us-east-1",
        "eventTime": "2026-09-27T18:56:19.488834316Z",
        "eventName": "ObjectCreated:Put",
        "userIdentity": {
            "principalId": "AWS:EMULATOR"
        },
        "requestParameters": {
            "sourceIPAddress": "127.0.0.1"
        },
        "responseElements": {
            "x-amz-request-id": "3654b70a-33fb-4575-91df-13dfb1f83149"
        },
        "s3": {
            "s3SchemaVersion": "1.0",
            "configurationId": "emulator",
            "bucket": {
            "name": "file-processing-bucket",
            "arn": "arn:aws:s3:::file-processing-bucket"
            },
            "object": {
            "key": "sample",
            "size": 6436706,
            "eTag": "85c5e8ce14d054fff496522620989380"
            }
        }
    }

*/