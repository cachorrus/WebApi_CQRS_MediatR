using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MediatR;
using MediatRApi.ApplicationCore.Common.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace MediatRApi.ApplicationCore.Features.Auth.Commands;

public class TokenCommand : IRequest<TokenCommandResponse>
{
    public string UserName { get; set; } = default!;
    public string Password { get; set; } = default!;
}

public class TokenCommandHandler : IRequestHandler<TokenCommand, TokenCommandResponse>
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IConfiguration _config;

    public TokenCommandHandler(UserManager<IdentityUser> userManager, IConfiguration config)
    {
        _userManager = userManager;
        _config = config;
    }

    public async Task<TokenCommandResponse> Handle(TokenCommand request, CancellationToken cancellationToken)
    {
        // Verificamos las credenciales con Identity
        var user = await _userManager.FindByNameAsync(request.UserName);

        if (user is null || !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            throw new ForbiddenAccessException();
        }

        var roles = await _userManager.GetRolesAsync(user);

        // Generamos un token en función de los claims
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Sid, user.Id),
            new Claim(ClaimTypes.Name, user.UserName!)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var tokenDescrpitor = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(720),
            signingCredentials: credentials);

        var jwt = new JwtSecurityTokenHandler().WriteToken(tokenDescrpitor);

        return new TokenCommandResponse
        {
            AccessToken = jwt
        };
    }
}

public class TokenCommandResponse
{
    public string AccessToken { get; set; } = default!;
}