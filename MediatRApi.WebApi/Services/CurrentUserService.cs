using System.Security.Claims;
using MediatRApi.ApplicationCore.Common.Exceptions;
using MediatRApi.ApplicationCore.Common.Interfaces;


namespace MediatRApi.WebApi.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;

        var id = _httpContextAccessor.HttpContext?.User?.Claims?
            .FirstOrDefault(q => q.Type == ClaimTypes.Sid)?
            .Value;

        var userName = _httpContextAccessor.HttpContext?.User?.Identity?.Name;

        if (id is null || userName is null)
        {
            throw new ForbiddenAccessException();
        }

        User = new CurrentUser(id, userName);
    }

    public CurrentUser User  { get; }

    public bool IsInRole(string roleName)
    {
        return _httpContextAccessor.HttpContext?.User?.IsInRole(roleName) ?? false;
    }
}