using MediatRApi.ApplicationCore.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace MediatRApi.ApplicationCore.Infrastructure.Persistence;

public static class SeedData
{
    /*    public static async Task SeedDataInitialize(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = serviceProvider.GetRequiredService<MyAppDbContext>();
        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        context.Database.EnsureCreated();

        Console.WriteLine("Seeding database...");
        await SeedProducts(context);
        await SeedUsers(userManager, roleManager);
    }
    */

    public static async Task SeedDataAsync(MyAppDbContext context)
    {
        Console.WriteLine("Seeding database...");

        await SeedProducts(context);
    }
    private static async Task SeedProducts(MyAppDbContext context)
    {
        Console.WriteLine("Seeding Products...");

        if (!context.Products.Any())
        {
            var random = new Random();

            var products =
                Enumerable.Range(1, 100)
                    .Select(index => new Product
                    {
                        Description = $"Product {index}",
                        Price = random.NextInt64(100, 1000)
                    });

            await context.Products.AddRangeAsync(products);

            await context.SaveChangesAsync();
        }
    }

    public static async Task SeedUsersAsync(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
    {
        Console.WriteLine("Seeding Users...");

        var testUser = await userManager.FindByNameAsync("test_user");

        if (testUser is null)
        {
            testUser = new User
            {
                UserName = "test_user"
            };

            await userManager.CreateAsync(testUser, "Passw0rd.1234");
            await userManager.CreateAsync(new User
            {
                UserName = "other_user"
            }, "Passw0rd.1234");
        }

        Console.WriteLine("Seeding Roles...");

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