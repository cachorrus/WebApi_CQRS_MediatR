using AutoMapper;
using FluentValidation;
using MediatR;
using MediatRApi.ApplicationCore.Common.Attributes;
using MediatRApi.ApplicationCore.Domain;
using MediatRApi.ApplicationCore.Infrastructure.Persistence;

namespace MediatRApi.ApplicationCore.Features.Products.Commands;

[AuditLog]
public class CreateProductCommand : IRequest
{
    public string Description { get; set; } = default!;
    public decimal Price { get; set; }
}

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand>
{
    private readonly MyAppDbContext _context;
    private readonly IMapper _mapper;

    public CreateProductCommandHandler(MyAppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Unit> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var newProduct = _mapper.Map<Product>(request);

        _context.Products.Add(newProduct);

        await _context.SaveChangesAsync();

        return Unit.Value;
    }
}

public class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
    {
        RuleFor(r => r.Description).NotNull().NotEmpty();
        RuleFor(r => r.Price).NotNull().GreaterThan(0);
    }
}

public class CreateProductCommandProfile : Profile
{
    public CreateProductCommandProfile()
    {
        CreateMap<CreateProductCommand, Product>();
    }
}