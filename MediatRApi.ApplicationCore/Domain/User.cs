using Microsoft.AspNetCore.Identity;

namespace MediatRApi.ApplicationCore.Domain;

public class User : IdentityUser
{
    public ICollection<AccessToken> AccessTokens { get; set; } 
        = new HashSet<AccessToken>();
}