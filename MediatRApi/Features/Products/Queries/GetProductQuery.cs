using MediatR;
using MediatRApi.Infrastructure.Persistence;

namespace MediatRApi.Features.Products.Queries;

public class GetProductQuery: IRequest<GetProductQueryResponse>
{
    public int ProductId { get; set; }
}

public class GetProductQueryHandler : IRequestHandler<GetProductQuery, GetProductQueryResponse>
{
    private readonly MyAppDbContext _context;

    public GetProductQueryHandler(MyAppDbContext context)
    {
        _context = context;
    }

    public async Task<GetProductQueryResponse> Handle(GetProductQuery request, CancellationToken cancellationToken)
    {
        var product = await _context.Products.FindAsync(request.ProductId);

        if(product == null)
        {
            throw new Exception("Product not found");
        }

        return new GetProductQueryResponse
        {
            ProductId = product.ProductId,
            Description = product.Description,
            Price = product.Price
        };
    }
}

public class GetProductQueryResponse
{
    public int ProductId { get; set; }
    public string Description { get; set; } = default!;
    public decimal Price { get; set; }
}