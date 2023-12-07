namespace MediatRApi.ApplicationCore.Domain;

public class Product
{
    public int ProductId { get; set; }
    public string Description { get; set; } = default!;
    public decimal Price { get; set; }
}