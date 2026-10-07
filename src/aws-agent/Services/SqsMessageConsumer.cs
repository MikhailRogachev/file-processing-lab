using aws_agent.Extensions;
using domain.Extensions;

namespace aws_agent.Services;

public class SqsMessageConsumer : IMessageConsumer<AmazonSQSClient>
{
    private readonly ILogger<SqsMessageConsumer> _logger;
    private readonly string _queueName;
    private readonly IAmazonSQS _client;

    private string? _cachedQueueUrl;

    public SqsMessageConsumer(
        ILogger<SqsMessageConsumer> logger,
        IAwsClientConnectionFactory awsClientFactory
        )
    {
        _logger = logger;
        _queueName = awsClientFactory.SqsQueueName;
        _client = awsClientFactory.SqsClient();
    }

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
        try
        {
            // get sqs queue url
            var queueUrl = await GetQueueUrlAsync(cancellationToken);
            var request = GetConsumeRequest(queueUrl);

            _logger.LogDebug("Requesting messages from the SQS queue - {queue}", queueUrl);

            var response = await _client.ReceiveMessageAsync(request, cancellationToken);

            if (response == null || response.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception($"Error during request messages. Status code - {response?.HttpStatusCode}");
            }

            if (response.Messages.IsAny())
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
            _logger.LogError(ex, "SqsMessageConsumer (ReceiveMessageAsync) error: {msg}", ex.Message);
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
    public async Task RemoveMessagesAsync(IList<QueueMessageConsumed> messages, CancellationToken cancellationToken)
    {
        if (!messages.IsAny())
            return;

        try
        {
            var queueUrl = await GetQueueUrlAsync(cancellationToken);
            foreach (var chunk in messages.Chunk(10))
            {
                var deleteRequest = new DeleteMessageBatchRequest
                {
                    QueueUrl = queueUrl,
                    Entries = chunk.Select(m => new DeleteMessageBatchRequestEntry
                    {
                        Id = m.MessageId ?? Guid.NewGuid().ToString(),
                        ReceiptHandle = m.ReceiptHandle
                    }).ToList()
                };

                var response = await _client.DeleteMessageBatchAsync(deleteRequest, cancellationToken);

                if (response.Failed?.Count > 0)
                {
                    _logger.LogWarning("Failed to delete {Count} messages from SQS queue", response.Failed.Count);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SqsMessageConsumer (RemoveMassagesAsync) error: {msg}", ex.Message);
            throw;
        }
    }


    #region private functions and methods

    /// <summary>
    /// Asynchronously retrieves and caches the canonical Amazon SQS queue URL for the configured queue name.
    /// </summary>
    /// <remarks>
    /// This method uses in-memory caching to store the resolved queue URL upon initial retrieval, 
    /// avoiding redundant API calls on subsequent invocations. 
    /// <para>
    /// <strong>Note:</strong> The provided <paramref name="cancellationToken"/> is reserved for future signature compliance 
    /// or downstream thread propagation; the underlying AWS S3/SQS client call in this method does not currently observe it.
    /// </para>
    /// </remarks>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the resolved 
    /// SQS queue URL as a <see cref="string"/>.
    /// </returns>
    /// <exception cref="Amazon.SQS.Model.QueueDoesNotExistException">
    /// Thrown when the queue specified by <c>options.Value.SqsQueueName</c> does not exist on the target AWS endpoint.
    /// </exception>
    /// <exception cref="Amazon.SQS.AmazonSQSException">
    /// Thrown when an error occurs while communicating with the Amazon SQS service.
    /// </exception>
    /// <exception cref="System.InvalidOperationException">
    /// Thrown when the configured queue name or AWS credentials/options are invalid or uninitialized.
    /// </exception>
    private async Task<string> GetQueueUrlAsync(CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(_cachedQueueUrl))
            return _cachedQueueUrl;

        var response = await _client.GetQueueUrlAsync(new GetQueueUrlRequest
        {
            QueueName = _queueName,
        });

        _cachedQueueUrl = response.QueueUrl;

        return _cachedQueueUrl;
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
            WaitTimeSeconds = 20,
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