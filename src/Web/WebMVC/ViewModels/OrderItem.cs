namespace WebMVC.ViewModels;

public record OrderItem
{
    public int ProductId { get; init; }
    public string ProductName { get; init; }
    public decimal UnitPrice { get; init;}
    public int Units { get; init; }
    public string PictureUrl { get; init; }
}