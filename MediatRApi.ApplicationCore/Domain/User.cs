using Microsoft.AspNetCore.Identity;

namespace MediatRApi.ApplicationCore.Domain;

public class User : IdentityUser
{
    public ICollection<RefreshToken> RefreshTokens { get; set; }
        = new HashSet<RefreshToken>();
}