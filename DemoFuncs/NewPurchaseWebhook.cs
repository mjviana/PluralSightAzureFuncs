using Azure;
using Azure.Storage.Blobs;
using Demo.AzureStorageTrigger;
using HttpTriggeredFuncs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Demo.HttpTrigger
{
    public class NewPurchaseWebhookResponse
    {
        [QueueOutput("neworders", Connection = "AzureWebJobsStorage")]
        public NewOrderMessage? Message { get; set; }
        [HttpResult]
        public IActionResult? ActionResult { get; set; }

        [CosmosDBOutput("azurefuncs", "orders", Connection = "CosmosDbConnection")]
        public OrderDocument? OrderDocument { get; set; }
    }

    public class NewPurchaseWebhook
    {
        private readonly ILogger<NewPurchaseWebhook> _logger;

        public NewPurchaseWebhook(ILogger<NewPurchaseWebhook> logger)
        {
            _logger = logger;
        }

        record NewOrderWebhook(int productId, int quantity,
                string customerName, string customerEmail, decimal purchasePrice);

        [Function(nameof(NewPurchaseWebhook))]
        public async Task<NewPurchaseWebhookResponse> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "purchase")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var order = await req.ReadFromJsonAsync<NewOrderWebhook>();

            var message = new NewOrderMessage(
                Guid.NewGuid(), // orderId
                order.productId,
                order.quantity,
                order.customerName,
                order.customerEmail,
                order.purchasePrice
            );

            var document = new OrderDocument()
            {
                Id = message.orderId.ToString(),
                ProductId = order.productId,
                Quantity = order.quantity,
                CustomerEmail = order.customerEmail,
                CustomerName = order.customerName
            };

            return new NewPurchaseWebhookResponse
            {
                Message = message,
                ActionResult = new OkObjectResult($"{order?.customerName} purchased product {order?.productId}!"),
                OrderDocument = document
            };
        }

        [Function(nameof(GetPurchase))]
        public IActionResult GetPurchase(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "purchase/{orderId:guid}")] HttpRequest req,
            [BlobInput("tickets/{orderId}.txt", Connection = "AzureWebJobsStorage")] BlobClient ticketClient,
            Guid orderId)
        {
            _logger.LogInformation("Requested details of {orderId}", orderId);

            try
            {
                var ticketContents = ticketClient.DownloadContent().Value.Content.ToString();
                return new OkObjectResult(ticketContents);
            }
            catch (RequestFailedException rfe) when (rfe.ErrorCode == "BlobNotFound")
            {
                _logger.LogError(rfe, "Order {orderId} does not exist", orderId);
                return new NotFoundResult();
            }
        }
    }
}