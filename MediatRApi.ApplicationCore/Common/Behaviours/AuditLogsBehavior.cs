using System.Reflection;
using Audit.Core;
using MediatR;
using MediatRApi.ApplicationCore.Common.Attributes;
using MediatRApi.ApplicationCore.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MediatRApi.ApplicationCore.Common.Behaviours;

public class AuditLogsBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<AuditLogsBehavior<TRequest, TResponse>> _logger;
    private readonly IConfiguration _configuration;

    public AuditLogsBehavior(
        ICurrentUserService currentUserService,
        ILogger<AuditLogsBehavior<TRequest, TResponse>> logger,
        IConfiguration configuration)
    {
        _currentUserService = currentUserService;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        _logger.LogInformation("User {@User} with request {@Request}", _currentUserService.User, request);

        IAuditScope? scope = null;

        var auditLogAttributes = request.GetType().GetCustomAttributes<AuditLogAttribute>();

        if (auditLogAttributes.Any() && _configuration.GetValue<bool>("AuditLogs:Enabled"))
        {
            // El IRequest cuenta con el atributo [AuditLog] para ser auditado
            scope = AuditScope.Create(_ => _
                .EventType(typeof(TRequest).Name)
                .ExtraFields(new
                {
                    _currentUserService.User,
                    Request = request
                }));
        }

        var result = await next();

        if (scope is not null)
        {
            await scope.DisposeAsync();
        }

        return result;
    }
}