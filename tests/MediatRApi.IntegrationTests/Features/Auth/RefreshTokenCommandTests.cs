using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using MediatRApi.ApplicationCore.Features.Auth.Commands;

namespace MediatRApi.IntegrationTests.Features.Auth;

public class RefreshTokenCommandTests : TestBase
{
    [Test]
    public async Task AccessTokenDenied_WithUsed_RefreshToken()
    {
        // Arrange
        var (_, _, AuthInfo) = await CreateTestUser("testUser", "Passw0rd.1234", Array.Empty<string>());

        // Usamos el Refresh Token
        var command = new RefreshTokenCommand
        {
            AccessToken = AuthInfo.AccessToken,
            RefreshToken = AuthInfo.RefreshToken
        };

        await SendAsync(command);

        var anonymHttpClient = Application.CreateClient();

        // Act
        var result = await anonymHttpClient.PostAsJsonAsync("/api/auth/refresh", command);

        // Assert
        FluentActions.Invoking(() => result.EnsureSuccessStatusCode())
            .Should().Throw<HttpRequestException>();

        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task GetAccessToken_WithValid_RefreshToken()
    {
        // Arrange
        var (_, _, AuthInfo) = await CreateTestUser("testUser", "Passw0rd.1234", Array.Empty<string>());

        // Usamos el Refresh Token
        var command = new RefreshTokenCommand
        {
            AccessToken = AuthInfo.AccessToken,
            RefreshToken = AuthInfo.RefreshToken
        };

        var anonymHttpClient = Application.CreateClient();

        // Act
        var result = await anonymHttpClient.PostAsJsonAsync("api/auth/refresh", command);

        // Assert
        FluentActions.Invoking(() => result.EnsureSuccessStatusCode())
            .Should().NotThrow();

        var commandResponse = JsonSerializer.Deserialize<RefreshTokenCommandResponse>(result.Content.ReadAsStream(), new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        commandResponse.Should().NotBeNull();
        commandResponse?.AccessToken.Should().NotBeNull();
        commandResponse?.RefreshToken.Should().NotBeNull();
    }
}