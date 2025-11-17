using System;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

using Azure.Storage.Queues.Models;

namespace Demo.AzureStorageTrigger
{
    public record NewOrderMessage(Guid orderId, int productId, int quantityId, string customerName,
        string customerEmail, decimal purchasePrice);

    public class ProcessNewOrder
    {
        private readonly ILogger<ProcessNewOrder> _logger;

        public ProcessNewOrder(ILogger<ProcessNewOrder> logger)
        {
            _logger = logger;
        }

        [Function(nameof(ProcessNewOrder))]
        [BlobOutput("tickets/{orderId}.txt", Connection = "AzureWebJobsStorage")]
        public string Run([QueueTrigger("neworders", Connection = "AzureWebJobsStorage")]
            NewOrderMessage message)
        {
            _logger.LogInformation("C# Queue trigger function processed: {customerName} bought {productId} orderId:{orderId}",
                message.customerName, message.productId, message.orderId);

            var description = $"Order {message.orderId}: {message.customerName} bought {message.productId}";
            _logger.LogInformation(description);

            return description;
        }
    }
}