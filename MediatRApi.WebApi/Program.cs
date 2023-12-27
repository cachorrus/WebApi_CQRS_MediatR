
using MediatRApi.WebApi;
using MediatRApi.ApplicationCore;
using MediatRApi.ApplicationCore.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddWebApi();
builder.Services.AddApplicationCore();
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddSecurity(builder.Configuration);

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

await SeedDataInitialize();

app.Run();

async Task SeedDataInitialize()
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<MyAppDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    context.Database.EnsureCreated();

    await SeedData.SeedDataAsync(context);
    await SeedData.SeedUsersAsync(userManager, roleManager);
}