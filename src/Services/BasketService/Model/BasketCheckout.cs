namespace BasketService.Model;

public class BasketCheckout
{
    public string Country { get; set; }
    public string City { get; set; }
    public string Street { get; set; }
    public string PostCode { get; set; }
    public string Buyer { get; set; }
    public Guid RequestId { get; set; }
}