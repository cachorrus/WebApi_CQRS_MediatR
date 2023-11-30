using MediatRApi.Domain;
using Microsoft.EntityFrameworkCore;

namespace MediatRApi.Infrastructure.Persistence;

public class MyAppDbContext : DbContext
{
    public MyAppDbContext(DbContextOptions<MyAppDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
}