using MediatRApi.ApplicationCore;
using MediatRApi.ApplicationCore.Common.Interfaces;
using MediatRFunction.CheckoutProcessor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((hostBuilderContext, services) =>
    {
        var configuration = hostBuilderContext.Configuration;

        services
            .AddApplicationCore()
            .AddPersistence(configuration)
            .AddInfrastructure();

        services.AddTransient<ICurrentUserService, WorkerUserService>();
    })
    .Build();

host.Run();
