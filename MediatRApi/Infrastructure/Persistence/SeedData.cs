using MediatRApi.Domain;
using Microsoft.AspNetCore.Identity;

namespace MediatRApi.Infrastructure.Persistence;

public static class SeedData
{
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        //var context = serviceProvider.GetRequiredService<MyAppDbContext>();
        //var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

        Console.WriteLine("Seeding database...");
        await SeedProducts(scope);
        await SeedUsers(scope);
    }

    private static async Task SeedProducts(IServiceScope scope)
    {
        Console.WriteLine("Seeding Products...");

        var context = scope.ServiceProvider.GetRequiredService<MyAppDbContext>();

        if (!context.Products.Any())
        {
            context.Products.AddRange(new List<Product>
            {
                new Product
                {
                    Description = "Product 01",
                    Price = 16000
                },
                new Product
                {
                    Description = "Product 02",
                    Price = 52200
                }
            });

            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedUsers(IServiceScope scope)
    {
        Console.WriteLine("Seeding Users...");

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var testUser = await userManager.FindByNameAsync("test_user");

        if (testUser is null)
        {
            testUser = new IdentityUser
            {
                UserName = "test_user"
            };

            await userManager.CreateAsync(testUser, "Passw0rd.1234");
            await userManager.CreateAsync(new IdentityUser
            {
                UserName = "other_user"
            }, "Passw0rd.1234");
        }

        Console.WriteLine("Seeding Roles...");

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var adminRole = await roleManager.FindByNameAsync("Admin");

        if (adminRole is null)
        {
            await roleManager.CreateAsync(new IdentityRole
            {
                Name = "Admin"
            });

            await userManager.AddToRoleAsync(testUser, "Admin");
        }
    }
}