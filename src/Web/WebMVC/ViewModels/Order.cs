using System.ComponentModel.DataAnnotations;

namespace WebMVC.ViewModels;

public class Order
{
    public string OrderNumber { get; set; }
    public DateTime Date { get; set; }
    public decimal Total { get; set; }
    public string Status { get; set; }
    public string Description { get; set; }
    [Required]
    public string Street { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
    public string PostCode { get; set; }
    public string Buyer { get; set; }
    public List<OrderItem> OrderItems { get; set; }
    [Required]
    public Guid RequestId { get; set; }
}