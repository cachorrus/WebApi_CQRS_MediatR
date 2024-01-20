
using MediatRApi.WebApi;
using MediatRApi.ApplicationCore;
using MediatRApi.ApplicationCore.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Serilog;
using Serilog.Events;
using Serilog.Enrichers.Sensitive;
using MediatRApi.ApplicationCore.Domain;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    /* Lib para enmascarar datos
    * https://github.com/serilog-contrib/Serilog.Enrichers.Sensitive
    *
    * Otra opción es usar el paquete Destructurama.Attributed
    */
    .Enrich.WithSensitiveDataMasking(options =>
    {
        options.MaskingOperators.Clear();
        //Enmascara todas las propiedades del siguiente arreglo de propiedades con el valor = ***MASKED***
        options.MaskProperties.AddRange(["Password"]);
    })
    .WriteTo.Console()
    //.WriteTo.File("log.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Services.AddWebApi();
builder.Services.AddApplicationCore();
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddSecurity(builder.Configuration);
builder.Services.AddInfrastructure();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

try
{
    Log.Information("Starting web host");

    await SeedDataInitialize();

    Log.Information("Application started in:");
    Log.Information("http://localhost:5000");
    Log.Information("https://localhost:7122");
    Log.Information("Environment: {environment}", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
    return;
}
finally
{
    Log.CloseAndFlush();
}

/// <summary>
/// Inicializa la base de datos
/// </summary>
/// <returns></returns>
async Task SeedDataInitialize()
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<MyAppDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    context.Database.EnsureCreated();

    if (app.Environment.IsDevelopment())
    {
        await SeedData.SeedDataAsync(context);
        await SeedData.SeedUsersAsync(userManager, roleManager);
    }
}