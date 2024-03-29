using MediatR;
using MediatRApi.ApplicationCore.Common.Interfaces;
using MediatRApi.ApplicationCore.Features.Auth.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediatRApi.WebApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<TokenCommandResponse> Token([FromBody] TokenCommand command)
    {
        return await _mediator.Send(command);
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me([FromServices] ICurrentUserService currentUser)
    {
        return Ok(new
        {
            currentUser.User,
            IsAdmin = currentUser.IsInRole("Admin")
        });
    }

    [HttpPost("refresh")]
    public async Task<RefreshTokenCommandResponse> Refresh([FromBody] RefreshTokenCommand command)
    {
        return await _mediator.Send(command);
    }
}