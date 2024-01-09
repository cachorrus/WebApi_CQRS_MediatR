using MediatR;
using MediatRApi.ApplicationCore.Common.Exceptions;
using MediatRApi.ApplicationCore.Common.Services;
using MediatRApi.ApplicationCore.Domain;
using MediatRApi.ApplicationCore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MediatRApi.ApplicationCore.Features.Auth.Commands;

public class RefreshTokenCommand : IRequest<RefreshTokenCommandResponse>
{
    public string AccessToken { get; set; } = default!;
    public string RefreshToken { get; set; } = default!;
}

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, RefreshTokenCommandResponse>
{
    private readonly MyAppDbContext _context;
    private readonly AuthService _authService;
    private readonly ILogger<RefreshTokenCommand> _logger;

    public RefreshTokenCommandHandler(
        MyAppDbContext context,
        AuthService authService,
        ILogger<RefreshTokenCommand> logger)
    {
        _context = context;
        _authService = authService;
        _logger = logger;
    }

    public async Task<RefreshTokenCommandResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var refreshToken = await _context
            .RefreshTokens
            .FirstOrDefaultAsync(x => x.RefreshTokenValue == request.RefreshToken, cancellationToken);

        // Refresh token no existe, expiró o fue revocado manualmente
        // (Pensando en que el usuario puede dar click en "Cerrar Sesión en todos lados" o similar)
        if (refreshToken is null ||
            refreshToken.Active == false ||
            refreshToken.Expiration <= DateTime.UtcNow)
        {
            throw new ForbiddenAccessException();
        }

        // Se está intentando user un Refresh Token que ya fue usado anteriormente
        // puede significar que este refresh token fue robado.
        if (refreshToken.Used)
        {
            _logger.LogWarning("El Refresh token del {UserId} ya fue usado. RT={RefreshToken}", refreshToken.UserId, refreshToken.RefreshTokenValue);
            var refreshTokens = await _context.RefreshTokens
                .Where(x => x.UserId == refreshToken.UserId && x.Active && x.Used == false)
                .ToListAsync(cancellationToken);

            foreach (var rt in refreshTokens)
            {
                rt.Used = true;
                rt.Active = false;
            }

            await _context.SaveChangesAsync(cancellationToken);

            throw new ForbiddenAccessException();
        }

        // TODO: Podríamos validar que el Access Token sí corresponde al mismo usuario

        refreshToken.Used = true;

        var user = await _context
            .Users
            .FindAsync(refreshToken.UserId, cancellationToken);

        if (user is null)
        {
            throw new NotFoundException(nameof(user), refreshToken.UserId);
        }

        var jwt = await _authService.GenerateAccessToken(user);

        var newRefreshToken = new RefreshToken
        {
            Active = true,
            Expiration = DateTime.UtcNow.AddDays(7),
            RefreshTokenValue = Guid.NewGuid().ToString("N"),
            Used = false,
            UserId = user.Id
        };

        _context.Add(newRefreshToken);

        await _context.SaveChangesAsync(cancellationToken);

        return new RefreshTokenCommandResponse
        {
            AccessToken = jwt,
            RefreshToken = newRefreshToken.RefreshTokenValue
        };
    }
}

public class RefreshTokenCommandResponse
{
    public string AccessToken { get; set; } = default!;
    public string RefreshToken { get; set; } = default!;
}