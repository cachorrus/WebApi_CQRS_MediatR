using System.Net.Http.Json;
using FluentAssertions;
using MediatRApi.ApplicationCore.Common.Models;
using MediatRApi.ApplicationCore.Features.Products.Queries;

namespace MediatRApi.IntegrationTests.Features.Products;

public class GetProductsQueryTests : TestBase
{
    [Test]
    [TestCase(10)]
    [TestCase(20)]
    [TestCase(30)]
    public async Task Products_Obtained_WithAuthenticatedUser(int pageSize)
    {
        // Arrange
        var (Client, userId, _) = await GetClientAsDefaultUserAsync();

        // Act
        var products = await Client.GetFromJsonAsync<PagedResult<GetProductsQueryResponse>>($"/api/products?pageSize={pageSize}&currentPage=1&sortDir=desc&sortProperty=price");

        // Assert
        products.Should().NotBeNull();
        products?.Results.Count().Should().Be(pageSize);
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