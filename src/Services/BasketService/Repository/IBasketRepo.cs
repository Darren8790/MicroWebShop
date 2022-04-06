namespace BasketService.Repository;

public interface IBasketRepo
{
    //Task<String> TestConnection(string key);
    Task<CustomerBasket> GetBasket(string customerId);
    Task<CustomerBasket> UpdateBasket(CustomerBasket customerBasket);
    Task<bool> DeleteBasket(string id);
    IEnumerable<string> GetUsers();
}