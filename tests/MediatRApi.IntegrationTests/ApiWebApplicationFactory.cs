using MediatRApi.WebApi;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;

namespace MediatRApi.IntegrationTests;

public class ApiWebApplicationFactory : WebApplicationFactory<Api>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        base.ConfigureWebHost(builder);
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Configura cualquier Mock o similares aquí.
        });

        return base.CreateHost(builder);
    }
}