namespace MediatRApi.ApplicationCore.Domain;

public class Checkout : BaseEntity
{
    public int CheckoutId { get; set; }
    public DateTime CheckoutDateTime { get; set; }
    public DateTime? ProcessedDateTime { get; set; }
    public decimal Total { get; set; }
    public bool Processed { get; set; }
    public string UserId { get; set; } = default!;

    public ICollection<CheckoutProduct> CheckoutItems { get; set; } = [];
    public User User { get; set; } = default!;
}