using MediatRApi.ApplicationCore.Common.Interfaces;
using MediatRApi.ApplicationCore.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MediatRApi.ApplicationCore.Infrastructure.Persistence;

public class MyAppDbContext : IdentityDbContext<User>
{
    private readonly CurrentUser _user;

    public MyAppDbContext(
        DbContextOptions<MyAppDbContext> options,
        ICurrentUserService currentUserService)
        : base(options)
    {
        _user = currentUserService.User;
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Checkout> Checkouts => Set<Checkout>();
    public DbSet<CheckoutProduct> CheckoutProducts => Set<CheckoutProduct>();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedBy = _user.Id;
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;

                case EntityState.Modified:
                    entry.Entity.LastModifiedBy = _user.Id;
                    entry.Entity.LastModifiedByAt = DateTime.UtcNow;
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(MyAppDbContext).Assembly);

        base.OnModelCreating(builder);
    }
}