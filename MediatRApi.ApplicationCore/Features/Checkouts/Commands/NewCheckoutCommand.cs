using FluentValidation;
using FluentValidation.Results;
using MediatR;
using MediatRApi.ApplicationCore.Common.Attributes;
using MediatRApi.ApplicationCore.Common.Helpers;
using MediatRApi.ApplicationCore.Common.Interfaces;
using MediatRApi.ApplicationCore.Common.Messages;
using MediatRApi.ApplicationCore.Common.Services;
using MediatRApi.ApplicationCore.Domain;
using MediatRApi.ApplicationCore.Infrastructure.Persistence;

namespace MediatRApi.ApplicationCore.Features.Checkouts.Commands;

[AuditLog]
public class NewCheckoutCommand : IRequest
{
    public List<NewCheckoutItems> Products { get; set; } = [];

    public class NewCheckoutItems
    {
        public string ProductId { get; set; } = default!;
        public int Quantity { get; set; }
    }
}

public class NewCheckoutCommandHandler : IRequestHandler<NewCheckoutCommand>
{
    private readonly MyAppDbContext _context;
    private readonly CurrentUser _currentUserService;
    private readonly IQueuesService _queuesService;

    public NewCheckoutCommandHandler(
        MyAppDbContext context,
        ICurrentUserService currentUserService,
        IQueuesService queuesService)
    {
        _context = context;
        _currentUserService = currentUserService.User;
        _queuesService = queuesService;
    }

    public async Task<Unit> Handle(NewCheckoutCommand request, CancellationToken cancellationToken)
    {
        var newCheckout = await CreateCheckoutAsync(request, cancellationToken);

        await QueueCheckoutAsync(newCheckout, cancellationToken);

        return Unit.Value;
    }

    private async Task<Checkout> CreateCheckoutAsync(NewCheckoutCommand request, CancellationToken cancellationToken)
    {
        var newCheckout = new Checkout
        {
            CheckoutDateTime = DateTime.UtcNow,
            UserId = _currentUserService.Id,
            Total = 0
        };

        foreach (var item in request.Products)
        {
            var product = await _context
                .Products
                .FindAsync(item.ProductId.FromSqids(), cancellationToken);

            if (product is null)
            {
                throw new ValidationException(new List<ValidationFailure>
                {
                    new ValidationFailure("Error", $"Product {item.ProductId} not found")
                });
            }

            var newProduct = new CheckoutProduct
            {
                ProductId = product.ProductId,
                Quantity = item.Quantity,
                UnitPrice = product.Price,
                Total = item.Quantity * product.Price
            };

            newCheckout.CheckoutItems.Add(newProduct);
        }

        newCheckout.Total = newCheckout.CheckoutItems.Sum(x => x.Total);

        _context.Checkouts.Add(newCheckout);

        await _context.SaveChangesAsync(cancellationToken);

        return newCheckout;
    }

    private async Task QueueCheckoutAsync(Checkout checkout, CancellationToken cancellationToken)
    {
        await _queuesService
            .QueueAsync("new-checkouts", new NewCheckoutMessage
            {
                CheckoutId = checkout.CheckoutId
            });
    }
}