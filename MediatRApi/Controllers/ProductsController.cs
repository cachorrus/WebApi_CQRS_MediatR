using MediatR;
using MediatRApi.Features.Products.Commands;
using MediatRApi.Features.Products.Queries;
using Microsoft.AspNetCore.Mvc;

namespace MediatRApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<List<GetProductsQueryResponse>> GetProducts()
    {
        return await _mediator.Send(new GetProductsQuery());
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductCommand command)
    {
         await _mediator.Send(command);

         return Ok();
    }

    [HttpGet("{ProductId}")]
    public async Task<GetProductQueryResponse> GetProductById([FromRoute] GetProductQuery query)
    {
        return await _mediator.Send(query);
    }
}