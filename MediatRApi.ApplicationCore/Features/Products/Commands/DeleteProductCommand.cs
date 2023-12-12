using MediatR;
using MediatRApi.ApplicationCore.Common.Attributes;
using MediatRApi.ApplicationCore.Common.Exceptions;
using MediatRApi.ApplicationCore.Common.Helpers;
using MediatRApi.ApplicationCore.Domain;
using MediatRApi.ApplicationCore.Infrastructure.Persistence;

namespace MediatRApi.ApplicationCore.Features.Products.Commands;

[AuditLog]
public class DeleteProductCommand : IRequest
{
    public string ProductId { get; set; } = default!;
}

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand>
{
    private readonly MyAppDbContext _context;
    public DeleteProductCommandHandler(MyAppDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products.FindAsync(request.ProductId.FromSqids());

        if (product is null)
        {
            throw new NotFoundException(nameof(Product), request.ProductId);
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}