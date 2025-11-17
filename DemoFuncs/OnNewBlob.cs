using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Demo.BlobTrigger;

public class OnNewBlob
{
    private readonly ILogger<OnNewBlob> _logger;

    public OnNewBlob(ILogger<OnNewBlob> logger)
    {
        _logger = logger;
    }

    [Function(nameof(OnNewBlob))]
    public async Task Run([BlobTrigger("tickets/{name}", Connection = "AzureWebJobsStorage")] 
        Stream stream, string name)
    {
        using var blobStreamReader = new StreamReader(stream);
        var content = await blobStreamReader.ReadToEndAsync();
        _logger.LogInformation("C# Blob trigger function Processed blob\n Name: {name} \n Data: {content}", name, content);
    }

     [Function(nameof(OnNewBlob2))]
    public async Task OnNewBlob2([BlobTrigger("tickets2/{name}", Connection = "AzureWebJobsStorage")] 
        BlobClient blobClient, string name)
    {
        var content = (await blobClient.DownloadContentAsync()).Value.Content.ToArray();
        var props = await blobClient.GetPropertiesAsync();
        
        _logger.LogInformation("C# Blob trigger function Processed blob\n Name: {name} \n" + 
            "Data: {content} \n" +
            "Last Modified: {lastModified}. " +
            "Content length: {contentLength}. " +
            "Content type: {contentType}.", 
            name, content, props.Value.LastModified, props.Value.ContentLength, props.Value.ContentType);
    }
}