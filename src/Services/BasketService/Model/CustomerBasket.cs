namespace BasketService.Model;

public class CustomerBasket
{
    public string BuyerId { get; set; }
    public List<BasketItem> Items { get; set; } = new();
    public CustomerBasket()
    {

    }

    public CustomerBasket(string customerId)
    {
        BuyerId = customerId;
        //Guid custId = Guid.NewGuid();
        //custId.ToString();
        //BuyerId = custId;
        //BuyerId = GetCartId();
    }

    // public string GetCartId()
    // {
    //     // Generate a new random GUID using System.Guid class.     
    //     Guid tempCartId = Guid.NewGuid();
    //     HttpContext.Current.Session[CartSessionKey] = tempCartId.ToString();
    //     return HttpContext.Current.Session[CartSessionKey].ToString();
    // }
}