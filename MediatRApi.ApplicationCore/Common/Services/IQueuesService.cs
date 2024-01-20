namespace MediatRApi.ApplicationCore.Common.Services;

public interface IQueuesService
{
    Task QueueAsync<T>(string queueName, T message);
}