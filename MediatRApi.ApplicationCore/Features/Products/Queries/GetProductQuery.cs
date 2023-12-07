using AutoMapper;
using MediatR;
using MediatRApi.ApplicationCore.Common.Exceptions;
using MediatRApi.ApplicationCore.Common.Helpers;
using MediatRApi.ApplicationCore.Domain;
using MediatRApi.ApplicationCore.Infrastructure.Persistence;

namespace MediatRApi.ApplicationCore.Features.Products.Queries;

public class GetProductQuery: IRequest<GetProductQueryResponse>
{
    public string ProductId { get; set; } = default!;
}

public class GetProductQueryHandler : IRequestHandler<GetProductQuery, GetProductQueryResponse>
{
    private readonly MyAppDbContext _context;
    private readonly IMapper _mapper;

    public GetProductQueryHandler(MyAppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<GetProductQueryResponse> Handle(GetProductQuery request, CancellationToken cancellationToken)
    {
        var product = await _context.Products.FindAsync(request.ProductId.FromSqids());

        if (product is null)
        {
            throw new NotFoundException(nameof(Product), request.ProductId);
        }

        return _mapper.Map<GetProductQueryResponse>(product);
    }
}

public class GetProductQueryResponse
{
    public string ProductId { get; set; } = default!;
    public string Description { get; set; } = default!;
    public decimal Price { get; set; }
}

public class GetProductQueryProfile : Profile
{
    public GetProductQueryProfile()
    {
        CreateMap<Product, GetProductQueryResponse>()
            .ForMember(dest =>
                dest.ProductId,
                opt => opt.MapFrom(src => src.ProductId.ToSqids()));
    }
}