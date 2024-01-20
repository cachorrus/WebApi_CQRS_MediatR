using Microsoft.AspNetCore.Identity;

namespace MediatRApi.ApplicationCore.Domain;

public class User : IdentityUser
{
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
    public ICollection<Checkout> Checkouts { get; set; } = [];
}