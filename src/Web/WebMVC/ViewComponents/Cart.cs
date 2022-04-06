using Microsoft.AspNetCore.Mvc;
using WebMVC.Services;
using WebMVC.ViewModels;
using WebMVC.ViewModels.BasketViewModels;

namespace WebMVC.ViewComponents;

public class Cart : ViewComponent
{
    private readonly IBasketService _basketService;
    public Cart(IBasketService basketService)
    {
        _basketService = basketService;
    }

    public async Task<IViewComponentResult> InvokeAsync(AppUser appUser)
    {
        var viewModel = new BasketComponentViewModel();
        try
        {
            var itemsInCart = await ItemsInCart(appUser);
            viewModel.ItemsCount = itemsInCart;
            return View(viewModel);
        }
        catch
        {
            ViewBag.IsBasketInoperative = true;
        }
        return View(viewModel);
    }

    private async Task<int> ItemsInCart(AppUser appUser)
    {
        var basket = await _basketService.GetBasket(appUser);
        return basket.Items.Count;
    }
}