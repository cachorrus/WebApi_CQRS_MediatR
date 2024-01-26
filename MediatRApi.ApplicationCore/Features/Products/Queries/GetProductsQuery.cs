using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using MediatRApi.ApplicationCore.Common.Extensions;
using MediatRApi.ApplicationCore.Common.Helpers;
using MediatRApi.ApplicationCore.Common.Models;
using MediatRApi.ApplicationCore.Domain;
using MediatRApi.ApplicationCore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MediatRApi.ApplicationCore.Features.Products.Queries;

public class GetProductsQuery : QueryStringPagination, IRequest<PagedResult<GetProductsQueryResponse>>
{

}

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, PagedResult<GetProductsQueryResponse>>
{
    private readonly MyAppDbContext _context;
    private readonly IMapper _mapper;

    public GetProductsQueryHandler(MyAppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public Task<PagedResult<GetProductsQueryResponse>> Handle(GetProductsQuery request, CancellationToken cancellationToken) =>
        _context.Products
            .AsNoTracking()
            .OrderBy($"{request.SortProperty} {request.SortDir}", nameof(Product.ProductId))
            .AsQueryable()
            .ProjectTo<GetProductsQueryResponse>(_mapper.ConfigurationProvider)
            .GetPagedResultAsync(request.PageSize, request.CurrentPage);
}

public class GetProductsQueryResponse
{
    public string ProductId { get; set; } = default!;
    public string Description { get; set; } = default!;
    public decimal Price { get; set; }
    public string ListDescription { get; set; } = default!;
}

public class GetProductsQueryProfile : Profile
{
    public GetProductsQueryProfile()
    {
        CreateMap<Product, GetProductsQueryResponse>()
            .ForMember(dest =>
                dest.ListDescription,
                opt => opt.MapFrom(src => $"{src.Description} - {src.Price}")) // CurrencySymbol Invariant https://github.com/dotnet/runtime/blob/main/docs/design/features/globalization-invariant-mode.md
            .ForMember(dest =>
                dest.ProductId,
                opt => opt.MapFrom(src => src.ProductId.ToSqids()));
    }
}
