using AutoMapper;
using FluentValidation;
using MediatR;
using MediatRApi.ApplicationCore.Common.Attributes;
using MediatRApi.ApplicationCore.Common.Exceptions;
using MediatRApi.ApplicationCore.Common.Helpers;
using MediatRApi.ApplicationCore.Domain;
using MediatRApi.ApplicationCore.Infrastructure.Persistence;

namespace MediatRApi.ApplicationCore.Features.Products.Commands;

[AuditLog]
public class UpdateProductCommand : IRequest
{
    public string ProductId { get; set; } = default!;
    public string Description { get; set; } = default!;
    public decimal Price { get; set; }
}

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand>
{
    private readonly MyAppDbContext _context;
    private readonly IMapper _mapper;

    public UpdateProductCommandHandler(MyAppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Unit> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products.FindAsync(request.ProductId.FromSqids());

        if (product is null)
        {
            throw new NotFoundException(nameof(Product), request.ProductId);
        }

        _mapper.Map(request, product);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

public class UpdateProductValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductValidator()
    {
        RuleFor(r => r.ProductId).NotNull().NotEmpty();
        RuleFor(r => r.Description).NotNull().NotEmpty();
        RuleFor(r => r.Price).NotNull().GreaterThan(0).NotEmpty();
    }
}

public class UpdateProductProfile : Profile
{
    public UpdateProductProfile()
    {
        CreateMap<UpdateProductCommand, Product>()
            .ForMember(dest => dest.ProductId, opt => opt.Ignore());
    }
}