using System.Text.Json;
using Microsoft.Extensions.Options;
using WebMVC.Services.DTOs;
using WebMVC.ViewModels;

namespace WebMVC.Services;

public class BasketService : IBasketService
{
    private readonly HttpClient _httpClient;
    private readonly IOptions<AppSettings> _settings;
    private readonly IConfiguration _configuration;
    private readonly string _purchaseUrl;
    private readonly string _basketUrl;

    public BasketService(HttpClient httpClient, IOptions<AppSettings> settings, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _settings = settings;
        _configuration = configuration;

        _purchaseUrl = $"{_settings.Value.PurchaseUrl}{_configuration["BasketService"]}/api/v1";
        _basketUrl = $"{_settings.Value.PurchaseUrl}{_configuration["BasketService"]}/b/api/v1/basket";
    }

    // Potential issues with API Class and lack of user

    public async Task AddItemToBasket(AppUser appUser, int productId)
    {
        var uri = API.Purchase.AddItemToBasket(_purchaseUrl);
        
        var itemAdded = new
        {
            BasketId = appUser.Id,
            CatalogueItemId = productId,
            Quantity = 1
        };

        var basketContents = new StringContent(JsonSerializer.Serialize(itemAdded),
            System.Text.Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(uri, basketContents);
    }

    public async Task Checkout(BasketDTO basketDTO)
    {
        var uri = API.Basket.CheckoutBasket(_basketUrl);

        var basketContents = new StringContent(JsonSerializer.Serialize(basketDTO),
            System.Text.Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(uri, basketContents);
        response.EnsureSuccessStatusCode();
    }

    public async Task<Basket> GetBasket(AppUser appUser)
    {
        var uri = API.Basket.GetBasket(_basketUrl, appUser.Id);
        Console.WriteLine("--> WebMVC - GetBasket");
        var response = await _httpClient.GetAsync(uri);
        var responseString = await response.Content.ReadAsStringAsync();

        return string.IsNullOrEmpty(responseString) ?
            new Basket() {BuyerId = appUser.Id} :
            JsonSerializer.Deserialize<Basket>(responseString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
    }

    public async Task<Order> GetOrderDraft(string basketId)
    {
        var uri = API.Purchase.GetOrderDraft(_purchaseUrl, basketId);
        var responseString = await _httpClient.GetStringAsync(uri);
        var response = JsonSerializer.Deserialize<Order>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        return response;
    }

    public async Task<Basket> SetQuantities(AppUser appUser, Dictionary<string, int> quantities)
    {
        var uri = API.Purchase.UpdateBasketItem(_purchaseUrl);
        // potential issue: need to add basket id
        var basketUpdated = new
        {
            BasketId = appUser.Id,
            Updates = quantities.Select(kvp => new
            {
                BasketItemId = kvp.Key,
                NewQuantity = kvp.Value
            }).ToArray()
        };

        var basketContent = new StringContent(JsonSerializer.Serialize(basketUpdated),
            System.Text.Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PutAsync(uri, basketContent);
        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<Basket>(jsonResponse, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public async Task<Basket> UpdateBasket(Basket basket)
    {
        var uri = API.Basket.UpdateBasket(_basketUrl);

        var basketContents = new StringContent(JsonSerializer.Serialize(basket),
            System.Text.Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(uri, basketContents);
        response.EnsureSuccessStatusCode();

        return basket;
    }
}