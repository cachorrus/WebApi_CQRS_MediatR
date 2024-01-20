namespace MediatRApi.ApplicationCore.Domain;

public class CheckoutProduct: BaseEntity
{
    public int CheckoutProductId { get; set; }
    public int CheckoutId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Total { get; set; }

    public virtual Checkout Checkout { get; set; } = default!;
    public virtual Product Product { get; set; } = default!;
}