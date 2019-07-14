using System;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace HealthChecks.ServiceBus.Base
{
    /// <summary>
    /// A sender of messages to an SQS queue of type T.  
    /// </summary>
    /// <typeparam name="T">The type of class to send to your queue</typeparam>
    public abstract class SqsSender<T>
        where T : class
    {
        private readonly string _queueName;
        private readonly ILogger<SqsListener<T>> _logger;
        private readonly IAmazonSQS _sqsClient;
        private GetQueueUrlResponse _queueDetails;

        protected SqsSender(
            string queueName,
            IAmazonSQS amazonSqs,
            ILogger<SqsListener<T>> logger)
        {
            _queueName = queueName;
            _sqsClient = amazonSqs;
            _logger = logger;
            GetQueueDetailsFromAwsAsync().Wait();
        }

        protected async Task GetQueueDetailsFromAwsAsync()
        {
            try { 
                _logger.LogDebug("Getting SQS queue URL...");
                _queueDetails = await _sqsClient.GetQueueUrlAsync(_queueName);
                _logger.LogDebug($"SQS queue URL retrieved: {_queueDetails.QueueUrl}");
            } catch(Exception ex)
            {
                throw ex;
            }
        }


        public async Task Send(T message)
        {
            var messageBody = JsonConvert.SerializeObject(message);
            _logger.LogDebug($"Sending SQS message: {messageBody} on queue {_queueDetails.QueueUrl}");
            var sendMessageRequest = new SendMessageRequest
            {
                MessageBody = messageBody,
                QueueUrl = _queueDetails.QueueUrl
            };

            await _sqsClient.SendMessageAsync(sendMessageRequest);
        }
    }
}