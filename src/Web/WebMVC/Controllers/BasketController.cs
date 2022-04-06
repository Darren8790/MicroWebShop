using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebMVC.Services;
using WebMVC.ViewModels;

namespace WebMVC.Controllers;
[Authorize(AuthenticationSchemes = OpenIdConnectDefaults.AuthenticationScheme)]
public class BasketController : Controller
{
    private readonly IBasketService _basketService;
    private readonly ICatalogueService _catalogueService;
    private readonly IIdentityParser<AppUser> _appUser;

    //private readonly AppUser _appUser;

    public BasketController(IBasketService basketService, ICatalogueService catalogueService, IIdentityParser<AppUser> appUser) //AppUser appUser)
    {
        _basketService = basketService;
        _catalogueService = catalogueService;
        _appUser = appUser;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var user = _appUser.Parse(HttpContext.User);
            var viewModel = await _basketService.GetBasket(user);
            return View(viewModel);
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Index(Dictionary<string, int> quantities, string actions)
    {
        try
        {
            var user = _appUser.Parse(HttpContext.User);
            var basket = await _basketService.SetQuantities(user, quantities);
            if(actions == "[ Checkout ]")
            {
                return RedirectToAction("Create", "Order");
            }
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
        return View();
    }

    public async Task<IActionResult> AddToBasket(CatalogueItem productDetails)
    {
        try
        {
            if(productDetails?.Id != null)
            {
                var user = _appUser.Parse(HttpContext.User);
                await _basketService.AddItemToBasket(user, productDetails.Id);
            }
            return RedirectToAction("Index", "Catalogue");
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
        return RedirectToAction("Index", "Catalogue");
    }

    private void HandleException(Exception ex)
    {
        ViewBag.BasketInoperativeMsg = $"Basket service is currently inoperative {ex.GetType().Name} - {ex.Message}";
    }
}