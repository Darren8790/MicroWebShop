//using System.ComponentModel.DataAnnotations;

namespace BasketService.Model;

public class BasketItem
{
    public string Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public string UnitPrice { get; set; }
    public int Quantity { get; set; }
    public string PictureUrl { get; set; }
}