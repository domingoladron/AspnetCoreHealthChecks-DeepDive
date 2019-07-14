using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace HealthChecks.ServiceBus.Base
{
    /// <summary>
    /// The SqsListener represents a long-running service in your API which is
    /// SQS queue aware.  It will listen to messages of type T which are routed on the queue
    /// Remember that this contract is OWNED by the API, and should not be a shared contract between the
    /// recipient and sender, as this leads to tight-coupling.
    /// </summary>
    /// <typeparam name="T">The type of class to receive from your queue</typeparam>
    public abstract class SqsListener<T> : IHostedService, IDisposable where T: class
    {
        private readonly string _queueName;
        protected readonly ILogger<SqsListener<T>> Logger;
        private readonly IAmazonSQS _sqsClient;
        private GetQueueUrlResponse _queueDetails;

        public bool Shutdown { get; set; }
        public bool Started { get; set; }

        public abstract Task HandleMessageAsync(
            T constructedMessage);

        protected SqsListener(
            string queueName,
            IAmazonSQS sqsClient,
            ILogger<SqsListener<T>> logger)
        {
            Logger = logger;
            _queueName = queueName;
            _sqsClient = sqsClient;
        }

        #region Implementation of IHostedService

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.Run(StartListeningAsync, cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Shutdown = true;
            return Task.CompletedTask;
        }

        #endregion

        protected void GetQueueDetailsFromAws()
        {
            Logger.LogDebug("Getting SQS queue URL...");
            _queueDetails = _sqsClient.GetQueueUrlAsync(_queueName).Result;
            Logger.LogDebug($"SQS queue URL retrieved: {_queueDetails.QueueUrl}");
        }

        public async Task StartListeningAsync()
        {
            Logger.LogDebug("Starting SQS listener...");

            GetQueueDetailsFromAws();

            var request = new ReceiveMessageRequest
            {
                AttributeNames = new List<string> { "All" },
                MaxNumberOfMessages = 5,
                QueueUrl = _queueDetails.QueueUrl,
                VisibilityTimeout = (int)TimeSpan.FromMinutes(2).TotalSeconds,
                WaitTimeSeconds = (int)TimeSpan.FromSeconds(20).TotalSeconds
            };

            Started = true;

            while (!Shutdown)
            {
                Logger.LogDebug("Checking for new SQS messages...");
                var response = _sqsClient.ReceiveMessageAsync(request).Result;

                if (response.Messages.Count > 0)
                {
                    Logger.LogDebug($"Received {response.Messages.Count} messages from SQS");
                    foreach (var message in response.Messages)
                    {
                        Logger.LogDebug($"Processing message {message.MessageId}");
                        Logger.LogDebug($"Message contents: {message.Body.Replace("{", "{{").Replace("}", "}}")}");

                        T constructedMessage;
                        try
                        {
                            Logger.LogDebug($"Deserialising message body...");
                             constructedMessage = JsonConvert.DeserializeObject<T>(message.Body);
                            Logger.LogDebug($"Deserialised message {message.MessageId} successfully.");
                            Logger.LogDebug("Sanity check re-serialised:");
                            Logger.LogDebug(JsonConvert.SerializeObject(constructedMessage).Replace("{", "{{").Replace("}", "}}"));
                        }
                        catch (JsonReaderException ex)
                        {
                            Logger.LogError(ex, "Unknown message type.");

                            // Bad message, delete it from the queue.
                            Logger.LogError($"Deleting bad message from queue with ID {message.MessageId}");
                            Logger.LogError("Message body:");
                            Logger.LogError(message.Body);
                            var deleteMessageRequest = new DeleteMessageRequest(_queueDetails.QueueUrl, message.ReceiptHandle);
                            _sqsClient.DeleteMessageAsync(deleteMessageRequest).Wait();

                            continue;
                        }

                        try
                        {
                            await HandleMessageAsync(constructedMessage);
                            // Message handled, delete it from the queue.
                            var deleteMessageRequest = new DeleteMessageRequest(_queueDetails.QueueUrl, message.ReceiptHandle);
                            await _sqsClient.DeleteMessageAsync(deleteMessageRequest);
                        }
                        catch (Exception ex)
                        {
                            Logger.LogDebug($"Processing of message failed.  Release message {message.MessageId} back to queue.");
                            Logger.LogDebug($"Error was: {ex.Message}");
                            Logger.LogDebug(ex.StackTrace);

                            var changeMessageRequest = new ChangeMessageVisibilityRequest(_queueDetails.QueueUrl, message.ReceiptHandle, 0);
                            await _sqsClient.ChangeMessageVisibilityAsync(changeMessageRequest);
                        }
                    }
                }
                else
                {
                    Logger.LogDebug("No sqs messages received.");
                }
            }

            Logger.LogDebug("Shutting down");
            Started = false;

            await Task.CompletedTask;
        }

        public void Dispose()
        {
            _sqsClient?.Dispose();
        }
    }
}
