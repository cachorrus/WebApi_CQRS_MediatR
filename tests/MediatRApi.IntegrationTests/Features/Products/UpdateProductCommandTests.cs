using System.Net.Http.Json;
using FluentAssertions;
using MediatRApi.ApplicationCore.Common.Helpers;
using MediatRApi.ApplicationCore.Domain;
using MediatRApi.ApplicationCore.Features.Products.Commands;

namespace MediatRApi.IntegrationTests.Features.Products;

public class UpdateProductCommandTests : TestBase
{
    [Test]
    public async Task Product_IsUpdated_WithValidFields_AndAuthUser()
    {
        // Arrange
        var productDemo = await FindAsync<Product>(q => 1 == 1);
        var (Client, userId) = await GetClientAsAdminAsync();

        var command = new UpdateProductCommand
        {
            ProductId = productDemo!.ProductId.ToSqids(),
            Description = "Updated Product",
            Price = 123456
        };

        // Act
        var result = await Client.PutAsJsonAsync($"/api/products/{command.ProductId}", command);

        // Assert
        FluentActions.Invoking(() => result.EnsureSuccessStatusCode())
            .Should().NotThrow();

        var product = await FindAsync<Product>(productDemo.ProductId);

        product.Should().NotBeNull();
        product!.Description.Should().Be(command.Description);
        product.Price.Should().Be(command.Price);
        product.LastModifiedBy.Should().Be(userId);
    }

    [Test]
    public async Task Product_IsNotUpdated_WithInvalidFields_AndAuthUser()
    {
        // Arrange
        var productDemo = await FindAsync<Product>(q => 1 == 1);
        var (Client, userId) = await GetClientAsAdminAsync();

        var command = new UpdateProductCommand
        {
            ProductId = productDemo!.ProductId.ToSqids(),
            Description = string.Empty,
            Price = 0
        };

        // Act
        var result = await Client.PutAsJsonAsync($"/api/products/{command.ProductId}", command);

        // Assert
        FluentActions.Invoking(() => result.EnsureSuccessStatusCode())
            .Should().Throw<HttpRequestException>();

        var product = await FindAsync<Product>(productDemo.ProductId);

        product.Should().NotBeNull();
        product!.Description.Should().NotBe(command.Description);
        product.Price.Should().NotBe(command.Price);
    }

    [Test]
    public async Task Product_IsNotUpdated_WithValidFields_AndAnonymUser()
    {
        // Arrange
        var productDemo = await FindAsync<Product>(q => 1 == 1);
        var (Client, userId) = await GetClientAsDefaultUserAsync();

        var command = new UpdateProductCommand
        {
            ProductId = productDemo!.ProductId.ToSqids(),
            Description = "Updated Product",
            Price = 123456
        };

        // Act
        var result = await Client.PutAsJsonAsync($"/api/products/{command.ProductId}", command);

        // Assert
        FluentActions.Invoking(() => result.EnsureSuccessStatusCode())
            .Should().Throw<HttpRequestException>();

        var product = await FindAsync<Product>(productDemo.ProductId);

        product.Should().NotBeNull();
        product!.Description.Should().NotBe(command.Description);
        product.Price.Should().NotBe(command.Price);
    }
}