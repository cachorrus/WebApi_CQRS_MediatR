namespace MediatRApi.ApplicationCore.Domain;

public class RefreshToken
{
    public int RefreshTokenId { get; set; }
    public string RefreshTokenValue { get; set; } = default!;
    public bool Active { get; set; }
    public DateTime Expiration { get; set; }
    public bool Used { get; set; }
    public User User { get; set; } = default!;
    public string UserId { get; set; } = default!;
}