using System.Linq.Expressions;
using System.Net.Http.Headers;
using MediatR;
using MediatRApi.ApplicationCore.Common.Exceptions;
using MediatRApi.ApplicationCore.Domain;
using MediatRApi.ApplicationCore.Features.Auth.Commands;
using MediatRApi.ApplicationCore.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Respawn;

namespace MediatRApi.IntegrationTests;

public class TestBase
{
    protected ApiWebApplicationFactory Application;

    /// <summary>
    /// Crear un usuario de prueba según los parámetros
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="password"></param>
    /// <param name="roles"></param>
    /// <returns></returns>
    public async Task<(HttpClient Client, string UserId, TokenCommandResponse AuthInfo)> CreateTestUser(
        string userName, string password, string[] roles)
    {
        using var scope = Application.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        var newUser = new User
        {
            UserName = userName
        };

        await userManager.CreateAsync(newUser, password);

        foreach (var role in roles)
        {
            var _role = await roleManager.FindByNameAsync(role);

            if (_role is null)
            {
                await roleManager.CreateAsync(new IdentityRole
                {
                    Name = role
                });
            }

            await userManager.AddToRoleAsync(newUser, role);
        }

        var authResponse = await GetAccessToken(userName, password);

        var client = Application.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResponse.AccessToken);

        return (client, newUser.Id, authResponse);
    }

    /// <summary>
    /// Al finalizar cada prueba, se borra la base de datos
    /// </summary>
    /// <returns></returns>
    [TearDown]
    public async Task Down()
    {
        await ResetState();
    }

    /// <summary>
    /// Crea un HttpClient con un JWT válido para un usuario Admin
    /// </summary>
    /// <returns></returns> <summary>
    public Task<(HttpClient Client, string UserId, TokenCommandResponse AuthInfo)> GetClientAsAdminAsync() =>
        CreateTestUser("user@admin.com", "Pass.W0rd", new string[] { "Admin" });

    /// <summary>
    /// Crea un HttpClient con un JWT válido para un usuario default
    /// </summary>
    /// <returns></returns>
    public Task<(HttpClient Client, string UserId, TokenCommandResponse AuthInfo)> GetClientAsDefaultUserAsync() =>
        CreateTestUser("user@normal.com", "Pass.W0rd", Array.Empty<string>());

    /// <summary>
    /// Libera recursos al terminar todas las pruebas
    /// </summary>
    [OneTimeTearDown]
    public void RunAfterAnyTest()
    {
        Application.Dispose();
    }

    /// <summary>
    /// Inicializar la API y la base de datos antes de comenzar las pruebas
    /// </summary>
    [OneTimeSetUp]
    public void RunBeforeAnyTest()
    {
        Application = new ApiWebApplicationFactory();

        EnsureDataBase();
    }

    /// <summary>
    /// Shortcut para ejecutar IRequest con el Mediator
    /// </summary>
    /// <param name="request"></param>
    /// <typeparam name="TResponse"></typeparam>
    /// <returns></returns>
    public async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
    {
        using var scope = Application.Services.CreateScope();

        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();

        return await mediator.Send(request);
    }

    /// <summary>
    /// Shorcut para agregar Entities a la Base de Datos
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public async Task<TEntity> AddAsync<TEntity>(TEntity entity) where TEntity : class
    {
        using var scope = Application.Services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<MyAppDbContext>();

        context.Add(entity);

        await context.SaveChangesAsync();

        return entity;
    }

    /// <summary>
    ///  Shotcut para buscar entities por primary key
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="keyValues"></param>
    /// <returns></returns>
    /// <exception cref="NotFoundException"></exception>
    public async Task<TEntity?> FindAsync<TEntity>(params object[] keyValues) where TEntity : class
    {
        using var scope = Application.Services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<MyAppDbContext>();

        return await context.FindAsync<TEntity>(keyValues);
    }

    /// <summary>
    /// Shortcut para buscar entities según criterio
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="predicate"></param>
    /// <returns></returns>
    /// <exception cref="NotFoundException"></exception>
    public async Task<TEntity?> FindAsync<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class
    {
        using var scope = Application.Services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<MyAppDbContext>();

        return await context.Set<TEntity>().FirstOrDefaultAsync(predicate);
    }

    /// <summary>
    /// Se asegura de crear la base de datos
    /// </summary>
    private void EnsureDataBase()
    {
        using var scope = Application.Services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<MyAppDbContext>();

        context.Database.EnsureCreated();
    }

    /// <summary>
    /// Shortcut para autenticar un usuario para pruebas
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    public async Task<TokenCommandResponse> GetAccessToken(string userName, string password)
    {
        using var scope = Application.Services.CreateScope();

        var result = await SendAsync(new TokenCommand
        {
            UserName = userName,
            Password = password
        });

        return result;
    }

    private async Task ResetState()
    {
        using var scope = Application.Services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<MyAppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        if (context.Database.IsSqlite())
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }
        else if (context.Database.IsSqlServer())
        {
            var config = scope.ServiceProvider.GetService<IConfiguration>();
            var connection = config?.GetConnectionString("Default");

            var respawner = await Respawner.CreateAsync(
                connection!,
                new RespawnerOptions
            {
                TablesToIgnore = new Respawn.Graph.Table[] { "__EFMigrationsHistory" }
            });

            await respawner.ResetAsync(connection!);
        }

        await SeedData.SeedDataAsync(context);
        await SeedData.SeedUsersAsync(userManager, roleManager);
    }
}