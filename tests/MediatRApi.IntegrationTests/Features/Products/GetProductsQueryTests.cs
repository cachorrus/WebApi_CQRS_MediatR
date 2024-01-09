using System.Net.Http.Json;
using FluentAssertions;
using MediatRApi.ApplicationCore.Features.Products.Queries;

namespace MediatRApi.IntegrationTests.Features.Products;

public class GetProductsQueryTests : TestBase
{
    [Test]
    public async Task Products_Obtained_WithAuthenticatedUser()
    {
        // Arrange
        var (Client, userId, _) = await GetClientAsDefaultUserAsync();

        // Act
        var products = await Client.GetFromJsonAsync<List<GetProductsQueryResponse>>("/api/products");

        // Assert
        products.Should().NotBeNullOrEmpty();
        products?.Count.Should().Be(2);
    }

    [Test]
    public async Task Products_ProducesException_WithUnauthenticatedUser()
    {
        // Arrange
        var Client = Application.CreateClient();

        // Act and Assert
        await FluentActions.Invoking(() =>
            Client.GetFromJsonAsync<List<GetProductsQueryResponse>>("/api/products"))
                .Should().ThrowAsync<HttpRequestException>();
    }
}