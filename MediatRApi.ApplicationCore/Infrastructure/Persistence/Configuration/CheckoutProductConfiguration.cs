using MediatRApi.ApplicationCore.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediatRApi.ApplicationCore.Infrastructure.Persistence.Configuration;

public class CheckoutProductConfiguration : IEntityTypeConfiguration<CheckoutProduct>
{
    public void Configure(EntityTypeBuilder<CheckoutProduct> builder)
    {
        builder.Property(q => q.UnitPrice).IsRequired().HasPrecision(18, 4);
        builder.Property(q => q.Total).IsRequired().HasPrecision(18, 4);
    }
}