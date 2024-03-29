using MediatR;
using MediatRApi.ApplicationCore.Common.Models;
using MediatRApi.ApplicationCore.Features.Products.Commands;
using MediatRApi.ApplicationCore.Features.Products.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediatRApi.WebApi.Controllers;

[Authorize]
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
    public async Task<PagedResult<GetProductsQueryResponse>> GetProducts([FromQuery] GetProductsQuery query)
    {
        return await _mediator.Send(query);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
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

    [HttpPut("{ProductId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateProduct([FromBody] UpdateProductCommand command, [FromRoute] string ProductId)
    {
        command.ProductId = ProductId;

        await _mediator.Send(command);

        return Ok();
    }

    [HttpDelete("{ProductId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteProduct([FromRoute] DeleteProductCommand command)
    {
        await _mediator.Send(command);

        return Ok();
    }
}