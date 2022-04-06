using WebMVC.Services.DTOs;
using WebMVC.ViewModels;

namespace WebMVC.Services;

public interface IBasketService
{
    Task<Basket> GetBasket(AppUser appUser);
    Task AddItemToBasket(AppUser appUser, int productId);
    Task<Basket> UpdateBasket(Basket basket);
    Task Checkout(BasketDTO basketDTO);
    Task<Basket> SetQuantities(AppUser appUser, Dictionary<string, int> quantities);
    Task<Order> GetOrderDraft(string basketId);
}