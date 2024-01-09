using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using MediatRApi.ApplicationCore.Features.Auth.Commands;

namespace MediatRApi.IntegrationTests.Features.Auth;

public class TokenCommandTests : TestBase
{
    [Test]
    public async Task User_CanLogin()
    {
        // Arrange
        var Client = Application.CreateClient();

        // Act
        var result = await Client.PostAsJsonAsync("/api/auth", new TokenCommand
        {
            UserName = "test_user",
            Password = "Passw0rd.1234"
        });

        // Assert
        FluentActions.Invoking(() => result.EnsureSuccessStatusCode())
            .Should().NotThrow();

        var response = JsonSerializer.Deserialize<TokenCommandResponse>(result.Content.ReadAsStream(), new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        response.Should().NotBeNull();
        response?.AccessToken.Should().NotBeNullOrEmpty();
    }

    [Test]
    public async Task User_CannotLogin()
    {
        // Arrange
        var client = Application.CreateClient();

        // Act
        var result = await client.PostAsJsonAsync("api/auth", new TokenCommand
        {
            UserName = "test_user",
            Password = "123456"
        });

        // Assert
        FluentActions.Invoking(() => result.EnsureSuccessStatusCode())
            .Should().Throw<HttpRequestException>();

        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}