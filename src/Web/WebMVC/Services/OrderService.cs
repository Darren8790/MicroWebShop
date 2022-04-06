using System.Text.Json;
using Microsoft.Extensions.Options;
using WebMVC.Services.DTOs;
using WebMVC.ViewModels;

namespace WebMVC.Services;

public class OrderService : IOrderService
{
    private HttpClient _htttpClient;
    private readonly IOptions<AppSettings> _settings;
    private readonly IConfiguration _configuration;
    private readonly string _orderServiceUrl;

    public OrderService(HttpClient httpClient, IOptions<AppSettings> settings, IConfiguration configuration)
    {
        _htttpClient = httpClient;
        _settings = settings;
        _configuration = configuration;

        _orderServiceUrl = $"{settings.Value.PurchaseUrl}{_configuration["OrderService"]}/api/v1/order";
    }
    public async Task CancelOrder(string orderId)
    {
        var order = new OrderDTO()
        {
            OrderNumber = orderId
        };

        var uri = API.Order.CancelOrder(_orderServiceUrl);
        var orderContent = new StringContent(JsonSerializer.Serialize(order),
            System.Text.Encoding.UTF8, "application/json");
        var response = await _htttpClient.PutAsync(uri, orderContent);
        response.EnsureSuccessStatusCode();
    }

    public async Task CreateOrder(string orderId)
    {
        var order = new OrderDTO()
        {
            OrderNumber = orderId
        };

        var uri = API.Order.ShipOrder(_orderServiceUrl);
        var orderContent = new StringContent(JsonSerializer.Serialize(order),
            System.Text.Encoding.UTF8, "application/json");
        var response = await _htttpClient.PutAsync(uri, orderContent);
        response.EnsureSuccessStatusCode();
    }

    public async Task<Order> GetOrder(string orderId, AppUser appUser)
    {
        var uri = API.Order.GetOrder(_orderServiceUrl, orderId);
        var responseString = await _htttpClient.GetStringAsync(uri);
        var response = JsonSerializer.Deserialize<Order>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        return response;
    }

    public async Task<List<Order>> GetOrders(AppUser appUser)
    {
        var uri = API.Order.GetAllOrders(_orderServiceUrl);
        var responseString = await _htttpClient.GetStringAsync(uri);
        var response = JsonSerializer.Deserialize<List<Order>>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        return response;
    }

    public BasketDTO MapOrderToBasket(Order order)
    {
        return new BasketDTO()
        {
            RequestId = order.RequestId,
            Buyer = order.Buyer,
            Street = order.Street,
            City = order.City,
            Country = order.Country,
            PostCode = order.PostCode
        };
    }

    // public Order MapUserToOrder(AppUser appUser, Order order)
    // {
    //     order.Street = user.Street;
    //     order.City = user.City;
    //     order.Country = user.Country;
    //     order.PostCode = user.PostCode;
    //     return order;
    // }
}