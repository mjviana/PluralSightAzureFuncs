using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.ConfigureCosmosDBExtensionOptions(options =>
    options.ClientOptions = new Microsoft.Azure.Cosmos.CosmosClientOptions()
    {
        SerializerOptions = new Microsoft.Azure.Cosmos.CosmosSerializationOptions()
        {
            PropertyNamingPolicy = Microsoft.Azure.Cosmos.CosmosPropertyNamingPolicy.CamelCase
        }
    });

builder.Build().Run();
