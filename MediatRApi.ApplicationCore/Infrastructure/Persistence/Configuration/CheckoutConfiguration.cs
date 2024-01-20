using MediatRApi.ApplicationCore.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediatRApi.ApplicationCore.Infrastructure.Persistence.Configuration;

public class CheckoutConfiguration : IEntityTypeConfiguration<Checkout>
{
    public void Configure(EntityTypeBuilder<Checkout> builder)
    {
        builder.Property(q => q.Total).IsRequired().HasPrecision(18, 4);
    }
}