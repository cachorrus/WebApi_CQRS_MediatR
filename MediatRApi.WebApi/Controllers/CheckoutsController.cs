using MediatR;
using MediatRApi.ApplicationCore.Features.Checkouts.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediatRApi.WebApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CheckoutsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CheckoutsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Crea una nueva orden de compra (productos)
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> Checkout([FromBody] NewCheckoutCommand command)
    {
        await _mediator.Send(command);

        return Accepted();
    }
}