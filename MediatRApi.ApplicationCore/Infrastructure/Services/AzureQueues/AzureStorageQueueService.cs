using System.Text.Json;
using Azure.Storage.Queues;
using MediatRApi.ApplicationCore.Common.Services;
using Microsoft.Extensions.Configuration;

namespace MediatRApi.ApplicationCore.Infrastructure.Services.AzureQueues;

public class AzureStorageQueueService : IQueuesService
{
    private readonly IConfiguration _configuration;

    public AzureStorageQueueService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task QueueAsync<T>(string queueName, T item)
    {
        var queueClient = new QueueClient(
            _configuration.GetConnectionString("StorageAccount"),
            queueName,
            new QueueClientOptions {
                MessageEncoding = QueueMessageEncoding.Base64
            }
        );

        await queueClient.CreateIfNotExistsAsync();

        var message = JsonSerializer.Serialize(item);

        await queueClient.SendMessageAsync(message);
    }
}