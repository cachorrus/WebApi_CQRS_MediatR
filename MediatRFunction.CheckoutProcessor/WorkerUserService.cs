using MediatRApi.ApplicationCore.Common.Interfaces;

namespace MediatRFunction.CheckoutProcessor;

public class WorkerUserService : ICurrentUserService
{
    public CurrentUser User => new(Guid.Empty.ToString(), "CheckoutProcessor", true);

    public bool IsInRole(string roleName) => true;
}