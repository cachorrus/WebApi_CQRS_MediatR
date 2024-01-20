using MediatR;
using MediatRApi.ApplicationCore.Common.Attributes;
using MediatRApi.ApplicationCore.Common.Exceptions;
using MediatRApi.ApplicationCore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MediatRApi.ApplicationCore.Features.Checkouts.Commands;

[AuditLog]
public class ProcessCheckoutCommand : IRequest
{
    public int CheckoutId { get; set; } = default!;
}

public class ProcessCheckoutCommandHandler : IRequestHandler<ProcessCheckoutCommand>
{
    private readonly MyAppDbContext _context;
    private readonly ILogger<ProcessCheckoutCommandHandler> _logger;

    public ProcessCheckoutCommandHandler(MyAppDbContext context, ILogger<ProcessCheckoutCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Unit> Handle(ProcessCheckoutCommand request, CancellationToken cancellationToken)
    {
        var checkout = await _context
            .Checkouts
            .Include(x => x.CheckoutItems)
            .FirstOrDefaultAsync(x => x.CheckoutId == request.CheckoutId);

        if (checkout is null)
        {
            throw new NotFoundException();
        }

        _logger.LogInformation("Nueva orden recibida con el Id {CheckoutId}", checkout.CheckoutId);
        _logger.LogInformation("El usuario {UserId} ordenó {ProductsCount} productos", checkout.UserId, checkout.CheckoutItems.Count());

        //Working
        _logger.LogInformation("Realizando cobro...");
        await Task.Delay(5000);
        _logger.LogInformation("Cobro realizado");

        checkout.Processed = true;
        checkout.ProcessedDateTime = DateTime.UtcNow;

        _logger.LogInformation("Se procesó una orden con costo total de {Total}", checkout.Total);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}