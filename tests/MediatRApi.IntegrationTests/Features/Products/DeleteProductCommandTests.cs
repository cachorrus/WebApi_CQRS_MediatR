using FluentAssertions;
using MediatRApi.ApplicationCore.Common.Helpers;
using MediatRApi.ApplicationCore.Domain;

namespace MediatRApi.IntegrationTests.Features.Products;

public class DeleteProductCommandTests : TestBase
{
    [Test]
    public async Task Product_IsDeleted_WhenValidFieldsAreProvided_AndUserIsAdmin()
    {
        // Arrange
        var productDemo = await FindAsync<Product>(q => 1 == 1);
        var (Client, userId) = await GetClientAsAdminAsync();

        // Act
        var result = await Client.DeleteAsync($"/api/products/{productDemo!.ProductId.ToSqids()}");

        // Assert
        FluentActions.Invoking(() => result.EnsureSuccessStatusCode())
            .Should().NotThrow();

        var product = await FindAsync<Product>(productDemo!.ProductId);
        product.Should().BeNull();
    }

    [Test]
    public async Task Product_IsNotDeleted_WhenValidFieldsAreProvided_AndDefaultUser()
    {
        // Arrange
        var productDemo = await FindAsync<Product>(q => 1 == 1);
        var (Client, userId) = await GetClientAsDefaultUserAsync();

        // Act
        var result = await Client.DeleteAsync($"/api/products/{productDemo!.ProductId.ToSqids()}");

        // Assert
        FluentActions.Invoking(() => result.EnsureSuccessStatusCode())
            .Should().Throw<HttpRequestException>();

        var product = await FindAsync<Product>(productDemo!.ProductId);
        product.Should().NotBeNull();
    }

    [Test]
    public async Task Product_ProduceNotFoundException_WhenInvalidFieldsAreProvided_AndUserIsAdmin()
    {
        // Arrange
        var (Client, userId) = await GetClientAsAdminAsync();

        // Act
        var result = await Client.DeleteAsync($"/api/products/{0.ToSqids()}");

        // Assert
        FluentActions.Invoking(() => result.EnsureSuccessStatusCode())
            .Should().Throw<HttpRequestException>();

    }
}