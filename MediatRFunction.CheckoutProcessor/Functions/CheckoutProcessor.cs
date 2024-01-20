using MediatRApi.ApplicationCore.Common.Messages;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using MediatRApi.ApplicationCore.Features.Checkouts.Commands;
using MediatR;

namespace MediatRFunction.CheckoutProcessor.Functions
{
    public class CheckoutProcessor
    {
        private readonly ILogger<CheckoutProcessor> _logger;
        private readonly IMediator _mediator;

        public CheckoutProcessor(ILogger<CheckoutProcessor> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        [Function(nameof(CheckoutProcessor))]
        public async Task Run([QueueTrigger("new-checkouts", Connection = "AzureWebJobsStorage")] string myQueueItem)
        {
            _logger.LogInformation("Nuevo mensaje recibido: {Message}", myQueueItem);
            _logger.LogInformation("Procesando...");

            var message = JsonSerializer.Deserialize<NewCheckoutMessage>(myQueueItem);

            if (message is null)
            {
                _logger.LogError("El mensaje es nulo");
                throw new ArgumentNullException(nameof(NewCheckoutMessage));
            }

            await _mediator.Send(new ProcessCheckoutCommand
            {
                CheckoutId = message.CheckoutId
            });
        }
    }
}
