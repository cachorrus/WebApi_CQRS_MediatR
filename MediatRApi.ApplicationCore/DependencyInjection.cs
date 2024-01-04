using System.Reflection;
using System.Text;
using Audit.Core;
using FluentValidation;
using MediatR;
using MediatRApi.ApplicationCore.Common.Behaviours;
using MediatRApi.ApplicationCore.Common.Interfaces;
using MediatRApi.ApplicationCore.Domain;
using MediatRApi.ApplicationCore.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace MediatRApi.ApplicationCore;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationCore(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddMediatR(Assembly.GetExecutingAssembly());
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuditLogsBehavior<,>));
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        return services;
    }

    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        if (configuration.GetValue<bool>("UseSqlite"))
        {
            services.AddSqlite<MyAppDbContext>(configuration.GetConnectionString("Sqlite"));
        } else
        {
            services.AddDbContext<MyAppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("SqlServer")));
        }

        Configuration.Setup()
            .UseAzureStorageBlobs(config => config
                .WithConnectionString(configuration["AuditLogs:ConnectionStrings"]!)
                .ContainerName(ev => $"meditrapilogs{DateTime.Today:yyyyMMdd}")
                .BlobName(ev =>
                {
                    var currentUser = ev.CustomFields["User"] as CurrentUser;

                    return $"{ev.EventType}/{currentUser?.Id}-{DateTime.UtcNow.Ticks}.json";
                }));

        return services;
    }

    public static IServiceCollection AddSecurity(this IServiceCollection services, IConfiguration config)
    {

        services
            .AddIdentityCore<User>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<MyAppDbContext>();

        services
            .AddHttpContextAccessor()
            .AddAuthorization()
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = config["Jwt:Issuer"],
                    ValidAudience = config["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!))
                };
            });

        return services;
    }
}