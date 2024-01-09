using MediatRApi.ApplicationCore.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediatRApi.ApplicationCore.Infrastructure.Persistence.Configuration;

public class AccessTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(q => q.RefreshTokenId);
        builder.Property(q => q.RefreshTokenValue).IsRequired();
        builder.Property(q => q.UserId).IsRequired();

        builder.HasOne(q => q.User)
            .WithMany(q => q.RefreshTokens)
            .HasForeignKey(q => q.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}