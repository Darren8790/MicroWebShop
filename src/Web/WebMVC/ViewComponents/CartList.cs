using Microsoft.AspNetCore.Mvc;
using WebMVC.Services;
using WebMVC.ViewModels;

namespace WebMVC.ViewComponents;

public class CartList : ViewComponent
{
    private readonly IBasketService _basketService;

    public CartList(IBasketService basketService)
    {
        _basketService = basketService;
    }

    public async Task<IViewComponentResult> InvokeAsync(AppUser appUser)
    {
        var viewModel = new Basket();
        try
        {
            viewModel = await GetItems(appUser);
            return View(viewModel);
        }
        catch (Exception ex)
        {
            ViewBag.BasketInoperativeMsg = $"Basket service is not working! ({ex.GetType().Name} - {ex.Message}))";
        }
        return View(viewModel);
    }

    private Task<Basket> GetItems(AppUser appUser)
    {
       return _basketService.GetBasket(appUser);
    }
}