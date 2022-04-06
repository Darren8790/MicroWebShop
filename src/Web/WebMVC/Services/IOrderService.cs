using WebMVC.Services.DTOs;
using WebMVC.ViewModels;

namespace WebMVC.Services;

public interface IOrderService
{
    Task<List<Order>> GetOrders(AppUser appUser);
    Task<Order> GetOrder(string orderId, AppUser appUser);
    Task CancelOrder(string orderId);
    Task CreateOrder(string orderId);
    BasketDTO MapOrderToBasket(Order order);
    //Order MapUserToOrder(Order order, AppUser appUser);
}