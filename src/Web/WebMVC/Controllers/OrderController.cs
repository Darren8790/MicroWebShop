using Microsoft.AspNetCore.Mvc;
using WebMVC.Services;
using WebMVC.ViewModels;

namespace WebMVC.Controllers;

public class OrderController : Controller
{
    private IOrderService _orderService;
    private IBasketService _basketService;
    private readonly IIdentityParser<AppUser> _appUser;

    public OrderController(IOrderService orderService, IBasketService basketService, IIdentityParser<AppUser> appUser)
    {
        _orderService = orderService;
        _basketService = basketService;
        _appUser = appUser;
    }

    // public async Task<IActionResult> Create()
    // {
    //     var user = _appUser.Parse(HttpContext.User);
    //     var order = await _basketService.GetOrderDraft(user.Id);
    //     var viewModel = _orderService.MapUserToOrder(user, order);
    //     return View(viewModel);
    // }

    [HttpPost]
    public async Task<IActionResult> Checkout(Order modelOrder)
    {
        if(ModelState.IsValid)
        {
            var user = _appUser.Parse(HttpContext.User);
            var basket = _orderService.MapOrderToBasket(modelOrder);
            await _basketService.Checkout(basket);
            return RedirectToAction("Index");
        }
        return View("Crate", modelOrder);
    }

    public async Task<IActionResult> CancelOrder(string orderId)
    {
        await _orderService.CancelOrder(orderId);
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> OrderDetails(string orderId)
    {
        var user = _appUser.Parse(HttpContext.User);
        var order = await _orderService.GetOrder(orderId, user);
        return View(order);
    }

    public async Task<IActionResult> Index(Order item)
    {
        var user = _appUser.Parse(HttpContext.User);
        var viewModel = await _orderService.GetOrders(user);
        return View(viewModel);
    }
}