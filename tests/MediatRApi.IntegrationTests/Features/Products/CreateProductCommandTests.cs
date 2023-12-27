using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MediatRApi.ApplicationCore.Domain;
using MediatRApi.ApplicationCore.Features.Products.Commands;

namespace MediatRApi.IntegrationTests.Features.Products;

public class CreateProductCommandTests : TestBase
{
    [Test]
    public async Task Product_IsCreated_WhenValidFieldsAreProvided_AndUserIsAdmin()
    {
        // Arrange
        var (Client, userId) = await GetClientAsAdminAsync();

        // Act
        var command = new CreateProductCommand
        {
            Description = "Test Product",
            Price = 999
        };

        var result = await Client.PostAsJsonAsync("/api/products", command);

        // Assert
        FluentActions.Invoking(() => result.EnsureSuccessStatusCode())
            .Should().NotThrow();

        var product = await FindAsync<Product>(q => q.Description == command.Description);

        product.Should().NotBeNull();
        product!.Description.Should().Be(command.Description);
        product.Price.Should().Be(command.Price);
        product.CreatedBy.Should().Be(userId);
    }

    [Test]
    public async Task Product_IsNotCreated_WhenInvalidFieldsAreProvided_AndUserIsAdmin()
    {
        // Arrange
        var (Client, userId) = await GetClientAsAdminAsync();

        // Act
        var command = new CreateProductCommand
        {
            Description = "Test Product",
            Price = 0
        };

        var result = await Client.PostAsJsonAsync("/api/products", command);

        // Assert
        FluentActions.Invoking(() => result.EnsureSuccessStatusCode())
            .Should().Throw<HttpRequestException>();

        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task Product_IsNotCreated_WhenValidFieldsAreProvided_AndDefaultUser()
    {
        // Arrange
        var (Client, userId) = await GetClientAsDefaultUserAsync();

        var command = new CreateProductCommand
        {
            Description = "Test Product",
            Price = 999
        };

        // Act
        var result = await Client.PostAsJsonAsync("/api/products", command);

        // Assert
        FluentActions.Invoking(() => result.EnsureSuccessStatusCode())
            .Should().Throw<HttpRequestException>();

        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}