using MediatRApi.ApplicationCore.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediatRApi.ApplicationCore.Infrastructure.Persistence.Configuration;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(q => q.ProductId);
        builder.Property(q => q.Description).IsRequired();
        builder.Property(q => q.Price).IsRequired().HasPrecision(18, 4);
    }
}